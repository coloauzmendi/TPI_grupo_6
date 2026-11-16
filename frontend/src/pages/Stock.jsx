import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useAuth } from '../context/AuthContext'
import { useBranches } from '../context/BranchContext'
import { useToast } from '../context/ToastContext'
import { ConfirmModal, FormModal } from '../components/Modal'
import { Empty, Icon, PageHeader, Pill, iconBtn, inputBox, primaryBtn, tableCard, thStyle } from '../components/ui'
import { branchLabel, money, parseDecimal } from '../utils/format'
import { INDUSTRIES, industryLabel } from '../utils/industry'

const COLS = 'minmax(0,2.2fr) minmax(0,1fr) minmax(0,1fr) 110px 80px 80px 120px 96px'

function statusOf(i) {
  if (i.quantity === 0) return { text: 'Sin stock', bg: '#eceef2', fg: '#6b7385' }
  if (i.quantity <= i.minimumStock) return { text: 'Stock bajo', bg: '#fdf0db', fg: '#a3620a' }
  return { text: 'OK', bg: '#e3f4ea', fg: '#17783f' }
}

const COST_TYPES = [{ value: 'unit', label: 'Costo por unidad' }, { value: 'total', label: 'Costo total' }]

// En el alta y la reactivación se informa el costo de la mercadería cargada
const COST_FIELDS = [
  { key: 'costType', label: 'Tipo de costo', options: COST_TYPES },
  { key: 'cost', label: 'Importe ($)', placeholder: '0,00' },
]

const PRODUCT_FIELDS = (editing) => [
  { key: 'name', label: 'Nombre', full: true, placeholder: 'Ej.: Coca-Cola 500ml' },
  { key: 'brand', label: 'Marca' },
  { key: 'category', label: 'Categoría', list: 'dl-cats', placeholder: 'Ej.: Bebidas' },
  { key: 'industry', label: 'Rubro', options: INDUSTRIES },
  { key: 'price', label: 'Precio ($)', placeholder: '0,00' },
  { key: 'quantity', label: editing ? 'Cantidad en stock' : 'Cantidad inicial' },
  { key: 'minimumStock', label: 'Stock mínimo' },
  ...(editing ? [] : COST_FIELDS),
]

const STOCK_FIELDS = [
  { key: 'quantity', label: 'Cantidad inicial' },
  { key: 'minimumStock', label: 'Stock mínimo' },
  ...COST_FIELDS,
]

const productKey = (p) => `${p.name.trim().toLowerCase()}|${p.brand.trim().toLowerCase()}`

export default function Stock() {
  const { isOwner } = useAuth()
  const { active, reloadAlerts } = useBranches()
  const toast = useToast()
  const [items, setItems] = useState([])
  const [q, setQ] = useState('')
  const [cat, setCat] = useState('')
  const [lowOnly, setLowOnly] = useState(false)
  const [showInactive, setShowInactive] = useState(false)
  const [form, setForm] = useState(null) // { id?, reactivate?, values, errors }
  const [confirm, setConfirm] = useState(null)
  const [busy, setBusy] = useState(false)

  const withInactive = isOwner && showInactive
  const load = useCallback(() => {
    if (active) api.stock(active.id, withInactive).then(setItems).catch((e) => toast(e.message, 'err'))
  }, [active, toast, withInactive])
  useEffect(load, [load])

  const term = q.toLowerCase().trim()
  const rows = items.filter(
    (i) => (!term || `${i.name} ${i.brand}`.toLowerCase().includes(term)) && (!cat || i.category === cat) && (!lowOnly || i.quantity <= i.minimumStock),
  )
  const categories = [...new Set(items.map((i) => i.category))].sort()

  const openNew = () =>
    setForm({ values: { name: '', brand: '', category: '', industry: active.industry, price: '', quantity: '1', minimumStock: '5', costType: 'unit', cost: '' }, errors: {} })
  const openEdit = (i) =>
    setForm({
      id: i.productId,
      values: { name: i.name, brand: i.brand, category: i.category, industry: i.industry, price: String(i.price), quantity: String(i.quantity), minimumStock: String(i.minimumStock) },
      errors: {},
    })
  const openReactivate = (i) =>
    setForm({ reactivate: i, values: { quantity: '1', minimumStock: String(i.minimumStock), costType: 'unit', cost: '' }, errors: {} })

  const change = (key, value) => setForm((f) => ({ ...f, values: { ...f.values, [key]: value }, errors: { ...f.errors, [key]: null } }))

  const validate = (v) => {
    const e = {}
    if (!form.reactivate) {
      Object.entries({ name: 100, brand: 50, category: 50 }).forEach(([k, max]) => {
        if (!v[k].trim()) e[k] = 'Campo obligatorio'
        else if (v[k].trim().length > max) e[k] = `Máximo ${max} caracteres`
      })
      const price = parseDecimal(v.price)
      if (isNaN(price) || price < 0.01 || price > 99999999.99) e.price = 'Ingresá un precio entre 0,01 y 99.999.999,99'
    }
    // En el alta y la reactivación la cantidad mínima es 1; al editar puede quedar en 0
    const minQuantity = form.id ? 0 : 1
    ;[['quantity', minQuantity], ['minimumStock', 0]].forEach(([k, min]) => {
      const n = Number(v[k])
      if (v[k].trim() === '' || !Number.isInteger(n) || n < min || n > 100000) e[k] = `Ingresá un número entero entre ${min} y 100.000`
    })
    if (!form.id) {
      const cost = parseDecimal(v.cost)
      const max = v.costType === 'total' ? 9999999999.99 : 99999999.99
      if (isNaN(cost) || cost < 0.01 || cost > max) e.cost = `Ingresá un importe entre 0,01 y ${v.costType === 'total' ? '9.999.999.999,99' : '99.999.999,99'}`
    }
    return e
  }

  const costOf = (v) => (v.costType === 'total' ? { totalCost: parseDecimal(v.cost) } : { unitCost: parseDecimal(v.cost) })

  const save = async () => {
    const errors = validate(form.values)
    if (Object.keys(errors).length) return setForm({ ...form, errors })

    const v = form.values
    const stock = { quantity: Number(v.quantity), minimumStock: Number(v.minimumStock) }

    setBusy(true)
    try {
      let message
      if (form.reactivate) {
        await api.createStock(active.id, { productId: form.reactivate.productId, ...stock, ...costOf(v) })
        message = 'Producto reactivado'
      } else {
        const product = { name: v.name.trim(), brand: v.brand.trim(), category: v.category.trim(), price: parseDecimal(v.price), industry: v.industry }
        if (form.id) {
          await api.updateProduct(form.id, product)
          await api.updateStock(active.id, form.id, stock)
          message = 'Cambios guardados'
        } else {
          const existing = (await api.products(active.companyId)).find((p) => productKey(p) === productKey(product))
          if (existing) {
            if (items.some((i) => i.productId === existing.id && i.isActive)) throw new Error('Ese producto ya está cargado en esta sucursal')
            await api.createStock(active.id, { productId: existing.id, ...stock, ...costOf(v) })
            message = 'El producto ya existía: se cargó el stock con el precio vigente'
          } else {
            const created = await api.createProduct({ ...product, companyId: active.companyId })
            await api.createStock(active.id, { productId: created.id, ...stock, ...costOf(v) })
            message = 'Alta realizada correctamente'
          }
        }
      }
      setForm(null)
      toast(message)
      load()
      reloadAlerts()
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
    }
  }

  const remove = async (item) => {
    setBusy(true)
    try {
      await api.deleteStock(active.id, item.productId)
      setConfirm(null)
      toast('Producto dado de baja')
      load()
      reloadAlerts()
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
      <PageHeader title={isOwner ? 'Gestionar stock' : 'Stock de tu sucursal'} subtitle={branchLabel(active)}>
        {isOwner && (
          <button onClick={openNew} className="hv-primary" style={primaryBtn}>
            <Icon name="add" style={{ fontSize: 20 }} />Agregar producto
          </button>
        )}
      </PageHeader>

      <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', alignItems: 'center' }}>
        <input value={q} onChange={(e) => setQ(e.target.value)} placeholder="Buscar producto…" style={{ ...inputBox, flex: 1, minWidth: 220 }} />
        <select value={cat} onChange={(e) => setCat(e.target.value)} style={{ ...inputBox, padding: '0 12px' }}>
          <option value="">Todas las categorías</option>
          {categories.map((c) => <option key={c} value={c}>{c}</option>)}
        </select>
        <button
          onClick={() => setLowOnly(!lowOnly)}
          style={{ height: 44, padding: '0 14px', borderRadius: 10, border: `1px solid ${lowOnly ? '#e8961c' : '#e1e5ec'}`, background: lowOnly ? '#fdf0db' : '#fff', color: lowOnly ? '#a3620a' : '#3b4a66', fontSize: 14, fontWeight: 600, cursor: 'pointer' }}
        >
          Solo stock bajo
        </button>
        {isOwner && (
          <button
            onClick={() => setShowInactive(!showInactive)}
            style={{ height: 44, padding: '0 14px', borderRadius: 10, border: `1px solid ${showInactive ? '#1f66e0' : '#e1e5ec'}`, background: showInactive ? '#eef2f8' : '#fff', color: showInactive ? '#1f66e0' : '#3b4a66', fontSize: 14, fontWeight: 600, cursor: 'pointer' }}
          >
            Mostrar dados de baja
          </button>
        )}
      </div>

      <div style={tableCard}>
        <div style={{ minWidth: 760 }}>
          <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, padding: '12px 18px', borderBottom: '1px solid #eef0f4', ...thStyle }}>
            <span>Producto</span><span>Categoría</span><span>Rubro</span>
            <span style={{ textAlign: 'right' }}>Precio</span><span style={{ textAlign: 'right' }}>Stock</span><span style={{ textAlign: 'right' }}>Mínimo</span>
            <span>Estado</span><span />
          </div>
          {rows.length === 0 && <Empty>No hay productos para mostrar.</Empty>}
          {rows.map((i) => {
            const s = i.isActive ? statusOf(i) : { text: 'Inactivo', bg: '#eceef2', fg: '#6b7385' }
            const dim = i.isActive ? {} : { opacity: 0.5 }
            return (
              <div key={i.productId} style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, padding: '13px 18px', alignItems: 'center', fontSize: 15, borderBottom: '1px solid #f1f3f6' }}>
                <span style={dim}>
                  <strong style={{ fontWeight: 600 }}>{i.name}</strong>
                  <span style={{ display: 'block', fontSize: 13, color: '#667085' }}>{i.brand}</span>
                </span>
                <span style={{ color: '#3b4a66', ...dim }}>{i.category}</span>
                <span style={{ color: '#3b4a66', ...dim }}>{industryLabel(i.industry)}</span>
                <span style={{ textAlign: 'right', fontWeight: 600, ...dim }}>{money(i.price)}</span>
                <span style={{ textAlign: 'right', fontWeight: 700, ...dim }}>{i.quantity}</span>
                <span style={{ textAlign: 'right', color: '#667085', ...dim }}>{i.minimumStock}</span>
                <span><Pill bg={s.bg} fg={s.fg} style={{ fontSize: 12, padding: '4px 10px' }}>{s.text}</Pill></span>
                <span style={{ display: 'flex', gap: 4, justifyContent: 'flex-end' }}>
                  {isOwner && !i.isActive && (
                    <button
                      onClick={() => openReactivate(i)}
                      style={{ height: 36, padding: '0 12px', border: 0, borderRadius: 10, background: '#eef2f8', color: '#1f66e0', fontSize: 14, fontWeight: 600, cursor: 'pointer' }}
                    >
                      Reactivar
                    </button>
                  )}
                  {isOwner && i.isActive && (
                    <>
                      <button title="Editar" onClick={() => openEdit(i)} style={iconBtn('#eef2f8', '#1f66e0')}><Icon name="edit" style={{ fontSize: 20 }} /></button>
                      <button
                        title="Eliminar"
                        onClick={() => setConfirm(i)}
                        style={iconBtn('#fdecec', '#dc3a3a')}
                      >
                        <Icon name="delete" style={{ fontSize: 20 }} />
                      </button>
                    </>
                  )}
                </span>
              </div>
            )
          })}
        </div>
      </div>

      {form && (
        <FormModal
          title={form.reactivate ? `Reactivar ${form.reactivate.name}` : form.id ? 'Editar producto' : 'Agregar producto'}
          fields={form.reactivate ? STOCK_FIELDS : PRODUCT_FIELDS(!!form.id)}
          values={form.values}
          errors={form.errors}
          onChange={change}
          onSave={save}
          onClose={() => setForm(null)}
          busy={busy}
          categories={categories}
        />
      )}
      {confirm && (
        <ConfirmModal
          title="Dar de baja producto"
          text={`¿Querés dar de baja "${confirm.name}" en ${branchLabel(active)}? Dejará de aparecer en el listado activo.`}
          actionLabel="Dar de baja"
          busy={busy}
          onConfirm={() => remove(confirm)}
          onClose={() => setConfirm(null)}
        />
      )}
    </div>
  )
}
