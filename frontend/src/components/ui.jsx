// Colores y estilos compartidos, tomados del diseño de Claude Design
export const C = {
  navy: '#111a2e', blue: '#1f66e0', green: '#1f9d55', amber: '#e8961c',
  red: '#dc3a3a', purple: '#7444e0', teal: '#1b9688', indigo: '#4f52d6',
}

export const card = { background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: '22px 24px' }
export const tableCard = { background: '#fff', border: '1px solid #e6e9ef', borderRadius: 14, overflow: 'auto' }
export const h1 = { margin: 0, fontSize: 28, letterSpacing: '-0.02em' }
export const subtitle = { margin: '4px 0 0', color: '#667085' }
export const inputBox = { height: 44, padding: '0 14px', border: '1px solid #e1e5ec', borderRadius: 10, fontSize: 15, background: '#fff' }
export const thStyle = { fontSize: 12, fontWeight: 700, color: '#667085', textTransform: 'uppercase', letterSpacing: '.04em' }
export const errorBox = { background: '#fdecec', color: '#b42323', padding: '10px 14px', borderRadius: 10, fontSize: 14 }

export const primaryBtn = {
  height: 44, padding: '0 18px', border: 0, borderRadius: 10, background: C.blue, color: '#fff',
  fontWeight: 700, fontSize: 15, cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 8,
}
export const outlineBtn = {
  height: 46, padding: '0 18px', border: '1px solid #d5dae3', borderRadius: 10, background: '#fff', fontWeight: 600, cursor: 'pointer',
}
export const iconBtn = (bg, fg, size = 36) => ({
  width: size, height: size, border: 0, borderRadius: 8, background: bg, color: fg, cursor: 'pointer', fontSize: 20,
})

// Badge de estado de stock: sin stock / stock bajo / stock normal
export function stockBadge(quantity, minimum) {
  if (quantity === 0) return { text: 'Sin stock', bg: '#eceef2', fg: '#6b7385' }
  if (quantity <= minimum) return { text: `Stock bajo: ${quantity}`, bg: '#fdf0db', fg: '#a3620a' }
  return { text: `Stock: ${quantity}`, bg: '#e3f4ea', fg: '#17783f' }
}

export function Icon({ name, style, className = '' }) {
  return <span className={`icon ${className}`} style={style}>{name}</span>
}

export function Pill({ children, bg, fg, style }) {
  return (
    <span style={{ fontSize: 13, fontWeight: 700, padding: '5px 11px', borderRadius: 20, background: bg, color: fg, whiteSpace: 'nowrap', ...style }}>
      {children}
    </span>
  )
}

export function PageHeader({ title, subtitle: sub, children }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', gap: 16, flexWrap: 'wrap' }}>
      <div>
        <h1 style={h1}>{title}</h1>
        {sub && <p style={subtitle}>{sub}</p>}
      </div>
      {children}
    </div>
  )
}

export function Empty({ children }) {
  return <div style={{ padding: 28, textAlign: 'center', color: '#667085' }}>{children}</div>
}
