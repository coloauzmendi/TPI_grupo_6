import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/endpoints'
import { useBranches } from '../context/BranchContext'
import { useToast } from '../context/ToastContext'
import { C, Icon, PageHeader, errorBox, tableCard, thStyle } from '../components/ui'
import { money, parseDecimal } from '../utils/format'
import { INDUSTRIES, industryLabel } from '../utils/industry'

const COLS = 'minmax(0,2fr) minmax(0,1fr) 130px 130px 100px'

// "mode" es el valor que espera el backend (enum PriceMode)
const MODES = [
  { mode: 'Individual', icon: 'edit', label: 'Individual', sub: 'Un producto, precio nuevo' },
  { mode: 'Category', icon: 'category', label: 'Por categoría', sub: '% sobre una categoría' },
  { mode: 'Industry', icon: 'storefront', label: 'Por rubro', sub: '% sobre un rubro' },
  { mode: 'All', icon: 'select_all', label: 'Total', sub: '% sobre todos' },
]

export default function Prices() {
  const { branches, reloadAlerts } = useBranches()
  const toast = useToast()
  const navigate = useNavigate()
  const [products, setProducts] = useState([])
  const [mode, setMode] = useState('Category')
  const [target, setTarget] = useState('')
  const [value, setValue] = useState('')
  const [busy, setBusy] = useState(false)

  // Productos con stock activo en alguna sucursal del dueño: son los que alcanza la actualización
  const load = () =>
    Promise.all(branches.map((b) => api.stock(b.id)))
      .then((all) => {
        const byId = new Map()
        all.flat().forEach((i) => byId.set(i.productId, i))
        setProducts([...byId.values()].sort((a, b) => a.name.localeCompare(b.name)))
      })
      .catch((e) => toast(e.message, 'err'))
  useEffect(() => {
    load()
  }, [branches]) // eslint-disable-line react-hooks/exhaustive-deps

  const options =
    mode === 'Individual' ? products.map((p) => ({ value: p.productId, label: `${p.name} — ${p.brand}` }))
      : mode === 'Category' ? [...new Set(products.map((p) => p.category))].sort().map((c) => ({ value: c, label: c }))
        : mode === 'Industry' ? INDUSTRIES
          : []
  const selected = options.some((o) => o.value === target) ? target : options[0]?.value

  const hit = products.filter(
    (p) => mode === 'All' || (mode === 'Individual' && p.productId === selected) || (mode === 'Category' && p.category === selected) || (mode === 'Industry' && p.industry === selected),
  )

  const val = parseDecimal(value)
  let error = null
  if (value.trim() !== '' && isNaN(val)) error = 'Ingresá un valor numérico.'
  const calc = (p) => (value.trim() === '' || error ? p.price : Math.round((mode === 'Individual' ? val : p.price * (1 + val / 100)) * 100) / 100)
  const preview = hit.map((p) => ({ p, next: calc(p) }))
  if (!error && preview.some((r) => r.next < 0)) error = 'Ningún precio puede quedar menor a 0. Revisá el valor ingresado.'
  const changes = preview.filter((r) => r.next !== r.p.price)
  const disabled = !!error || value.trim() === '' || !changes.length || busy

  const pickMode = (m) => { setMode(m); setTarget(''); setValue('') }

  const confirm = async () => {
    setBusy(true)
    try {
      const n = await api.updatePrices({ mode, target: mode === 'All' ? null : selected, value: val })
      setValue('')
      toast(`Se actualizaron ${n} ${n === 1 ? 'precio' : 'precios'}`)
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
      <PageHeader title="Actualizar precios" subtitle="Las ventas ya registradas conservan su precio original." />

      <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: 22, display: 'flex', flexDirection: 'column', gap: 18 }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,minmax(0,1fr))', gap: 10 }}>
          {MODES.map((m) => (
            <button
              key={m.mode}
              onClick={() => pickMode(m.mode)}
              style={{ padding: 14, borderRadius: 12, border: `2px solid ${mode === m.mode ? C.amber : '#e6e9ef'}`, background: mode === m.mode ? '#fdf5e8' : '#fff', color: '#172033', cursor: 'pointer', textAlign: 'left', display: 'flex', flexDirection: 'column', gap: 4 }}
            >
              <Icon name={m.icon} style={{ fontSize: 24, color: C.amber }} />
              <span style={{ fontWeight: 700, fontSize: 15 }}>{m.label}</span>
              <span style={{ fontSize: 12, color: '#667085' }}>{m.sub}</span>
            </button>
          ))}
        </div>

        <div style={{ display: 'flex', gap: 14, flexWrap: 'wrap', alignItems: 'flex-end' }}>
          {mode !== 'All' && (
            <label style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: 13, fontWeight: 600, color: '#3b4a66', flex: 1, minWidth: 240 }}>
              {mode === 'Individual' ? 'Producto' : mode === 'Category' ? 'Categoría' : 'Rubro'}
              <select value={selected ?? ''} onChange={(e) => setTarget(e.target.value)} style={{ height: 46, padding: '0 12px', border: '1px solid #d5dae3', borderRadius: 10, fontSize: 15, background: '#fff' }}>
                {options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </label>
          )}
          <label style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: 13, fontWeight: 600, color: '#3b4a66', width: 260 }}>
            {mode === 'Individual' ? 'Precio nuevo ($)' : 'Porcentaje (negativo = descuento)'}
            <input
              value={value}
              onChange={(e) => setValue(e.target.value)}
              inputMode="decimal"
              placeholder={mode === 'Individual' ? 'Ej.: 1650' : 'Ej.: 12 o -5'}
              style={{ height: 46, padding: '0 12px', border: '1px solid #d5dae3', borderRadius: 10, fontSize: 16 }}
            />
          </label>
          <div style={{ marginLeft: 'auto', fontSize: 14, color: '#667085', paddingBottom: 12 }}>
            {hit.length} {hit.length === 1 ? 'producto alcanzado' : 'productos alcanzados'}
          </div>
        </div>
        {error && <div style={errorBox}>{error}</div>}
      </div>

      <div style={{ ...tableCard, overflow: 'hidden' }}>
        <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, padding: '12px 18px', borderBottom: '1px solid #eef0f4', ...thStyle }}>
          <span>Producto</span><span>Categoría</span>
          <span style={{ textAlign: 'right' }}>Precio actual</span><span style={{ textAlign: 'right' }}>Precio nuevo</span><span style={{ textAlign: 'right' }}>Variación</span>
        </div>
        {preview.length === 0 && (
          <div style={{ padding: 24, textAlign: 'center', color: '#667085' }}>
            {mode === 'Industry' ? 'El rubro elegido no tiene productos con stock en tus sucursales.' : 'No hay productos alcanzados.'}
          </div>
        )}
        <div style={{ maxHeight: 420, overflow: 'auto' }}>
          {preview.map(({ p, next }) => {
            const d = next - p.price
            return (
              <div key={p.productId} style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, padding: '12px 18px', fontSize: 15, borderBottom: '1px solid #f1f3f6', alignItems: 'center' }}>
                <span><strong style={{ fontWeight: 600 }}>{p.name}</strong> <span style={{ color: '#667085', fontSize: 13 }}>{p.brand}</span></span>
                <span style={{ color: '#3b4a66' }}>{p.category}{mode === 'Industry' ? '' : ` · ${industryLabel(p.industry)}`}</span>
                <span style={{ textAlign: 'right', color: '#667085' }}>{money(p.price)}</span>
                <span style={{ textAlign: 'right', fontWeight: 700 }}>{money(next)}</span>
                <span style={{ textAlign: 'right', fontWeight: 600, color: d > 0 ? C.green : d < 0 ? C.red : '#98a1b3' }}>
                  {d === 0 ? '—' : `${d > 0 ? '+' : ''}${((d / p.price) * 100).toFixed(1).replace('.', ',')}%`}
                </span>
              </div>
            )
          })}
        </div>
      </div>

      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
        <button onClick={() => navigate('/')} style={{ height: 50, padding: '0 22px', border: '1px solid #d5dae3', borderRadius: 12, background: '#fff', fontSize: 15, fontWeight: 600, cursor: 'pointer' }}>Cancelar</button>
        <button onClick={confirm} disabled={disabled} style={{ height: 50, padding: '0 26px', border: 0, borderRadius: 12, background: C.amber, color: '#fff', fontSize: 16, fontWeight: 700, cursor: 'pointer', opacity: disabled ? 0.5 : 1 }}>
          {busy ? 'Actualizando…' : `Confirmar actualización${changes.length && !error ? ` (${changes.length})` : ''}`}
        </button>
      </div>
    </div>
  )
}
