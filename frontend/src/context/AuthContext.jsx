import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { setToken, setUnauthorizedHandler } from '../api/client'

const AuthContext = createContext(null)
const STORAGE_KEY = 'gestion.token'

// El payload del JWT va en Base64 (solo se lee la expiración, la validación real la hace el backend)
function tokenExpired(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
    return payload.exp * 1000 <= Date.now()
  } catch {
    return true
  }
}

export function AuthProvider({ children }) {
  const [me, setMe] = useState(null)
  const [loading, setLoading] = useState(true)

  const logout = useCallback(() => {
    localStorage.removeItem(STORAGE_KEY)
    setToken(null)
    setMe(null)
  }, [])

  const startSession = useCallback(async (token) => {
    setToken(token)
    try {
      setMe(await api.me())
      localStorage.setItem(STORAGE_KEY, token)
    } catch (e) {
      logout()
      throw e
    }
  }, [logout])

  // Al abrir la app se retoma la sesión guardada si el token sigue vigente
  useEffect(() => {
    setUnauthorizedHandler(logout)
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved && !tokenExpired(saved)) {
      startSession(saved).catch(() => {}).finally(() => setLoading(false))
    } else {
      localStorage.removeItem(STORAGE_KEY)
      setLoading(false)
    }
  }, [logout, startSession])

  const login = useCallback(
    async (cuit, password) => startSession(await api.login(cuit.trim(), password)),
    [startSession],
  )

  return <AuthContext.Provider value={{ me, loading, login, logout, isOwner: me?.role === 'Owner' }}>{children}</AuthContext.Provider>
}

export const useAuth = () => useContext(AuthContext)
