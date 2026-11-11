import { useEffect, useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useBranches } from '../context/BranchContext'
import { useCart } from '../context/CartContext'
import { Icon } from './ui'
import { branchLabel, longDate, timeShort } from '../utils/format'

const OWNER_NAV = [
  ['/', 'home', 'Inicio'], ['/stock', 'inventory_2', 'Stock'], ['/compras', 'local_shipping', 'Registrar Stock'],
  ['/precios', 'sell', 'Precios'], ['/ranking', 'leaderboard', 'Ranking de ventas'],
  ['/cierre', 'calendar_month', 'Cierre mensual'], ['/empresas', 'store', 'Empresas y sucursales'], ['/empleados', 'group', 'Empleados'],
]
const EMPLOYEE_NAV = [['/', 'home', 'Inicio'], ['/ventas', 'point_of_sale', 'Registrar venta'], ['/stock', 'inventory_2', 'Consultar stock']]

function Logo({ size, box, font }) {
  return (
    <>
      <span className="icon" style={{ width: box, height: box, borderRadius: 10, background: '#1f9d55', display: 'grid', placeItems: 'center', fontSize: size }}>storefront</span>
      <span style={{ fontSize: font, fontWeight: 700, letterSpacing: '-0.02em' }}>Gesti<span style={{ color: '#4fd18b' }}>ON</span></span>
    </>
  )
}

export { Logo }

export default function AppLayout() {
  const { me, isOwner, logout } = useAuth()
  const { branches, active, setActive, alerts } = useBranches()
  const { cart, setCart } = useCart()
  const navigate = useNavigate()
  const [now, setNow] = useState(new Date())

  useEffect(() => {
    const t = setInterval(() => setNow(new Date()), 10000)
    return () => clearInterval(t)
  }, [])

  const badges = { '/ventas': cart.length || null, '/stock': isOwner ? alerts.length || null : null }
  const nav = isOwner ? OWNER_NAV : EMPLOYEE_NAV

  const doLogout = () => {
    setCart([])
    logout()
    navigate('/')
  }

  return (
    <div style={{ height: '100vh', display: 'grid', gridTemplateColumns: '236px minmax(0,1fr)', gridTemplateRows: '76px minmax(0,1fr)' }}>
      <div style={{ gridRow: 1, gridColumn: 1, background: '#111a2e', display: 'flex', alignItems: 'center', gap: 12, padding: '0 22px', color: '#fff' }}>
        <Logo size={22} box={36} font={22} />
      </div>

      <div style={{ gridRow: 1, gridColumn: 2, background: '#111a2e', color: '#fff', display: 'grid', gridTemplateColumns: 'minmax(0,1fr) auto minmax(0,1fr)', alignItems: 'center', padding: '0 28px', gap: 20 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
          <Icon name="location_on" style={{ fontSize: 20, color: '#8b97b3' }} />
          {isOwner ? (
            <select
              value={active?.id ?? ''}
              onChange={(e) => setActive(e.target.value)}
              style={{ height: 38, maxWidth: 320, padding: '0 10px', borderRadius: 9, border: '1px solid #2b3754', background: '#1a2540', color: '#fff', fontSize: 14 }}
            >
              {branches.map((b) => <option key={b.id} value={b.id}>{branchLabel(b)}</option>)}
            </select>
          ) : (
            <span style={{ fontSize: 14, color: '#dfe5f1', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{branchLabel(active)}</span>
          )}
        </div>
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: 22, fontWeight: 700, letterSpacing: '-0.01em' }}>{timeShort(now)}</div>
          <div style={{ fontSize: 13, color: '#aab4c9' }}>{longDate(now)}</div>
        </div>
        <div style={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', gap: 12 }}>
          <span style={{ width: 40, height: 40, borderRadius: '50%', background: '#3a4663', display: 'grid', placeItems: 'center', fontWeight: 700 }}>
            {me.firstName[0]}{me.lastName[0]}
          </span>
          <div style={{ lineHeight: 1.2 }}>
            <div style={{ fontWeight: 700, fontSize: 15 }}>{me.firstName} {me.lastName}</div>
            <div style={{ fontSize: 12, color: '#aab4c9' }}>{isOwner ? 'Dueño' : 'Empleado'}</div>
          </div>
        </div>
      </div>

      <nav style={{ gridRow: 2, gridColumn: 1, background: '#111a2e', color: '#c9d1e2', display: 'flex', flexDirection: 'column', paddingTop: 14, overflow: 'auto' }}>
        {nav.map(([to, icon, label]) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            style={({ isActive }) => ({
              display: 'flex', alignItems: 'center', gap: 12, width: '100%', padding: '12px 22px', background: isActive ? '#1f66e0' : 'transparent',
              color: isActive ? '#fff' : '#c9d1e2', fontSize: 15, textDecoration: 'none',
            })}
          >
            <Icon name={icon} style={{ fontSize: 22 }} />
            <span style={{ flex: 1 }}>{label}</span>
            {badges[to] && (
              <span style={{ minWidth: 22, height: 22, padding: '0 6px', borderRadius: 11, background: '#dc3a3a', color: '#fff', fontSize: 12, fontWeight: 700, display: 'grid', placeItems: 'center' }}>
                {badges[to]}
              </span>
            )}
          </NavLink>
        ))}
        <div style={{ marginTop: 'auto', borderTop: '1px solid #222d47', padding: '16px 18px', display: 'flex', flexDirection: 'column', gap: 6, fontSize: 12, color: '#8b97b3' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <span style={{ width: 8, height: 8, borderRadius: '50%', background: '#1f9d55' }} />Conectado
          </div>
          <div>Versión 0.1.0</div>
          <button
            onClick={doLogout}
            className="hv-red"
            style={{ marginTop: 8, height: 40, border: 0, borderRadius: 9, background: '#dc3a3a', color: '#fff', fontWeight: 600, fontSize: 14, cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8 }}
          >
            <Icon name="logout" style={{ fontSize: 18 }} />Cerrar sesión
          </button>
        </div>
      </nav>

      <main style={{ gridRow: 2, gridColumn: 2, overflow: 'auto', padding: '28px 32px 40px' }}>
        <Outlet />
      </main>
    </div>
  )
}
