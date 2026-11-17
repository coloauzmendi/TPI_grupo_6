import { useState } from 'react'
import { api } from '../api/endpoints'
import { useToast } from '../context/ToastContext'
import { Modal } from './Modal'
import { C, Icon, errorBox, outlineBtn } from './ui'
import { MESES, money, parseDecimal } from '../utils/format'
import { INDUSTRIES } from '../utils/industry'

const SCOPES = [
  { mode: 'Individual', icon: 'edit', label: 'Un producto' },
  { mode: 'Category', icon: 'category', label: 'Una categoría' },
  { mode: 'Industry', icon: 'storefront', label: 'Un rubro' },
  { mode: 'All', icon: 'select_all', label: 'Todos' },
]

// Aplica la inflación publicada (editable) al alcance elegido: producto, categoría, rubro o todos
export function InflationModal({ inflation, products, onClose, onApplied }) {
  const toast = useToast()
  const [mode, setMode] = useState('All')
  const [target, setTarget] = useState('')
  const [percent, setPercent] = useState(String(inflation.percent).replace('.', ','))
  const [busy, setBusy] = useState(false)

  const options =
    mode === 'Individual' ? products.map((p) => ({ value: p.productId, label: `${p.name} — ${p.brand}` }))
      : mode === 'Category' ? [...new Set(products.map((p) => p.category))].sort().map((c) => ({ value: c, label: c }))
        : mode === 'Industry' ? INDUSTRIES
          : []
  const selected = options.some((o) => o.value === target) ? target : options[0]?.value

  const hit = products.filter(
    (p) => mode === 'All' || (mode === 'Individual' && p.productId === selected) || (mode === 'Category' && p.category === selected) || (mode === 'Industry' && p.industry === selected),
  )
  const pct = parseDecimal(percent)
  const invalid = percent.trim() === '' || isNaN(pct)
  const raise = (price) => Math.round(price * (1 + pct / 100) * 100) / 100
  const negative = !invalid && hit.some((p) => raise(p.price) < 0)
  const error = percent.trim() !== '' && isNaN(pct) ? 'Ingresá un porcentaje numérico.' : negative ? 'Ningún precio puede quedar menor a 0.' : null
  const disabled = invalid || !!error || !hit.length || busy
  const sample = hit[0]

  const apply = async () => {
    setBusy(true)
    try {
      // El back espera, en modo Individual, el precio nuevo y no el porcentaje
      const body = mode === 'Individual'
        ? { mode, target: selected, value: raise(sample.price) }
        : { mode, target: mode === 'All' ? null : selected, value: pct }
      const n = await api.updatePrices(body)
      toast(`Se actualizaron ${n} ${n === 1 ? 'precio' : 'precios'} (${percent}%)`)
      onApplied()
      onClose()
    } catch (e) {
      toast(e.message, 'err')
      setBusy(false)
    }
  }

  const label = { display: 'flex', flexDirection: 'column', gap: 6, fontSize: 13, fontWeight: 600, color: '#3b4a66' }
  const box = { height: 44, padding: '0 12px', border: '1px solid #d5dae3', borderRadius: 10, fontSize: 15, background: '#fff' }

  return (
    <Modal title="Actualizar precios por inflación" onClose={onClose} width={560}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, background: '#fdf5e8', border: `1px solid ${C.amber}`, borderRadius: 12, padding: '12px 14px' }}>
        <Icon name="trending_up" style={{ fontSize: 26, color: C.amber }} />
        <span style={{ fontSize: 14, color: '#5b6579' }}>
          Inflación de {MESES[inflation.month - 1]} de {inflation.year} (INDEC): <strong style={{ color: '#172033' }}>{inflation.percent.toFixed(2).replace('.', ',')}%</strong>
        </span>
      </div>

      <div style={{ ...label }}>
        ¿A qué querés aplicarla?
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,minmax(0,1fr))', gap: 8 }}>
          {SCOPES.map((s) => (
            <button
              key={s.mode}
              onClick={() => { setMode(s.mode); setTarget('') }}
              style={{ padding: '10px 6px', borderRadius: 10, border: `2px solid ${mode === s.mode ? C.amber : '#e6e9ef'}`, background: mode === s.mode ? '#fdf5e8' : '#fff', color: '#172033', cursor: 'pointer', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4, fontSize: 13, fontWeight: 600 }}
            >
              <Icon name={s.icon} style={{ fontSize: 22, color: C.amber }} />
              {s.label}
            </button>
          ))}
        </div>
      </div>

      {mode !== 'All' && (
        <label style={label}>
          {mode === 'Individual' ? 'Producto' : mode === 'Category' ? 'Categoría' : 'Rubro'}
          <select value={selected ?? ''} onChange={(e) => setTarget(e.target.value)} style={box}>
            {options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </label>
      )}

      <label style={label}>
        Porcentaje de aumento (podés ajustarlo)
        <input value={percent} onChange={(e) => setPercent(e.target.value)} inputMode="decimal" style={box} />
      </label>

      {error && <div style={errorBox}>{error}</div>}

      <div style={{ fontSize: 14, color: '#3b4a66', lineHeight: 1.5 }}>
        Se actualizarán <strong>{hit.length} {hit.length === 1 ? 'producto' : 'productos'}</strong>.
        {sample && !invalid && !error && <> Ejemplo: {sample.name} pasa de {money(sample.price)} a <strong>{money(raise(sample.price))}</strong>.</>}
        {' '}Las ventas ya registradas conservan su precio.
      </div>

      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
        <button onClick={onClose} style={outlineBtn}>Cancelar</button>
        <button
          onClick={apply}
          disabled={disabled}
          style={{ height: 46, padding: '0 22px', border: 0, borderRadius: 10, background: C.amber, color: '#fff', fontWeight: 700, cursor: 'pointer', opacity: disabled ? 0.5 : 1 }}
        >
          {busy ? 'Actualizando…' : 'Aplicar aumento'}
        </button>
      </div>
    </Modal>
  )
}
