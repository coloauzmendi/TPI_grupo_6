import { request } from './client'

const qs = (params) => {
  const q = new URLSearchParams()
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') q.set(k, v)
  })
  return q.toString()
}

// Un método por endpoint del backend .NET
export const api = {
  // Autenticación y usuario logueado
  login: (cuit, password) => request('POST', '/api/authentication/authenticate', { cuit, password }),
  registerOwner: (data) => request('POST', '/api/user', { ...data, role: 'Owner' }),
  me: () => request('GET', '/api/me'),
  myBranches: () => request('GET', '/api/me/branches'),
  myCompanies: () => request('GET', '/api/me/companies'),
  myEmployees: () => request('GET', '/api/me/employees'),
  stockAlerts: () => request('GET', '/api/me/stock-alerts'),

  // Productos y stock
  products: () => request('GET', '/api/product'),
  createProduct: (p) => request('POST', '/api/product', p),
  updateProduct: (id, p) => request('PUT', `/api/product/${id}`, p),
  updatePrices: (data) => request('POST', '/api/product/prices', data),
  stock: (branchId) => request('GET', `/api/branch/${branchId}/stock`),
  createStock: (branchId, s) => request('POST', `/api/branch/${branchId}/stock`, s),
  updateStock: (branchId, productId, s) => request('PUT', `/api/branch/${branchId}/stock/${productId}`, s),
  deleteStock: (branchId, productId) => request('DELETE', `/api/branch/${branchId}/stock/${productId}`),

  // Ventas y compras
  createSale: (branchId, items) => request('POST', `/api/branch/${branchId}/sales`, { items }),
  createPurchase: (branchId, data) => request('POST', `/api/branch/${branchId}/purchases`, data),

  // Empresas y sucursales
  createCompany: (name) => request('POST', '/api/company', { name }),
  updateCompany: (id, name) => request('PUT', `/api/company/${id}`, { name }),
  deleteCompany: (id) => request('DELETE', `/api/company/${id}`),
  addOwner: (companyId, cuit) => request('POST', `/api/company/${companyId}/owners/${cuit}`),
  removeOwner: (companyId, cuit) => request('DELETE', `/api/company/${companyId}/owners/${cuit}`),
  createBranch: (b) => request('POST', '/api/branch', b),
  updateBranch: (id, b) => request('PUT', `/api/branch/${id}`, b),
  deleteBranch: (id) => request('DELETE', `/api/branch/${id}`),

  // Empleados
  createEmployee: (e) => request('POST', '/api/user', { ...e, role: 'Employee' }),
  updateEmployee: (cuit, e) => request('PUT', `/api/user/${cuit}`, e),
  deleteEmployee: (cuit) => request('DELETE', `/api/user/${cuit}`),

  // Reportes
  today: () => request('GET', '/api/reports/today'),
  ranking: (params) => request('GET', `/api/reports/ranking?${qs(params)}`),
  monthlyClose: (params) => request('GET', `/api/reports/monthly-close?${qs(params)}`),
}
