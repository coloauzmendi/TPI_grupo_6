import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/endpoints'
import { useBranches } from '../context/BranchContext'
import { useToast } from '../context/ToastContext'
import { ConfirmModal, FormModal } from '../components/Modal'
import { Icon, PageHeader, iconBtn, primaryBtn, tableCard, thStyle } from '../components/ui'
import { CUIT_RE, branchLabel, num } from '../utils/format'

const COLS = '160px minmax(0,1.2fr) minmax(0,2fr) 140px 96px'

export default function Employees() {
  const { branches, active } = useBranches()
  const toast = useToast()
  const [employees, setEmployees] = useState([])
  const [form, setForm] = useState(null) // { cuit?: (si se edita), values, errors }
  const [confirm, setConfirm] = useState(null)
  const [busy, setBusy] = useState(false)

  const load = useCallback(() => api.myEmployees().then(setEmployees).catch((e) => toast(e.message, 'err')), [toast])
  useEffect(() => {
    load()
  }, [load])

  const branchOptions = branches.map((b) => ({ value: b.id, label: branchLabel(b) }))
  const editing = !!form?.cuit

  const openNew = () =>
    setForm({ values: { cuit: '', firstName: '', lastName: '', branchId: active?.id ?? '', password: '' }, errors: {} })
  const openEdit = (e) =>
    setForm({ cuit: e.cuit, values: { cuit: e.cuit, firstName: e.firstName, lastName: e.lastName, branchId: e.branchId, password: '' }, errors: {} })

  const change = (key, value) => setForm((f) => ({ ...f, values: { ...f.values, [key]: value }, errors: { ...f.errors, [key]: null } }))

  const save = async () => {
    const v = form.values
    const errors = {}
    if (!v.firstName.trim()) errors.firstName = 'Campo obligatorio'
    if (!v.lastName.trim()) errors.lastName = 'Campo obligatorio'
    if (!v.branchId) errors.branchId = 'Campo obligatorio'
    if (!editing) {
      if (!CUIT_RE.test(v.cuit.trim())) errors.cuit = 'Formato: 20-12345678-9'
      if (v.password.length < 4) errors.password = 'Mínimo 4 caracteres'
    }
    if (Object.keys(errors).length) return setForm({ ...form, errors })

    setBusy(true)
    try {
      const body = { firstName: v.firstName.trim(), lastName: v.lastName.trim(), branchId: v.branchId, password: v.password || null }
      if (editing) await api.updateEmployee(form.cuit, body)
      else await api.createEmployee({ ...body, cuit: v.cuit.trim() })
      setForm(null)
      toast(editing ? 'Cambios guardados' : 'Alta realizada correctamente')
      load()
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
    }
  }

  const remove = async () => {
    setBusy(true)
    try {
      await api.deleteEmployee(confirm.cuit)
      setConfirm(null)
      toast('Empleado dado de baja')
      load()
    } catch (e) {
      toast(e.message, 'err')
    } finally {
      setBusy(false)
    }
  }

  const fields = [
    ...(editing ? [] : [{ key: 'cuit', label: 'CUIT', full: true, placeholder: '20-12345678-9' }]),
    { key: 'firstName', label: 'Nombre' },
    { key: 'lastName', label: 'Apellido' },
    { key: 'branchId', label: 'Sucursal asignada', full: true, options: branchOptions },
    { key: 'password', label: editing ? 'Nueva contraseña (opcional)' : 'Contraseña inicial', type: 'password', full: true },
  ]

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
      <PageHeader title="Empleados" subtitle="Cada empleado opera sobre una única sucursal.">
        <button onClick={openNew} className="hv-primary" style={primaryBtn}>
          <Icon name="person_add" style={{ fontSize: 20 }} />Agregar empleado
        </button>
      </PageHeader>

      <div style={{ ...tableCard, overflow: 'hidden' }}>
        <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, padding: '12px 18px', borderBottom: '1px solid #eef0f4', ...thStyle }}>
          <span>CUIT</span><span>Nombre</span><span>Sucursal</span><span style={{ textAlign: 'right' }}>Ventas del mes</span><span />
        </div>
        {employees.length === 0 && <div style={{ padding: 28, textAlign: 'center', color: '#667085' }}>Todavía no cargaste empleados.</div>}
        {employees.map((e) => (
          <div key={e.cuit} style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, padding: '13px 18px', fontSize: 15, borderBottom: '1px solid #f1f3f6', alignItems: 'center' }}>
            <span style={{ fontVariantNumeric: 'tabular-nums', color: '#3b4a66' }}>{e.cuit}</span>
            <strong style={{ fontWeight: 600 }}>{e.firstName} {e.lastName}</strong>
            <span style={{ color: '#3b4a66' }}>{branchLabel(branches.find((b) => b.id === e.branchId))}</span>
            <span style={{ textAlign: 'right' }}>{num(e.monthSales)}</span>
            <span style={{ display: 'flex', gap: 4, justifyContent: 'flex-end' }}>
              <button onClick={() => openEdit(e)} aria-label="Editar" style={iconBtn('#eef2f8', '#1f66e0')}><Icon name="edit" style={{ fontSize: 20 }} /></button>
              <button onClick={() => setConfirm(e)} aria-label="Dar de baja" style={iconBtn('#fdecec', '#dc3a3a')}><Icon name="delete" style={{ fontSize: 20 }} /></button>
            </span>
          </div>
        ))}
      </div>

      {form && (
        <FormModal
          title={editing ? 'Editar empleado' : 'Agregar empleado'}
          fields={fields}
          values={form.values}
          errors={form.errors}
          onChange={change}
          onSave={save}
          onClose={() => setForm(null)}
          busy={busy}
        />
      )}
      {confirm && (
        <ConfirmModal
          title="Dar de baja empleado"
          text={`¿Dar de baja a ${confirm.firstName} ${confirm.lastName}? No podrá volver a iniciar sesión.`}
          actionLabel="Dar de baja"
          busy={busy}
          onConfirm={remove}
          onClose={() => setConfirm(null)}
        />
      )}
    </div>
  )
}
