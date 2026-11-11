export const MESES = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre']
export const DIAS = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado']

export const CUIT_RE = /^\d{2}-\d{8}-\d$/

// Deja solo dígitos (máx. 11) y agrega los guiones: 20-12345678-9
export const formatCuit = (v) => {
  const d = v.replace(/\D/g, '').slice(0, 11)
  if (d.length <= 2) return d
  if (d.length <= 10) return `${d.slice(0, 2)}-${d.slice(2)}`
  return `${d.slice(0, 2)}-${d.slice(2, 10)}-${d.slice(10)}`
}

export const cap = (s) => s.charAt(0).toUpperCase() + s.slice(1)

export const money = (n) =>
  '$' + (Math.round(n * 100) / 100).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

export const num = (n) => n.toLocaleString('es-AR')

// Acepta coma o punto como separador decimal; devuelve NaN si no es un número
export const parseDecimal = (text) => {
  const t = String(text ?? '').replace(',', '.').trim()
  return t === '' ? NaN : Number(t)
}

export const longDate = (d) => `${cap(DIAS[d.getDay()])}, ${d.getDate()} de ${MESES[d.getMonth()]} de ${d.getFullYear()}`

export const timeShort = (d) => d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })

export const greeting = (d) => {
  const h = d.getHours()
  return h < 13 ? 'Buen día' : h < 20 ? 'Buenas tardes' : 'Buenas noches'
}

// Variación porcentual con signo, ej.: "+12,5%"
export const pctText = (n) => (n >= 0 ? '+' : '') + n.toFixed(1).replace('.', ',') + '%'

// Etiqueta de una sucursal: "Empresa · Dirección"
export const branchLabel = (b) => (b ? (b.companyName ? `${b.companyName} · ${b.address}` : b.address) : '—')

// Monto abreviado para etiquetas chicas: $1,2 M, $350 mil, $900
export const moneyShort = (n) => {
  if (n >= 1e6) return '$' + (n / 1e6).toFixed(1).replace('.', ',') + ' M'
  if (n >= 1e3) return '$' + Math.round(n / 1e3) + ' mil'
  return '$' + Math.round(n)
}
