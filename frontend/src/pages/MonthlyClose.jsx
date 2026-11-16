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
  const real = data?.realIncome ?? null
  const realTotal = (m) => real?.realMonths.find((r) => r.year === m.year && r.month === m.month)?.total
  const maxBar = data ? Math.max(1, ...data.months.map((m) => m.total), ...(real?.realMonths.map((m) => m.total) ?? [])) : 1
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

          <div style={{ background: '#e3f4f2', borderRadius: 14, padding: '18px 20px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 16, flexWrap: 'wrap' }}>
            <div>
              <div style={{ fontSize: 14, color: '#5b6579' }}>Ganancia estimada</div>
              <div style={{ fontSize: 30, fontWeight: 800, color: data.estimatedProfit < 0 ? '#b42323' : C.teal, letterSpacing: '-0.02em' }}>{money(data.estimatedProfit)}</div>
            </div>
            <div style={{ fontSize: 13, color: '#5b6579', maxWidth: 460, lineHeight: 1.5 }}>
              Ventas del mes menos el costo de la última compra registrada de cada producto. No incluye productos sin compras registradas
              {data.productsWithoutCost > 0 && <strong> ({num(data.productsWithoutCost)} {data.productsWithoutCost === 1 ? 'producto vendido' : 'productos vendidos'} este mes)</strong>}.
            </div>
          </div>

          {real && (() => {
            const vsPrevReal = compare(data.current.total, real.realPrevious, money)
            const vsYearReal = compare(data.current.total, real.realLastYear, money)
            return (
              <div style={{ background: '#fdf5e8', border: `1px solid ${C.amber}`, borderRadius: 14, padding: '18px 20px', display: 'flex', flexDirection: 'column', gap: 12 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, flexWrap: 'wrap' }}>
                  <div>
                    <div style={{ fontSize: 14, color: '#5b6579' }}>Facturación real · ajustada por inflación (INDEC)</div>
                    <div style={{ fontSize: 13, color: '#5b6579', marginTop: 2 }}>
                      Los meses anteriores se expresan en pesos de {monthName(year, month)}.
                    </div>
                  </div>
                  <div style={{ display: 'flex', gap: 22, fontSize: 13, color: '#5b6579' }}>
                    <span>Inflación del mes <strong style={{ display: 'block', fontSize: 20, color: '#172033' }}>{real.monthlyPercent.toFixed(2).replace('.', ',')}%</strong></span>
                    <span>Últimos 12 meses <strong style={{ display: 'block', fontSize: 20, color: '#172033' }}>{real.accumulatedPercent.toFixed(1).replace('.', ',')}%</strong></span>
                  </div>
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 14, borderTop: '1px solid rgba(17,26,46,.08)', paddingTop: 10 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                    <span style={{ color: '#5b6579' }}>Crecimiento real vs. {monthName(prevMonth.y, prevMonth.m)} ({money(real.realPrevious)} actualizado)</span><strong style={{ color: vsPrevReal.color }}>{vsPrevReal.text}</strong>
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                    <span style={{ color: '#5b6579' }}>Crecimiento real vs. {monthName(year - 1, month)} ({money(real.realLastYear)} actualizado)</span><strong style={{ color: vsYearReal.color }}>{vsYearReal.text}</strong>
                  </div>
                </div>
              </div>
            )
          })()}

          <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1.5fr) minmax(0,1fr)', gap: 18 }}>
            <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: '20px 22px' }}>
              <h3 style={{ margin: '0 0 16px', fontSize: 17 }}>Facturación de los últimos 12 meses</h3>
              {real && (
                <div style={{ display: 'flex', gap: 16, fontSize: 12, color: '#667085', margin: '-8px 0 12px' }}>
                  <span><span style={{ display: 'inline-block', width: 10, height: 10, background: C.purple, borderRadius: 2, marginRight: 6 }} />Facturado</span>
                  <span><span style={{ display: 'inline-block', width: 10, height: 10, background: C.amber, borderRadius: 2, marginRight: 6 }} />Ajustado por inflación</span>
                </div>
              )}
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(12,minmax(0,1fr))', gap: 8, height: 220, alignItems: 'end' }}>
                {data.months.map((m) => {
                  const current = m.year === year && m.month === month
                  return (
                    <div key={`${m.year}-${m.month}`} title={`${monthName(m.year, m.month)}: ${money(m.total)}`} style={{ height: '100%', display: 'flex', flexDirection: 'column', justifyContent: 'flex-end', alignItems: 'center', gap: 6 }}>
                      <div style={{ width: '100%', height: `${Math.round((Math.max(m.total, realTotal(m) ?? 0) / maxBar) * 92)}%`, display: 'flex', alignItems: 'flex-end', gap: 2 }}>
                        <div style={{ flex: 1, height: `${m.total ? Math.max(2, Math.round((m.total / Math.max(m.total, realTotal(m) ?? 0)) * 100)) : 0}%`, minHeight: 2, background: current ? C.purple : '#d8d1f5', borderRadius: '6px 6px 2px 2px' }} />
                        {real && <div style={{ flex: 1, height: `${m.total ? Math.max(2, Math.round(((realTotal(m) ?? 0) / Math.max(m.total, realTotal(m) ?? 0)) * 100)) : 0}%`, minHeight: 2, background: current ? C.amber : '#f1d9ad', borderRadius: '6px 6px 2px 2px' }} />}
                      </div>
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
