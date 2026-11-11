import { useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useToast } from '../context/ToastContext'
import { PeriodFilters, scopeParams } from '../components/PeriodFilters'
import { C, PageHeader } from '../components/ui'
import { MESES, cap, money, num } from '../utils/format'
import { industryLabel } from '../utils/industry'

export default function Ranking() {
  const toast = useToast()
  const now = new Date()
  const [filters, setFilters] = useState({ scope: 'all', month: now.getMonth() + 1, year: now.getFullYear() })
  const [order, setOrder] = useState('most')
  const [data, setData] = useState(null)

  useEffect(() => {
    api.ranking({ ...scopeParams(filters.scope), year: filters.year, month: filters.month, order }).then(setData).catch((e) => toast(e.message, 'err'))
  }, [filters, order, toast])

  const rows = data?.rows ?? []
  const max = Math.max(1, ...rows.map((r) => r.units))
  const empty = data && data.kpi.sales === 0
  const kpis = data
    ? [{ label: 'Ventas del período', value: num(data.kpi.sales) }, { label: 'Unidades vendidas', value: num(data.kpi.units) }, { label: 'Monto vendido', value: money(data.kpi.total) }]
    : []

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
      <PageHeader title="Ranking de productos" subtitle={`${cap(MESES[filters.month - 1])} ${filters.year} · ordenado por unidades vendidas`}>
        <div style={{ display: 'flex', background: '#e9ecf2', borderRadius: 10, padding: 4 }}>
          {[['most', 'Más vendidos'], ['least', 'Menos vendidos']].map(([k, label]) => (
            <button
              key={k}
              onClick={() => setOrder(k)}
              style={{ height: 36, padding: '0 14px', border: 0, borderRadius: 7, background: order === k ? '#fff' : 'transparent', color: '#172033', fontWeight: 600, fontSize: 14, cursor: 'pointer', boxShadow: order === k ? '0 1px 3px rgba(0,0,0,.1)' : 'none' }}
            >
              {label}
            </button>
          ))}
        </div>
      </PageHeader>

      <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        <PeriodFilters value={filters} onChange={setFilters} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit,minmax(180px,1fr))', gap: 14 }}>
        {kpis.map((k) => (
          <div key={k.label} style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 14, padding: '16px 18px' }}>
            <div style={{ fontSize: 13, color: '#667085' }}>{k.label}</div>
            <div style={{ fontSize: 24, fontWeight: 800, marginTop: 2 }}>{k.value}</div>
          </div>
        ))}
      </div>

      <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 14, padding: '8px 20px 14px' }}>
        {empty && <div style={{ padding: 40, textAlign: 'center', color: '#667085', fontSize: 16 }}>No hay ventas registradas para el período seleccionado.</div>}
        {!empty && rows.map((r, i) => {
          const top = i < 3 && order === 'most'
          return (
            <div key={r.productId} style={{ display: 'grid', gridTemplateColumns: '40px minmax(0,1.4fr) minmax(0,2fr) 90px 130px', gap: 14, alignItems: 'center', padding: '11px 0', borderBottom: '1px solid #f1f3f6', fontSize: 15 }}>
              <span style={{ width: 30, height: 30, borderRadius: 8, background: top ? C.blue : '#eef0f4', color: top ? '#fff' : '#3b4a66', display: 'grid', placeItems: 'center', fontWeight: 800, fontSize: 14 }}>{i + 1}</span>
              <span>
                <strong style={{ fontWeight: 600 }}>{r.name}</strong>
                <span style={{ display: 'block', fontSize: 12, color: '#667085' }}>{r.category} · {industryLabel(r.industry)}</span>
              </span>
              <span style={{ height: 12, background: '#eef0f4', borderRadius: 6, overflow: 'hidden' }}>
                <span style={{ display: 'block', height: '100%', width: `${Math.round((r.units / max) * 100)}%`, background: order === 'most' ? C.blue : C.amber, borderRadius: 6 }} />
              </span>
              <span style={{ textAlign: 'right', fontWeight: 700 }}>{num(r.units)} u.</span>
              <span style={{ textAlign: 'right', color: '#3b4a66' }}>{money(r.amount)}</span>
            </div>
          )
        })}
      </div>
    </div>
  )
}
