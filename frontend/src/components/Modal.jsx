import { Icon, outlineBtn } from './ui'

export function Modal({ title, onClose, width = 520, children }) {
  return (
    <div
      onClick={onClose}
      style={{ position: 'fixed', inset: 0, background: 'rgba(17,26,46,.5)', display: 'grid', placeItems: 'center', padding: 24, zIndex: 50 }}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          width: '100%', maxWidth: width, maxHeight: '90vh', overflow: 'auto', background: '#fff', borderRadius: 18, padding: 26,
          display: 'flex', flexDirection: 'column', gap: 16, animation: 'gin .2s ease', boxShadow: '0 20px 50px rgba(17,26,46,.25)',
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 12 }}>
          <h2 style={{ margin: 0, fontSize: 22, letterSpacing: '-0.01em' }}>{title}</h2>
          <button
            onClick={onClose}
            aria-label="Cerrar"
            style={{ border: 0, background: '#f1f3f7', width: 36, height: 36, borderRadius: '50%', cursor: 'pointer' }}
          >
            <Icon name="close" style={{ fontSize: 20 }} />
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}

export function ConfirmModal({ title, text, warn, actionLabel, busy, onConfirm, onClose }) {
  return (
    <Modal title={title} onClose={onClose}>
      <p style={{ margin: 0, fontSize: 16, lineHeight: 1.5, color: '#3b4a66' }}>{text}</p>
      {warn && (
        <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start', background: '#fdf3e3', color: '#94590a', padding: '10px 12px', borderRadius: 10, fontSize: 14 }}>
          <Icon name="warning" style={{ fontSize: 20 }} />
          {warn}
        </div>
      )}
      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
        <button onClick={onClose} style={outlineBtn}>Cancelar</button>
        <button
          onClick={onConfirm}
          disabled={busy}
          style={{ height: 46, padding: '0 18px', border: 0, borderRadius: 10, background: '#dc3a3a', color: '#fff', fontWeight: 700, cursor: 'pointer' }}
        >
          {busy ? 'Procesando…' : actionLabel}
        </button>
      </div>
    </Modal>
  )
}

// Formulario genérico. fields: [{ key, label, type, options, full, placeholder, list }]
export function FormModal({ title, fields, values, errors = {}, onChange, onSave, onClose, busy, width = 600, categories }) {
  return (
    <Modal title={title} onClose={onClose} width={width}>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14 }}>
        {fields.map((f) => {
          const boxStyle = {
            height: 44, padding: f.options ? '0 10px' : '0 12px', border: `1px solid ${errors[f.key] ? '#dc3a3a' : '#d5dae3'}`,
            borderRadius: 9, fontSize: 15, background: '#fff',
          }
          return (
            <label
              key={f.key}
              style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: 13, fontWeight: 600, color: '#3b4a66', gridColumn: f.full ? '1 / -1' : 'auto' }}
            >
              {f.label}
              {f.options ? (
                <select value={values[f.key] ?? ''} onChange={(e) => onChange(f.key, e.target.value)} style={boxStyle}>
                  {f.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
              ) : (
                <input
                  type={f.type ?? 'text'}
                  value={values[f.key] ?? ''}
                  onChange={(e) => onChange(f.key, e.target.value)}
                  placeholder={f.placeholder}
                  list={f.list}
                  style={boxStyle}
                />
              )}
              {errors[f.key] && <span style={{ fontSize: 12, fontWeight: 500, color: '#b42323' }}>{errors[f.key]}</span>}
            </label>
          )
        })}
      </div>
      {categories && <datalist id="dl-cats">{categories.map((c) => <option key={c} value={c} />)}</datalist>}
      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 4 }}>
        <button onClick={onClose} style={outlineBtn}>Cancelar</button>
        <button
          onClick={onSave}
          disabled={busy}
          className="hv-primary"
          style={{ height: 46, padding: '0 22px', border: 0, borderRadius: 10, background: '#1f66e0', color: '#fff', fontWeight: 700, cursor: 'pointer' }}
        >
          {busy ? 'Guardando…' : 'Guardar'}
        </button>
      </div>
    </Modal>
  )
}
