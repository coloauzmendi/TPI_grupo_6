import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import AppLayout from './components/AppLayout'
import Login from './pages/Login'
import Home from './pages/Home'
import Sales from './pages/Sales'
import Stock from './pages/Stock'
import Purchases from './pages/Purchases'
import Prices from './pages/Prices'
import Ranking from './pages/Ranking'
import MonthlyClose from './pages/MonthlyClose'
import Companies from './pages/Companies'
import Employees from './pages/Employees'

// Pantalla accesible solo para un rol; si no corresponde vuelve al inicio
function OnlyFor({ role, children }) {
  const { me } = useAuth()
  return me.role === role ? children : <Navigate to="/" replace />
}

export default function App() {
  const { me, loading } = useAuth()

  if (loading) return null
  if (!me) return <Login />

  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Home />} />
        <Route path="stock" element={<Stock />} />
        <Route path="ventas" element={<OnlyFor role="Employee"><Sales /></OnlyFor>} />
        <Route path="compras" element={<OnlyFor role="Owner"><Purchases /></OnlyFor>} />
        <Route path="precios" element={<OnlyFor role="Owner"><Prices /></OnlyFor>} />
        <Route path="ranking" element={<OnlyFor role="Owner"><Ranking /></OnlyFor>} />
        <Route path="cierre" element={<OnlyFor role="Owner"><MonthlyClose /></OnlyFor>} />
        <Route path="empresas" element={<OnlyFor role="Owner"><Companies /></OnlyFor>} />
        <Route path="empleados" element={<OnlyFor role="Owner"><Employees /></OnlyFor>} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
