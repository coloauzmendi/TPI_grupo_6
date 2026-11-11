import { useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useBranches } from '../context/BranchContext'
import { useToast } from '../context/ToastContext'
import { PeriodFilters, scopeParams } from '../components/PeriodFilters'
import { C, PageHeader, thStyle } from '../components/ui'
import { SalesPie } from '../components/SalesPie'
import { MESES, branchLabel, cap, money, moneyShort, num, pctText } from '../utils/format'

const monthName = (year, month) => `${cap(MESES[month - 1])} ${year}`

// Diferencia contra otro período: "+$1.000,00 (+12,5%)" en verde o rojo
function compare(a, b, fmt) {
  if (!b) return { text: 'Sin datos', color: '#98a1b3' }
  const d = a - b
  return { text: `${d >= 0 ? '+' : '−'}${fmt(Math.abs(d))} (${pctText(((a - b) / b) * 100)})`, color: d >= 0 ? C.green : C.red }
}

export default function MonthlyClose() {
  const { branches } = useBranches()
  const toast = useToast()
  const now = new Date()
  // Por defecto se muestra el mes que acaba de cerrar
  const prev = new Date(now.getFullYear(), now.getMonth() - 1, 1)
  const [filters, setFilters] = useState({ scope: 'all', month: prev.getMonth() + 1, year: prev.getFullYear() })
  const [data, setData] = useState(null)

  useEffect(() => {
    api.monthlyClose({ ...scopeParams(filters.scope), year: filters.year, month: filters.month }).then(setData).catch((e) => toast(e.message, 'err'))
  }, [filters, toast])

  const { year, month } = filters
  const prevMonth = month === 1 ? { y: year - 1, m: 12 } : { y: year, m: month - 1 }
  const KPIS = [
    ['Cantidad de ventas', 'sales', num, '#fff', '#172033'],
    ['Unidades vendidas', 'units', num, '#e4ecfb', C.blue],
    ['Total facturado', 'total', money, '#e3f4ea', C.green],
  ]

  const empty = data && data.current.sales === 0
  const maxBar = data ? Math.max(1, ...data.months.map((m) => m.total)) : 1
  const maxTop = data?.top[0]?.units ?? 1

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
      <PageHeader title={`Cierre mensual · ${monthName(year, month)}`} subtitle="Comparado con el mes anterior y con el mismo mes del año anterior.">
        <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
          <PeriodFilters value={filters} onChange={setFilters} />
        </div>
      </PageHeader>

      {empty && (
        <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 14, padding: 40, textAlign: 'center', color: '#667085', fontSize: 16 }}>
          No hay ventas registradas para el período seleccionado.
        </div>
      )}

      {data && !empty && (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit,minmax(240px,1fr))', gap: 16 }}>
            {KPIS.map(([label, key, fmt, bg, color]) => {
              const vsPrev = compare(data.current[key], data.previous[key], fmt)
              const vsYear = compare(data.current[key], data.lastYear[key], fmt)
              return (
                <div key={key} style={{ background: bg, borderRadius: 14, padding: '18px 20px', display: 'flex', flexDirection: 'column', gap: 10 }}>
                  <div>
                    <div style={{ fontSize: 14, color: '#5b6579' }}>{label}</div>
                    <div style={{ fontSize: 30, fontWeight: 800, color, letterSpacing: '-0.02em' }}>{fmt(data.current[key])}</div>
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13, borderTop: '1px solid rgba(17,26,46,.08)', paddingTop: 10 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                      <span style={{ color: '#5b6579' }}>vs. {monthName(prevMonth.y, prevMonth.m)}</span><strong style={{ color: vsPrev.color }}>{vsPrev.text}</strong>
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                      <span style={{ color: '#5b6579' }}>vs. {monthName(year - 1, month)}</span><strong style={{ color: vsYear.color }}>{vsYear.text}</strong>
                    </div>
                  </div>
                </div>
              )
            })}
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1.5fr) minmax(0,1fr)', gap: 18 }}>
            <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: '20px 22px' }}>
              <h3 style={{ margin: '0 0 16px', fontSize: 17 }}>Facturación de los últimos 12 meses</h3>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(12,minmax(0,1fr))', gap: 8, height: 220, alignItems: 'end' }}>
                {data.months.map((m) => {
                  const current = m.year === year && m.month === month
                  return (
                    <div key={`${m.year}-${m.month}`} title={`${monthName(m.year, m.month)}: ${money(m.total)}`} style={{ height: '100%', display: 'flex', flexDirection: 'column', justifyContent: 'flex-end', alignItems: 'center', gap: 6 }}>
                      <div style={{ width: '100%', height: `${Math.round((m.total / maxBar) * 92)}%`, background: current ? C.purple : '#d8d1f5', borderRadius: '6px 6px 2px 2px', minHeight: 2 }} />
                      <span style={{ fontSize: 11, color: current ? '#172033' : '#667085', fontWeight: current ? 700 : 500 }}>{moneyShort(m.total)}</span>
                      <span style={{ fontSize: 12, color: current ? '#172033' : '#8a93a6', fontWeight: current ? 700 : 400 }}>{MESES[m.month - 1].slice(0, 3)}</span>
                    </div>
                  )
                })}
              </div>
            </div>
            <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: '20px 22px' }}>
              <h3 style={{ margin: '0 0 12px', fontSize: 17 }}>Más vendidos del mes</h3>
              {data.top.map((t) => (
                <div key={t.name} style={{ display: 'flex', flexDirection: 'column', gap: 5, padding: '8px 0' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 14, gap: 8 }}>
                    <span style={{ fontWeight: 600 }}>{t.name}</span><span style={{ color: '#667085' }}>{num(t.units)} u.</span>
                  </div>
                  <div style={{ height: 8, background: '#eef0f4', borderRadius: 4 }}>
                    <div style={{ height: '100%', width: `${Math.round((t.units / maxTop) * 100)}%`, background: C.purple, borderRadius: 4 }} />
                  </div>
                </div>
              ))}
            </div>
          </div>

          <SalesPie key={`${filters.scope}-${year}-${month}`} rows={data.breakdown ?? []} />

          <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 14, overflow: 'hidden' }}>
            <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0,2fr) 110px 110px 160px', gap: 12, padding: '12px 18px', borderBottom: '1px solid #eef0f4', ...thStyle }}>
              <span>Sucursal</span><span style={{ textAlign: 'right' }}>Ventas</span><span style={{ textAlign: 'right' }}>Unidades</span><span style={{ textAlign: 'right' }}>Total facturado</span>
            </div>
            {data.branches.map((b) => (
              <div key={b.branchId} style={{ display: 'grid', gridTemplateColumns: 'minmax(0,2fr) 110px 110px 160px', gap: 12, padding: '13px 18px', fontSize: 15, borderBottom: '1px solid #f1f3f6' }}>
                <span>{branchLabel(branches.find((x) => x.id === b.branchId))}</span>
                <span style={{ textAlign: 'right' }}>{num(b.sales)}</span>
                <span style={{ textAlign: 'right' }}>{num(b.units)}</span>
                <strong style={{ textAlign: 'right' }}>{money(b.total)}</strong>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  )
}
