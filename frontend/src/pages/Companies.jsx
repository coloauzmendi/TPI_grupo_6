import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useAuth } from '../context/AuthContext'
import { useBranches } from '../context/BranchContext'
import { useToast } from '../context/ToastContext'
import { ConfirmModal, FormModal } from '../components/Modal'
import { Icon, PageHeader, card, iconBtn, primaryBtn } from '../components/ui'
import { CUIT_RE } from '../utils/format'
import { INDUSTRIES, industryLabel } from '../utils/industry'

const splitCuits = (text) => text.split(',').map((c) => c.trim()).filter(Boolean)

export default function Companies() {
  const { me } = useAuth()
  const { reloadBranches, reloadAlerts } = useBranches()
  const toast = useToast()
  const [companies, setCompanies] = useState([])
  const [form, setForm] = useState(null) // { kind: 'company' | 'branch', id?, values, errors }
  const [confirm, setConfirm] = useState(null) // { kind, id, title, text, warn }
  const [busy, setBusy] = useState(false)

  const load = useCallback(() => api.myCompanies().then(setCompanies).catch((e) => toast(e.message, 'err')), [toast])
  useEffect(() => {
    load()
  }, [load])

  const refreshAll = () => Promise.all([load(), reloadBranches(), reloadAlerts()])

  const openCompany = (c) =>
    setForm({
      kind: 'company',
      id: c?.id,
      values: { name: c?.name ?? '', owners: c ? c.owners.filter((o) => o.cuit !== me.cuit).map((o) => o.cuit).join(', ') : '' },
      errors: {},
    })
  const openBranch = (companyId, b) =>
    setForm({
      kind: 'branch',
      id: b?.id,
      values: { companyId, address: b?.address ?? '', locality: b?.locality ?? '', industry: b?.industry ?? 'Kiosk' },
      errors: {},
    })

  const change = (key, value) => setForm((f) => ({ ...f, values: { ...f.values, [key]: value }, errors: { ...f.errors, [key]: null } }))

  const validate = () => {
    const v = form.values
    const e = {}
    if (form.kind === 'company') {
      if (!v.name.trim()) e.name = 'Campo obligatorio'
      const bad = splitCuits(v.owners).find((c) => !CUIT_RE.test(c))
      if (bad) e.owners = `El CUIT ${bad} no tiene un formato válido (20-12345678-9)`
    } else {
      ;['address', 'locality'].forEach((k) => { if (!v[k].trim()) e[k] = 'Campo obligatorio' })
    }
    return e
  }

  const saveCompany = async () => {
    const v = form.values
    const wanted = splitCuits(v.owners).filter((c) => c !== me.cuit)
    let companyId = form.id
    let current = []

    if (companyId) {
      await api.updateCompany(companyId, v.name.trim())
      current = companies.find((c) => c.id === companyId).owners.map((o) => o.cuit).filter((c) => c !== me.cuit)
    } else {
      companyId = (await api.createCompany(v.name.trim())).id
    }

    for (const cuit of wanted.filter((c) => !current.includes(c))) await api.addOwner(companyId, cuit)
    for (const cuit of current.filter((c) => !wanted.includes(c))) await api.removeOwner(companyId, cuit)
  }

  const saveBranch = async () => {
    const v = form.values
    const body = { locality: v.locality.trim(), address: v.address.trim(), industry: v.industry, companyId: v.companyId }
    if (form.id) await api.updateBranch(form.id, body)
    else await api.createBranch(body)
  }

  const save = async () => {
    const errors = validate()
    if (Object.keys(errors).length) return setForm({ ...form, errors })

    setBusy(true)
    try {
      await (form.kind === 'company' ? saveCompany() : saveBranch())
      setForm(null)
      toast(form.id ? 'Cambios guardados' : 'Alta realizada correctamente')
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
      refreshAll()
    }
  }

  const remove = async () => {
    setBusy(true)
    try {
      await (confirm.kind === 'company' ? api.deleteCompany(confirm.id) : api.deleteBranch(confirm.id))
      setConfirm(null)
      toast(confirm.kind === 'company' ? 'Empresa dada de baja' : 'Sucursal dada de baja')
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
      refreshAll()
    }
  }

  const askDeleteCompany = (c) => {
    const employees = c.branches.reduce((a, b) => a + b.employeeCount, 0)
    setConfirm({
      kind: 'company',
      id: c.id,
      title: 'Dar de baja empresa',
      text: `¿Dar de baja "${c.name}"? También se darán de baja sus sucursales.`,
      warn: c.branches.length ? `Tiene ${c.branches.length} sucursal(es) y ${employees} empleado(s) activos asociados.` : null,
    })
  }
  const askDeleteBranch = (b) =>
    setConfirm({
      kind: 'branch',
      id: b.id,
      title: 'Dar de baja sucursal',
      text: `¿Dar de baja la sucursal ${b.address}?`,
      warn: b.employeeCount ? `Tiene ${b.employeeCount} empleado(s) asignados.` : null,
    })

  const fields =
    form?.kind === 'company'
      ? [
          { key: 'name', label: 'Nombre de la empresa', full: true },
          { key: 'owners', label: 'CUIT de otros dueños (opcional, separados por coma)', full: true, placeholder: '27-31987654-2' },
        ]
      : [
          { key: 'companyId', label: 'Empresa', full: true, options: companies.map((c) => ({ value: c.id, label: c.name })) },
          { key: 'address', label: 'Dirección', full: true },
          { key: 'locality', label: 'Localidad' },
          { key: 'industry', label: 'Rubro', options: INDUSTRIES },
        ]

  const formTitle = form && `${form.id ? 'Editar' : 'Agregar'} ${form.kind === 'company' ? 'empresa' : 'sucursal'}`

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
      <PageHeader title="Empresas y sucursales" subtitle="Empresas de las que sos dueño.">
        <button onClick={() => openCompany()} className="hv-primary" style={primaryBtn}>
          <Icon name="add" style={{ fontSize: 20 }} />Agregar empresa
        </button>
      </PageHeader>

      {companies.map((c) => (
        <div key={c.id} style={{ ...card, display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 12, flexWrap: 'wrap' }}>
            <div>
              <div style={{ fontSize: 20, fontWeight: 700 }}>{c.name}</div>
              <div style={{ fontSize: 13, color: '#667085' }}>Dueños: {c.owners.map((o) => o.fullName).join(', ')}</div>
            </div>
            <div style={{ display: 'flex', gap: 8 }}>
              <button onClick={() => openBranch(c.id)} className="hv-link" style={{ height: 38, padding: '0 14px', border: '1px solid #d5dae3', borderRadius: 9, background: '#fff', fontWeight: 600, fontSize: 14, cursor: 'pointer' }}>
                + Sucursal
              </button>
              <button onClick={() => openCompany(c)} aria-label="Editar empresa" style={iconBtn('#eef2f8', '#1f66e0', 38)}><Icon name="edit" style={{ fontSize: 20 }} /></button>
              <button onClick={() => askDeleteCompany(c)} aria-label="Dar de baja empresa" style={iconBtn('#fdecec', '#dc3a3a', 38)}><Icon name="delete" style={{ fontSize: 20 }} /></button>
            </div>
          </div>

          {c.branches.length === 0 && <div style={{ fontSize: 14, color: '#667085' }}>Sin sucursales cargadas.</div>}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill,minmax(260px,1fr))', gap: 12 }}>
            {c.branches.map((b) => (
              <div key={b.id} style={{ border: '1px solid #e6e9ef', borderRadius: 12, padding: 14, display: 'flex', flexDirection: 'column', gap: 6, background: '#fafbfc' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                  <strong>{b.address}</strong>
                  <span style={{ fontSize: 12, fontWeight: 700, padding: '3px 9px', borderRadius: 20, background: '#eef2f8', color: '#3b4a66', height: 'fit-content' }}>{industryLabel(b.industry)}</span>
                </div>
                <div style={{ fontSize: 14, color: '#667085' }}>{b.locality} · {b.employeeCount} {b.employeeCount === 1 ? 'empleado' : 'empleados'}</div>
                <div style={{ display: 'flex', gap: 12, marginTop: 4, fontSize: 14 }}>
                  <button onClick={() => openBranch(c.id, b)} style={{ border: 0, background: 'none', padding: 0, color: '#1f66e0', fontWeight: 600, cursor: 'pointer', fontSize: 14 }}>Editar</button>
                  <button onClick={() => askDeleteBranch(b)} style={{ border: 0, background: 'none', padding: 0, color: '#dc3a3a', fontWeight: 600, cursor: 'pointer', fontSize: 14 }}>Dar de baja</button>
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}

      {form && (
        <FormModal title={formTitle} fields={fields} values={form.values} errors={form.errors} onChange={change} onSave={save} onClose={() => setForm(null)} busy={busy} />
      )}
      {confirm && (
        <ConfirmModal title={confirm.title} text={confirm.text} warn={confirm.warn} actionLabel="Dar de baja" busy={busy} onConfirm={remove} onClose={() => setConfirm(null)} />
      )}
    </div>
  )
}
