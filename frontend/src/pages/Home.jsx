import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/endpoints'
import { useAuth } from '../context/AuthContext'
import { useBranches } from '../context/BranchContext'
import { C, Icon, card, h1, subtitle } from '../components/ui'
import { branchLabel, greeting, longDate, money, num, timeShort } from '../utils/format'

const OWNER_TILES = [
  ['/stock', 'inventory_2', 'Stock', C.blue], ['/compras', 'local_shipping', 'Registrar Stock', C.teal], ['/precios', 'sell', 'Precios', C.amber],
  ['/ranking', 'leaderboard', 'Ranking', C.purple], ['/cierre', 'calendar_month', 'Cierre mensual', C.indigo], ['/empresas', 'store', 'Empresas', C.green],
]
const EMPLOYEE_TILES = [['/ventas', 'point_of_sale', 'Registrar venta', C.green], ['/stock', 'inventory_2', 'Consultar stock', C.blue]]

export default function Home() {
  const { me, isOwner } = useAuth()
  const { branches, alerts, setActive } = useBranches()
  const navigate = useNavigate()
  const [today, setToday] = useState(null)
  const now = new Date()

  useEffect(() => {
    api.today().then(setToday).catch(() => {})
  }, [])

  const kpi = today?.kpi ?? { sales: 0, units: 0, total: 0 }
  const kpis = isOwner
    ? [
        { label: 'Ventas de hoy', value: num(kpi.sales), bg: '#fff', color: '#172033' },
        { label: 'Facturado hoy', value: money(kpi.total), bg: '#e3f4ea', color: C.green },
        { label: 'Unidades vendidas hoy', value: num(kpi.units), bg: '#e4ecfb', color: C.blue },
        { label: 'Alertas de stock', value: num(alerts.length), bg: '#fdf0db', color: '#c27a0e' },
      ]
    : [
        { label: 'Tus ventas de hoy', value: num(kpi.sales), bg: '#fff', color: '#172033' },
        { label: 'Total vendido hoy', value: money(kpi.total), bg: '#e3f4ea', color: C.green },
        { label: 'Unidades', value: num(kpi.units), bg: '#e4ecfb', color: C.blue },
      ]

  const branchById = (id) => branches.find((b) => b.id === id)
  const buyFrom = (alert) => {
    setActive(alert.branchId)
    navigate('/compras', { state: { productId: alert.productId } })
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24, animation: 'gin .25s ease' }}>
      <div>
        <h1 style={{ ...h1, fontSize: 30 }}>{greeting(now)}, {me.firstName}</h1>
        <p style={{ ...subtitle, fontSize: 15 }}>{longDate(now)}</p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill,minmax(230px,1fr))', gap: 20 }}>
        {(isOwner ? OWNER_TILES : EMPLOYEE_TILES).map(([to, icon, label, color]) => (
          <button
            key={to}
            onClick={() => navigate(to)}
            className="hv-lift"
            style={{
              height: 180, borderRadius: 16, border: 0, background: color, color: '#fff', display: 'flex', flexDirection: 'column',
              alignItems: 'center', justifyContent: 'center', gap: 14, cursor: 'pointer', boxShadow: '0 2px 6px rgba(17,26,46,.08)',
            }}
          >
            <Icon name={icon} style={{ width: 70, height: 70, display: 'grid', placeItems: 'center', fontSize: 46 }} />
            <span style={{ fontSize: 21, fontWeight: 700 }}>{label}</span>
          </button>
        ))}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit,minmax(190px,1fr))', gap: 16 }}>
        {kpis.map((k) => (
          <div key={k.label} style={{ background: k.bg, borderRadius: 14, padding: '18px 20px' }}>
            <div style={{ fontSize: 14, color: '#5b6579' }}>{k.label}</div>
            <div style={{ fontSize: 28, fontWeight: 800, color: k.color, marginTop: 4, letterSpacing: '-0.02em' }}>{k.value}</div>
          </div>
        ))}
      </div>

      {isOwner && (
        <div style={card}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14, gap: 12 }}>
            <h3 style={{ margin: 0, fontSize: 18 }}>Alertas de stock bajo</h3>
            <span style={{ fontSize: 13, color: '#667085' }}>Stock igual o por debajo del mínimo</span>
          </div>
          {alerts.length === 0 && (
            <div style={{ color: '#667085', fontSize: 15, padding: '8px 0' }}>No hay alertas. Todo el stock está por encima del mínimo.</div>
          )}
          {alerts.slice(0, 8).map((a) => (
            <div
              key={a.branchId + a.productId}
              style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1fr) minmax(0,1fr) auto auto', gap: 16, alignItems: 'center', padding: '12px 0', borderTop: '1px solid #eef0f4', fontSize: 15 }}
            >
              <div><strong>{a.name}</strong> <span style={{ color: '#667085' }}>{a.brand}</span></div>
              <div style={{ color: '#667085', fontSize: 14 }}>{branchLabel(branchById(a.branchId))}</div>
              <span
                style={{
                  fontSize: 13, fontWeight: 700, padding: '4px 10px', borderRadius: 20,
                  background: a.quantity === 0 ? '#fdecec' : '#fdf0db', color: a.quantity === 0 ? '#b42323' : '#a3620a',
                }}
              >
                {a.quantity === 0 ? 'Sin stock' : `${a.quantity} / mín. ${a.minimumStock}`}
              </span>
              <button
                onClick={() => buyFrom(a)}
                className="hv-link"
                style={{ height: 34, padding: '0 12px', border: '1px solid #d5dae3', borderRadius: 8, background: '#fff', fontSize: 13, fontWeight: 600, cursor: 'pointer' }}
              >
                Registrar compra
              </button>
            </div>
          ))}
        </div>
      )}

      {!isOwner && (
        <div style={card}>
          <h3 style={{ margin: '0 0 10px', fontSize: 18 }}>Tus últimas ventas de hoy</h3>
          {(today?.recent ?? []).length === 0 && <div style={{ color: '#667085', fontSize: 15 }}>Todavía no registraste ventas hoy.</div>}
          {(today?.recent ?? []).map((s) => (
            <div key={s.id} style={{ display: 'flex', justifyContent: 'space-between', gap: 12, padding: '11px 0', borderTop: '1px solid #eef0f4', fontSize: 15 }}>
              <span>
                <strong>{s.code}</strong>{' '}
                <span style={{ color: '#667085' }}>· {timeShort(new Date(s.date))} · {s.items.reduce((a, i) => a + i.quantity, 0)} u.</span>
              </span>
              <strong>{money(s.total)}</strong>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
