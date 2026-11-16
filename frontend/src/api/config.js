const LOCAL_API_URL = 'http://localhost:5015'

const normalizeUrl = (url) => url.trim().replace(/\/+$/, '')

export const API_BASE_URL = normalizeUrl(import.meta.env.VITE_API_URL || LOCAL_API_URL)
