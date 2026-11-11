import { useMemo, useState } from 'react'
import { C } from './ui'
import { money, num } from '../utils/format'
import { industryLabel } from '../utils/industry'

const COLORS = [C.blue, C.purple, C.green, C.amber, C.teal, C.indigo, C.red, '#c25aa8', '#6b7a99', '#8bb33a']
const MAX_SLICES = 8

const GROUPS = [
  ['product', 'Producto', (r) => r.name],
  ['category', 'Categoría', (r) => r.category || 'Sin categoría'],
  ['industry', 'Rubro', (r) => industryLabel(r.industry)],
]
const METRICS = [['amount', 'Monto', money], ['units', 'Unidades', (n) => `${num(n)} u.`]]

const toggleWrap = { display: 'flex', background: '#e9ecf2', borderRadius: 10, padding: 3 }
const toggleBtn = (on) => ({ height: 32, padding: '0 12px', border: 0, borderRadius: 7, background: on ? '#fff' : 'transparent', color: '#172033', fontWeight: 600, fontSize: 13, cursor: 'pointer', boxShadow: on ? '0 1px 3px rgba(0,0,0,.1)' : 'none' })

function Toggle({ options, value, onChange }) {
  return (
    <div style={toggleWrap}>
      {options.map(([k, label]) => <button key={k} type="button" onClick={() => onChange(k)} style={toggleBtn(value === k)}>{label}</button>)}
    </div>
  )
}

const point = (cx, cy, r, a) => [cx + r * Math.cos(a), cy + r * Math.sin(a)]

function slicePath(cx, cy, r, a0, a1) {
  const [x0, y0] = point(cx, cy, r, a0)
  const [x1, y1] = point(cx, cy, r, a1)
  return `M${cx},${cy} L${x0},${y0} A${r},${r} 0 ${a1 - a0 > Math.PI ? 1 : 0} 1 ${x1},${y1} Z`
}

// Torta con la composición de las ventas del gráfico de barras; se puede agrupar y elegir una porción para ver su detalle
export function SalesPie({ rows }) {
  const [groupBy, setGroupBy] = useState('category')
  const [metric, setMetric] = useState('amount')
  const [selected, setSelected] = useState(null)

  const fmt = METRICS.find(([k]) => k === metric)[2]
  const keyOf = GROUPS.find(([k]) => k === groupBy)[2]

  const { slices, total } = useMemo(() => {
    const map = new Map()
    for (const r of rows) {
      const key = keyOf(r)
      const g = map.get(key) ?? { key, value: 0, items: [] }
      g.value += r[metric]
      g.items.push(r)
      map.set(key, g)
    }
    const sorted = [...map.values()].filter((g) => g.value > 0).sort((a, b) => b.value - a.value)
    // Las porciones chicas se juntan en "Otros" para que la torta siga siendo legible
    const head = sorted.slice(0, MAX_SLICES)
    const rest = sorted.slice(MAX_SLICES)
    if (rest.length) head.push({ key: 'Otros', value: rest.reduce((s, g) => s + g.value, 0), items: rest.flatMap((g) => g.items), other: true })
    return { slices: head, total: head.reduce((s, g) => s + g.value, 0) }
  }, [rows, keyOf, metric])

  const change = (setter) => (v) => { setter(v); setSelected(null) }
  const active = slices.find((s) => s.key === selected)

  let angle = -Math.PI / 2
  const arcs = slices.map((s, i) => {
    const sweep = (s.value / total) * Math.PI * 2
    const arc = { ...s, color: s.other ? '#b8c0d0' : COLORS[i % COLORS.length], a0: angle, a1: angle + sweep }
    angle += sweep
    return arc
  })

  const detail = active ? [...active.items].sort((a, b) => b[metric] - a[metric]) : []

  return (
    <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: '20px 22px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 12, flexWrap: 'wrap', marginBottom: 16 }}>
        <div>
          <h3 style={{ margin: 0, fontSize: 17 }}>¿Qué se vendió en esos 12 meses?</h3>
          <div style={{ fontSize: 13, color: '#667085', marginTop: 2 }}>Composición de las ventas del gráfico de barras. Tocá una porción para ver su detalle.</div>
        </div>
        <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
          <Toggle options={GROUPS.map(([k, l]) => [k, l])} value={groupBy} onChange={change(setGroupBy)} />
          <Toggle options={METRICS.map(([k, l]) => [k, l])} value={metric} onChange={change(setMetric)} />
        </div>
      </div>

      {!slices.length ? (
        <div style={{ padding: 30, textAlign: 'center', color: '#667085' }}>No hay ventas en el período.</div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'minmax(220px,300px) minmax(0,1fr)', gap: 24, alignItems: 'start' }}>
          <svg viewBox="0 0 200 200" style={{ width: '100%', maxWidth: 300 }} role="img" aria-label="Gráfico de torta de ventas">
            {arcs.map((a) => {
              const on = a.key === selected
              const dim = selected && !on
              const common = {
                fill: a.color, stroke: '#fff', strokeWidth: 1.5, opacity: dim ? 0.35 : 1,
                style: { cursor: 'pointer', transition: 'opacity .15s' },
                onClick: () => setSelected(on ? null : a.key),
              }
              if (arcs.length === 1) return <circle key={a.key} {...common} cx={100} cy={100} r={on ? 96 : 90}><title>{a.key}</title></circle>
              const mid = (a.a0 + a.a1) / 2
              const [dx, dy] = on ? [Math.cos(mid) * 6, Math.sin(mid) * 6] : [0, 0]
              return <path key={a.key} {...common} d={slicePath(100 + dx, 100 + dy, 90, a.a0, a.a1)}><title>{`${a.key}: ${fmt(a.value)} (${((a.value / total) * 100).toFixed(1)}%)`}</title></path>
            })}
          </svg>

          <div>
            {arcs.map((a) => {
              const on = a.key === selected
              return (
                <button
                  key={a.key}
                  type="button"
                  onClick={() => setSelected(on ? null : a.key)}
                  style={{ display: 'grid', gridTemplateColumns: '14px minmax(0,1fr) auto 56px', gap: 10, alignItems: 'center', width: '100%', padding: '8px 10px', border: 0, borderRadius: 8, background: on ? '#f1f3f9' : 'transparent', cursor: 'pointer', textAlign: 'left', fontSize: 14, color: '#172033' }}
                >
                  <span style={{ width: 14, height: 14, borderRadius: 4, background: a.color }} />
                  <span style={{ fontWeight: on ? 700 : 500, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{a.key}</span>
                  <span style={{ color: '#3b4a66' }}>{fmt(a.value)}</span>
                  <span style={{ textAlign: 'right', color: '#667085' }}>{((a.value / total) * 100).toFixed(1)}%</span>
                </button>
              )
            })}
          </div>
        </div>
      )}

      {active && (
        <div style={{ marginTop: 16, borderTop: '1px solid #eef0f4', paddingTop: 14 }}>
          <div style={{ fontSize: 14, fontWeight: 700, marginBottom: 8 }}>
            {active.key} · {fmt(active.value)} <span style={{ fontWeight: 400, color: '#667085' }}>({detail.length} {detail.length === 1 ? 'producto' : 'productos'})</span>
          </div>
          {detail.map((r) => (
            <div key={r.productId} style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1.5fr) minmax(0,1fr) 90px 130px', gap: 12, padding: '7px 0', fontSize: 14, borderBottom: '1px solid #f1f3f6' }}>
              <strong style={{ fontWeight: 600 }}>{r.name}</strong>
              <span style={{ color: '#667085' }}>{r.category} · {industryLabel(r.industry)}</span>
              <span style={{ textAlign: 'right' }}>{num(r.units)} u.</span>
              <span style={{ textAlign: 'right', color: '#3b4a66' }}>{money(r.amount)}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
