import { Modal } from './Modal'
import { Icon } from './ui'
import { branchLabel, money } from '../utils/format'

// Comprobante que se muestra después de confirmar una venta
export function SaleReceipt({ result, branch, onClose }) {
  const { sale, alerts } = result
  return (
    <Modal title={`Comprobante ${sale.code}`} onClose={onClose}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, background: '#e7f5ed', color: '#16683a', padding: '12px 14px', borderRadius: 12 }}>
        <Icon name="check_circle" style={{ fontSize: 26 }} />
        <span>
          <strong>Venta registrada</strong> · {new Date(sale.date).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })} · {branchLabel(branch)}
        </span>
      </div>
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        {sale.items.map((i) => (
          <div key={i.productId} style={{ display: 'flex', justifyContent: 'space-between', gap: 10, padding: '9px 0', borderBottom: '1px dashed #e1e5ec', fontSize: 15 }}>
            <span>{i.quantity} × {i.productName} <span style={{ color: '#667085' }}>({money(i.unitPrice)})</span></span>
            <strong>{money(i.subtotal)}</strong>
          </div>
        ))}
      </div>
      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 20, fontWeight: 800 }}>
        <span>Total</span><span>{money(sale.total)}</span>
      </div>
      {alerts.map((a) => (
        <div key={a} style={{ display: 'flex', gap: 8, alignItems: 'center', background: '#fdf3e3', color: '#94590a', padding: '9px 12px', borderRadius: 10, fontSize: 14 }}>
          <Icon name="warning" style={{ fontSize: 20 }} />Stock bajo: {a}
        </div>
      ))}
      <button onClick={onClose} style={{ height: 50, border: 0, borderRadius: 12, background: '#1f9d55', color: '#fff', fontSize: 16, fontWeight: 700, cursor: 'pointer' }}>
        Nueva venta
      </button>
    </Modal>
  )
}

export function PurchaseReceipt({ purchase, branch, onClose }) {
  return (
    <Modal title={`Comprobante ${purchase.code}`} onClose={onClose}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, background: '#e3f4f2', color: '#106b61', padding: '12px 14px', borderRadius: 12 }}>
        <Icon name="inventory" style={{ fontSize: 26 }} />
        <span><strong>Compra registrada</strong> · {purchase.supplier || 'Sin proveedor'} · {branchLabel(branch)}</span>
      </div>
      {purchase.items.map((i) => (
        <div key={i.productId} style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1fr) auto auto', gap: 14, padding: '9px 0', borderBottom: '1px dashed #e1e5ec', fontSize: 15 }}>
          <span>+{i.quantity} × {i.productName}</span>
          <span style={{ color: '#667085' }}>{money(i.unitCost)} c/u</span>
          <strong>Stock: {i.newStock}</strong>
        </div>
      ))}
      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 20, fontWeight: 800 }}>
        <span>Total</span><span>{money(purchase.total)}</span>
      </div>
      <button onClick={onClose} style={{ height: 50, border: 0, borderRadius: 12, background: '#1b9688', color: '#fff', fontSize: 16, fontWeight: 700, cursor: 'pointer' }}>
        Listo
      </button>
    </Modal>
  )
}
