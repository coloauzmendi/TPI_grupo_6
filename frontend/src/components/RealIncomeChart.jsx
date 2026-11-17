import { C, Icon } from './ui'
import { MESES, cap, money, moneyShort, pctText } from '../utils/format'

const monthName = (year, month) => `${cap(MESES[month - 1])} ${year}`
const pct = (n, digits = 1) => n.toFixed(digits).replace('.', ',') + '%'

// Ventas reales: cada mes facturado vs. ese mismo dinero expresado en pesos del mes consultado
export function RealIncomeChart({ data, real, year, month }) {
  const realOf = (m) => real.realMonths.find((r) => r.year === m.year && r.month === m.month)?.total ?? 0
  const max = Math.max(1, ...data.months.map((m) => Math.max(m.total, realOf(m))))
  const now = monthName(year, month)

  // Crecimiento real contra el mismo mes del año anterior (si no hay dato, contra el mes anterior)
  const base = real.realLastYear ? { ref: real.realLastYear, label: `${cap(MESES[month - 1])} del año pasado` } : { ref: real.realPrevious, label: 'el mes pasado' }
  const growth = base.ref ? ((data.current.total - base.ref) / base.ref) * 100 : null
  const up = growth !== null && growth >= 0

  return (
    <div style={{ background: '#fff', border: `2px solid ${C.amber}`, borderRadius: 18, padding: '24px 26px', display: 'flex', flexDirection: 'column', gap: 20 }}>
      <div>
        <h3 style={{ margin: 0, fontSize: 22, letterSpacing: '-0.01em' }}>¿Cuánto vendiste realmente?</h3>
        <p style={{ margin: '6px 0 0', fontSize: 15, lineHeight: 1.5, color: '#5b6579' }}>
          Con la inflación, un peso de hace unos meses valía más que uno de hoy. Por eso, para comparar de forma justa, convertimos las ventas de cada mes a <strong>pesos de {now}</strong>.
        </p>
      </div>

      {growth !== null && (
        <div style={{ display: 'flex', alignItems: 'center', gap: 14, background: up ? '#e3f4ea' : '#fbe7e7', borderRadius: 14, padding: '16px 18px' }}>
          <Icon name={up ? 'trending_up' : 'trending_down'} style={{ fontSize: 36, color: up ? C.green : C.red }} />
          <div style={{ fontSize: 17, lineHeight: 1.4, color: '#172033' }}>
            Descontando la inflación, tus ventas de {now} {up ? 'crecieron' : 'cayeron'}{' '}
            <strong style={{ color: up ? C.green : C.red }}>{pct(Math.abs(growth))}</strong> frente a {base.label}.
            <div style={{ fontSize: 13, color: '#5b6579', marginTop: 2 }}>
              Facturaste {money(data.current.total)}; lo de {base.label} equivale hoy a {money(base.ref)} ({pctText(growth)}).
            </div>
          </div>
        </div>
      )}

      <div style={{ display: 'flex', gap: 22, flexWrap: 'wrap', fontSize: 14, color: '#3b4a66' }}>
        <span><span style={{ display: 'inline-block', width: 14, height: 14, background: C.purple, borderRadius: 3, marginRight: 8, verticalAlign: -2 }} /><strong>Lo que facturaste</strong> en cada mes</span>
        <span><span style={{ display: 'inline-block', width: 14, height: 14, background: C.amber, borderRadius: 3, marginRight: 8, verticalAlign: -2 }} /><strong>Ese mismo dinero en pesos de {now}</strong></span>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(12,minmax(0,1fr))', gap: 10 }}>
        {data.months.map((m) => {
          const current = m.year === year && m.month === month
          const r = realOf(m)
          return (
            <div key={`${m.year}-${m.month}`} title={`${monthName(m.year, m.month)}: facturaste ${money(m.total)} · equivale a ${money(r)} de hoy`} style={{ display: 'flex', flexDirection: 'column', gap: 6, alignItems: 'center', minWidth: 0 }}>
              <div style={{ height: 260, width: '100%', display: 'flex', alignItems: 'flex-end', gap: 3 }}>
                <div style={{ flex: 1, height: `${m.total ? Math.max(2, (m.total / max) * 100) : 0}%`, background: C.purple, opacity: current ? 1 : 0.55, borderRadius: '6px 6px 2px 2px' }} />
                <div style={{ flex: 1, height: `${r ? Math.max(2, (r / max) * 100) : 0}%`, background: C.amber, opacity: current ? 1 : 0.55, borderRadius: '6px 6px 2px 2px' }} />
              </div>
              <span style={{ fontSize: 12, fontWeight: 700, color: C.purple }}>{moneyShort(m.total)}</span>
              <span style={{ fontSize: 12, fontWeight: 700, color: '#b9770e' }}>{moneyShort(r)}</span>
              <span style={{ fontSize: 13, color: current ? '#172033' : '#8a93a6', fontWeight: current ? 800 : 500 }}>{MESES[m.month - 1].slice(0, 3)}</span>
            </div>
          )
        })}
      </div>

      <div style={{ display: 'flex', gap: 14, flexWrap: 'wrap' }}>
        <div style={{ flex: 1, minWidth: 220, background: '#fdf5e8', borderRadius: 12, padding: '12px 16px' }}>
          <div style={{ fontSize: 24, fontWeight: 800 }}>{pct(real.monthlyPercent, 2)}</div>
          <div style={{ fontSize: 13, color: '#5b6579' }}>Subieron los precios en {now} (INDEC)</div>
        </div>
        <div style={{ flex: 1, minWidth: 220, background: '#fdf5e8', borderRadius: 12, padding: '12px 16px' }}>
          <div style={{ fontSize: 24, fontWeight: 800 }}>{pct(real.accumulatedPercent)}</div>
          <div style={{ fontSize: 13, color: '#5b6579' }}>Subieron los precios en los últimos 12 meses</div>
        </div>
      </div>
    </div>
  )
}
