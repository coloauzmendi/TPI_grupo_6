import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react'

const ToastContext = createContext(null)

export function ToastProvider({ children }) {
  const [toast, setToast] = useState(null)
  const timer = useRef(null)

  const show = useCallback((text, kind = 'ok') => {
    clearTimeout(timer.current)
    setToast({ text, kind })
    timer.current = setTimeout(() => setToast(null), 3200)
  }, [])

  useEffect(() => () => clearTimeout(timer.current), [])

  return (
    <ToastContext.Provider value={show}>
      {children}
      {toast && (
        <div
          style={{
            position: 'fixed', right: 24, bottom: 24, zIndex: 60, background: '#111a2e', color: '#fff',
            padding: '14px 18px', borderRadius: 12, fontSize: 15, display: 'flex', alignItems: 'center', gap: 10,
            boxShadow: '0 10px 30px rgba(17,26,46,.3)', animation: 'gin .2s ease', maxWidth: 'calc(100vw - 48px)',
          }}
        >
          <span className="icon" style={{ fontSize: 22, color: toast.kind === 'ok' ? '#4fd18b' : '#ff8a8a' }}>
            {toast.kind === 'ok' ? 'check_circle' : 'error'}
          </span>
          {toast.text}
        </div>
      )}
    </ToastContext.Provider>
  )
}

// toast('mensaje') para éxito, toast('mensaje', 'err') para error
export const useToast = () => useContext(ToastContext)
