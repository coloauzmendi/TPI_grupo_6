import { useState } from 'react'
import { useAuth } from '../context/AuthContext'
import { api } from '../api/endpoints'
import { Logo } from '../components/AppLayout'
import { errorBox } from '../components/ui'
import { CUIT_RE, formatCuit } from '../utils/format'

const fieldLabel = { display: 'flex', flexDirection: 'column', gap: 6, fontSize: 14, fontWeight: 600 }
const fieldInput = { height: 48, padding: '0 14px', border: '1px solid #d5dae3', borderRadius: 10, fontSize: 16, background: '#fff' }

export default function Login() {
  const { login } = useAuth()
  const [mode, setMode] = useState('login') // 'login' | 'register'
  const [form, setForm] = useState({ cuit: '', password: '', firstName: '', lastName: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value })
  const isRegister = mode === 'register'

  const submit = async (e) => {
    e.preventDefault()
    setError('')

    if (isRegister) {
      if (!CUIT_RE.test(form.cuit.trim())) return setError('El CUIT debe tener el formato 20-12345678-9.')
      if (!form.firstName.trim() || !form.lastName.trim()) return setError('Ingresá tu nombre y apellido.')
      if (form.password.length < 4) return setError('La contraseña debe tener al menos 4 caracteres.')
    }

    setBusy(true)
    try {
      if (isRegister) {
        await api.registerOwner({
          cuit: form.cuit.trim(), password: form.password, firstName: form.firstName.trim(), lastName: form.lastName.trim(),
        })
      }
      await login(form.cuit, form.password)
    } catch (err) {
      setError(err.message)
      setBusy(false)
    }
  }

  return (
    <div style={{ minHeight: '100vh', display: 'grid', gridTemplateColumns: 'minmax(0,1.1fr) minmax(0,1fr)' }}>
      <div style={{ background: '#111a2e', color: '#fff', padding: 56, display: 'flex', flexDirection: 'column', justifyContent: 'space-between', gap: 40 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <Logo size={26} box={42} font={26} />
        </div>
        <div style={{ maxWidth: 520 }}>
          <h1 style={{ fontSize: 44, lineHeight: 1.08, letterSpacing: '-0.03em', margin: '0 0 18px', fontWeight: 800, textWrap: 'pretty' }}>
            Stock, ventas y números del mes, en un solo lugar.
          </h1>
          <p style={{ fontSize: 18, lineHeight: 1.5, color: '#b9c2d6', margin: 0, textWrap: 'pretty' }}>
            Sistema de gestión para kioscos, almacenes, ferreterías y comercios de barrio.
          </p>
        </div>
        <div style={{ fontSize: 13, color: '#7f8ba5' }}>Trabajo Integrador Final · Tecnicatura Universitaria en Programación · 2026</div>
      </div>

      <div style={{ display: 'grid', placeItems: 'center', padding: 40 }}>
        <form onSubmit={submit} style={{ width: '100%', maxWidth: 380, display: 'flex', flexDirection: 'column', gap: 18 }}>
          <div>
            <h2 style={{ margin: '0 0 6px', fontSize: 28, letterSpacing: '-0.02em' }}>{isRegister ? 'Crear cuenta de dueño' : 'Iniciar sesión'}</h2>
            <p style={{ margin: 0, color: '#667085', fontSize: 15 }}>
              {isRegister ? 'Registrate para cargar tus empresas y sucursales.' : 'Ingresá con tu CUIT y contraseña.'}
            </p>
          </div>

          {isRegister && (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <label style={fieldLabel}>Nombre<input value={form.firstName} onChange={set('firstName')} style={fieldInput} /></label>
              <label style={fieldLabel}>Apellido<input value={form.lastName} onChange={set('lastName')} style={fieldInput} /></label>
            </div>
          )}
          <label style={fieldLabel}>
            CUIT
            <input
              value={form.cuit}
              onChange={(e) => setForm({ ...form, cuit: formatCuit(e.target.value) })}
              inputMode="numeric"
              placeholder="20-12345678-9"
              style={fieldInput}
            />
          </label>
          <label style={fieldLabel}>
            Contraseña
            <input type="password" value={form.password} onChange={set('password')} placeholder="••••" style={fieldInput} />
          </label>

          {error && <div style={errorBox}>{error}</div>}

          <button
            type="submit"
            disabled={busy}
            className="hv-primary"
            style={{ height: 50, border: 0, borderRadius: 10, background: '#1f66e0', color: '#fff', fontSize: 16, fontWeight: 700, cursor: 'pointer' }}
          >
            {busy ? 'Ingresando…' : isRegister ? 'Crear cuenta' : 'Ingresar'}
          </button>

          <div style={{ borderTop: '1px solid #e6e9ef', paddingTop: 16, fontSize: 14, color: '#667085', textAlign: 'center' }}>
            {isRegister ? '¿Ya tenés cuenta? ' : '¿Sos dueño y todavía no tenés cuenta? '}
            <button
              type="button"
              onClick={() => { setMode(isRegister ? 'login' : 'register'); setError('') }}
              style={{ border: 0, background: 'none', padding: 0, color: '#1f66e0', fontWeight: 600, fontSize: 14, cursor: 'pointer' }}
            >
              {isRegister ? 'Iniciar sesión' : 'Crear cuenta de dueño'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
