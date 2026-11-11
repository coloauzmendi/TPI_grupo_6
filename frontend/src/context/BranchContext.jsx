import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useAuth } from './AuthContext'

const BranchContext = createContext(null)
const STORAGE_KEY = 'gestion.branch'

// Sucursales del usuario, la sucursal activa y las alertas de stock bajo (todo depende del usuario logueado)
export function BranchProvider({ children }) {
  const { me } = useAuth()
  const [branches, setBranches] = useState([])
  const [alerts, setAlerts] = useState([])
  const [activeId, setActiveId] = useState(() => localStorage.getItem(STORAGE_KEY))

  const reloadBranches = useCallback(async () => setBranches(await api.myBranches()), [])
  const reloadAlerts = useCallback(async () => setAlerts(await api.stockAlerts()), [])

  useEffect(() => {
    if (!me) {
      setBranches([])
      setAlerts([])
      return
    }
    reloadBranches().catch(() => {})
    reloadAlerts().catch(() => {})
  }, [me, reloadBranches, reloadAlerts])

  // El empleado siempre opera en su sucursal; el dueño en la que eligió (o la primera)
  const active = me?.role === 'Employee'
    ? branches[0]
    : branches.find((b) => b.id === activeId) ?? branches[0]

  const setActive = useCallback((id) => {
    setActiveId(id)
    localStorage.setItem(STORAGE_KEY, id)
  }, [])

  return (
    <BranchContext.Provider value={{ branches, active, setActive, alerts, reloadAlerts, reloadBranches }}>
      {children}
    </BranchContext.Provider>
  )
}

export const useBranches = () => useContext(BranchContext)
