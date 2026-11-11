const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5015'

let token = null
let onUnauthorized = null

export const setToken = (t) => {
  token = t
}

// Se llama cuando la API rechaza el token (vencido o inválido) para cerrar la sesión
export const setUnauthorizedHandler = (fn) => {
  onUnauthorized = fn
}

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.status = status
  }
}

// La API devuelve ProblemDetails ({ detail, title }), errores de validación ({ errors }) o un string suelto
async function readError(res) {
  const text = await res.text()
  try {
    const data = JSON.parse(text)
    if (typeof data === 'string') return data
    if (data.errors) return Object.values(data.errors).flat()[0]
    return data.detail || data.title || text
  } catch {
    return text || `Error ${res.status}`
  }
}

export async function request(method, path, body) {
  let res
  try {
    res = await fetch(BASE_URL + path, {
      method,
      headers: {
        ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new ApiError('No se pudo conectar con el servidor.', 0)
  }

  if (!res.ok) {
    if (res.status === 401 && token && onUnauthorized) onUnauthorized()
    throw new ApiError(await readError(res), res.status)
  }

  if (res.status === 204) return null
  const type = res.headers.get('content-type') ?? ''
  return type.includes('json') ? res.json() : res.text()
}
