import { useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useBranches } from '../context/BranchContext'
import { inputBox } from './ui'
import { MESES, branchLabel, cap } from '../utils/format'

const selectStyle = { ...inputBox, padding: '0 12px' }

// Alcance del reporte: "all", "c:<idEmpresa>" o "s:<idSucursal>" → parámetros del endpoint
export const scopeParams = (scope) => {
  if (scope.startsWith('c:')) return { companyId: scope.slice(2) }
  if (scope.startsWith('s:')) return { branchId: scope.slice(2) }
  return {}
}

// Selectores de alcance, mes y año compartidos por Ranking y Cierre mensual
export function PeriodFilters({ value, onChange }) {
  const { branches } = useBranches()
  const [companies, setCompanies] = useState([])
  const year = new Date().getFullYear()

  useEffect(() => {
    api.myCompanies().then(setCompanies).catch(() => {})
  }, [])

  const set = (key, parse = (v) => v) => (e) => onChange({ ...value, [key]: parse(e.target.value) })

  return (
    <>
      <select value={value.scope} onChange={set('scope')} style={{ ...selectStyle, minWidth: 260 }}>
        <option value="all">Todas mis sucursales</option>
        {companies.map((c) => <option key={c.id} value={`c:${c.id}`}>Empresa: {c.name}</option>)}
        {branches.map((b) => <option key={b.id} value={`s:${b.id}`}>Sucursal: {branchLabel(b)}</option>)}
      </select>
      <select value={value.month} onChange={set('month', Number)} style={selectStyle}>
        {MESES.map((m, i) => <option key={m} value={i + 1}>{cap(m)}</option>)}
      </select>
      <select value={value.year} onChange={set('year', Number)} style={selectStyle}>
        {[year - 1, year].map((y) => <option key={y} value={y}>{y}</option>)}
      </select>
    </>
  )
}
