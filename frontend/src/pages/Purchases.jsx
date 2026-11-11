import { useCallback, useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/endpoints'
import { useBranches } from '../context/BranchContext'
import { useToast } from '../context/ToastContext'
import { PurchaseReceipt } from '../components/Receipts'
import { Icon, Pill, h1, stockBadge, subtitle } from '../components/ui'
import { branchLabel, money, parseDecimal } from '../utils/format'

const TEAL = '#1b9688'

export default function Purchases() {
  const { active, reloadAlerts } = useBranches()
  const toast = useToast()
  const navigate = useNavigate()
  const location = useLocation()
  const [items, setItems] = useState([])
  const [products, setProducts] = useState([])
  const [q, setQ] = useState('')
  const [supplier, setSupplier] = useState('')
  const [lines, setLines] = useState([])
  const [busy, setBusy] = useState(false)
  const [receipt, setReceipt] = useState(null)

  const load = useCallback(() => {
    if (!active) return
    api.stock(active.id).then(setItems).catch((e) => toast(e.message, 'err'))
    api.products().then(setProducts).catch(() => {})
  }, [active, toast])
  useEffect(load, [load])

  // Si se llega desde una alerta de stock bajo, el producto ya viene cargado en la compra
  const fromAlert = location.state?.productId
  useEffect(() => {
    setLines(fromAlert ? [{ productId: fromAlert, quantity: '', cost: '' }] : [])
  }, [active?.id, fromAlert])

  const inStock = Object.fromEntries(items.map((i) => [i.productId, i]))
  const term = q.toLowerCase().trim()
  // Productos de la sucursal + productos del mismo rubro que todavía no tienen alta en ella
  const catalog = [
    ...items,
    ...products
      .filter((p) => p.industry === active?.industry && !inStock[p.id])
      .map((p) => ({ productId: p.id, name: p.name, brand: p.brand, notRegistered: true })),
  ]
    .filter((p) => !term || `${p.name} ${p.brand}`.toLowerCase().includes(term))
    .sort((a, b) => a.name.localeCompare(b.name))

  const setLine = (productId, key, value) => setLines(lines.map((l) => (l.productId === productId ? { ...l, [key]: value } : l)))
  const removeLine = (productId) => setLines(lines.filter((l) => l.productId !== productId))

  let total = 0
  let incomplete = false
  const rows = lines
    .filter((l) => inStock[l.productId])
    .map((l) => {
      const n = Number(l.quantity)
      const cost = parseDecimal(l.cost)
      let error = null
      if (l.quantity !== '' && (!Number.isInteger(n) || n <= 0)) error = 'La cantidad debe ser un entero mayor a 0'
      else if (l.cost !== '' && (isNaN(cost) || cost < 0)) error = 'El costo debe ser un número ≥ 0'
      const complete = l.quantity !== '' && l.cost !== '' && !error
      if (!complete) incomplete = true
      else total += n * cost
      return { l, item: inStock[l.productId], n, cost, error, complete }
    })

  const cancel = () => {
    if (lines.length) toast('Compra cancelada. No se modificó el stock.')
    navigate('/')
  }

  const confirm = async () => {
    setBusy(true)
    try {
      const purchase = await api.createPurchase(active.id, {
        supplier: supplier.trim() || null,
        items: rows.map((r) => ({ productId: r.l.productId, quantity: r.n, unitCost: r.cost })),
      })
      setLines([])
      setSupplier('')
      setReceipt(purchase)
      load()
      reloadAlerts()
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
    }
  }

  const disabled = !rows.length || incomplete || busy

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1.3fr) minmax(360px,1fr)', gap: 24, minHeight: 560 }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 14, minHeight: 0 }}>
        <div>
          <h1 style={h1}>Registrar Stock</h1>
          <p style={subtitle}>Ingresa en: {branchLabel(active)}</p>
        </div>
        <input
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder="Buscar producto de la sucursal…"
          style={{ height: 50, padding: '0 16px', border: '1px solid #e1e5ec', borderRadius: 12, fontSize: 16, background: '#fff' }}
        />
        <div style={{ flex: 1, overflow: 'auto', display: 'flex', flexDirection: 'column', gap: 8, maxHeight: 560 }}>
          {catalog.map((p) => {
            const added = lines.some((l) => l.productId === p.productId)
            const badge = p.notRegistered
              ? { text: 'Sin alta', bg: '#eceef2', fg: '#6b7385' }
              : added
                ? { text: 'Agregado', bg: '#e3f4f2', fg: '#106b61' }
                : stockBadge(p.quantity, p.minimumStock)
            return (
              <button
                key={p.productId}
                disabled={p.notRegistered}
                onClick={() => !added && setLines([...lines, { productId: p.productId, quantity: '', cost: '' }])}
                style={{ display: 'flex', alignItems: 'center', gap: 14, padding: '12px 16px', background: '#fff', border: '1px solid #e6e9ef', borderRadius: 12, cursor: p.notRegistered ? 'not-allowed' : 'pointer', opacity: p.notRegistered ? 0.55 : 1, textAlign: 'left' }}
              >
                <span style={{ flex: 1, minWidth: 0 }}>
                  <span style={{ display: 'block', fontSize: 16, fontWeight: 600, color: '#172033' }}>{p.name}</span>
                  <span style={{ display: 'block', fontSize: 13, color: '#667085' }}>
                    {p.notRegistered ? 'No dado de alta en esta sucursal — cargalo desde Stock' : `${p.brand} · stock actual ${p.quantity}`}
                  </span>
                </span>
                <Pill bg={badge.bg} fg={badge.fg}>{badge.text}</Pill>
              </button>
            )
          })}
        </div>
      </div>

      <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: 20, display: 'flex', flexDirection: 'column', gap: 14 }}>
        <h3 style={{ margin: 0, fontSize: 19 }}>Detalle de la compra</h3>
        <label style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: 13, fontWeight: 600, color: '#3b4a66' }}>
          Proveedor (opcional)
          <input
            value={supplier}
            onChange={(e) => setSupplier(e.target.value)}
            placeholder="Ej.: Distribuidora del Sur"
            style={{ height: 42, padding: '0 12px', border: '1px solid #d5dae3', borderRadius: 9, fontSize: 15 }}
          />
        </label>
        <div style={{ flex: 1, overflow: 'auto', display: 'flex', flexDirection: 'column', gap: 10, background: '#f6f7fa', borderRadius: 12, padding: 10, minHeight: 200 }}>
          {rows.length === 0 && <div style={{ margin: 'auto', textAlign: 'center', color: '#8a93a6', fontSize: 15, padding: 24 }}>Agregá productos desde la lista</div>}
          {rows.map(({ l, item, n, cost, error, complete }) => (
            <div key={l.productId} style={{ background: '#fff', borderRadius: 10, padding: 12, display: 'flex', flexDirection: 'column', gap: 8 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                <div>
                  <div style={{ fontWeight: 600 }}>{item.name}</div>
                  <div style={{ fontSize: 13, color: '#667085' }}>Stock actual: {item.quantity}</div>
                </div>
                <button onClick={() => removeLine(l.productId)} aria-label="Quitar" style={{ border: 0, background: 'none', color: '#dc3a3a', cursor: 'pointer', height: 28 }}>
                  <Icon name="delete" style={{ fontSize: 22 }} />
                </button>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr auto', gap: 8, alignItems: 'end' }}>
                <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 12, color: '#667085' }}>
                  Cantidad
                  <input value={l.quantity} onChange={(e) => setLine(l.productId, 'quantity', e.target.value)} inputMode="numeric" placeholder="0" style={{ height: 38, padding: '0 10px', border: '1px solid #d5dae3', borderRadius: 8, fontSize: 15 }} />
                </label>
                <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 12, color: '#667085' }}>
                  Costo unitario ($)
                  <input value={l.cost} onChange={(e) => setLine(l.productId, 'cost', e.target.value)} inputMode="decimal" placeholder="0,00" style={{ height: 38, padding: '0 10px', border: '1px solid #d5dae3', borderRadius: 8, fontSize: 15 }} />
                </label>
                <strong style={{ width: 90, textAlign: 'right' }}>{complete ? money(n * cost) : '—'}</strong>
              </div>
              {error && <div style={{ fontSize: 13, color: '#b42323', background: '#fdecec', padding: '6px 10px', borderRadius: 7 }}>{error}</div>}
            </div>
          ))}
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', borderTop: '1px solid #eef0f4', paddingTop: 14 }}>
          <span style={{ fontSize: 17, fontWeight: 700 }}>TOTAL</span>
          <span style={{ fontSize: 30, fontWeight: 800, color: TEAL }}>{money(total)}</span>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 10 }}>
          <button onClick={cancel} className="hv-danger" style={{ height: 52, border: '1px solid #d5dae3', borderRadius: 12, background: '#fff', fontSize: 15, fontWeight: 600, cursor: 'pointer' }}>Cancelar</button>
          <button onClick={confirm} disabled={disabled} style={{ height: 52, border: 0, borderRadius: 12, background: TEAL, color: '#fff', fontSize: 16, fontWeight: 700, cursor: 'pointer', opacity: disabled ? 0.5 : 1 }}>
            {busy ? 'Registrando…' : 'Confirmar compra'}
          </button>
        </div>
      </div>

      {receipt && <PurchaseReceipt purchase={receipt} branch={active} onClose={() => setReceipt(null)} />}
    </div>
  )
}
