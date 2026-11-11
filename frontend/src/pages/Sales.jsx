import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/endpoints'
import { useBranches } from '../context/BranchContext'
import { useCart } from '../context/CartContext'
import { useToast } from '../context/ToastContext'
import { SaleReceipt } from '../components/Receipts'
import { Icon, Pill, stockBadge } from '../components/ui'
import { money } from '../utils/format'

export default function Sales() {
  const { active, reloadAlerts } = useBranches()
  const { cart, setCart } = useCart()
  const toast = useToast()
  const navigate = useNavigate()
  const [items, setItems] = useState([])
  const [q, setQ] = useState('')
  const [busy, setBusy] = useState(false)
  const [receipt, setReceipt] = useState(null)

  const load = useCallback(() => {
    if (active) api.stock(active.id).then(setItems).catch((e) => toast(e.message, 'err'))
  }, [active, toast])
  useEffect(load, [load])

  const byId = Object.fromEntries(items.map((i) => [i.productId, i]))
  const term = q.toLowerCase().trim()
  const visible = items.filter((i) => !term || `${i.name} ${i.brand} ${i.category}`.toLowerCase().includes(term))

  const add = (item) => {
    const cur = cart.find((c) => c.productId === item.productId)
    if (!cur) return setCart([...cart, { productId: item.productId, quantity: '1' }])
    if (Number(cur.quantity) >= item.quantity) return toast(`Stock máximo disponible: ${item.quantity}`, 'err')
    setCart(cart.map((c) => (c.productId === item.productId ? { ...c, quantity: String((Number(c.quantity) || 0) + 1) } : c)))
  }
  const setQty = (productId, quantity) => setCart(cart.map((c) => (c.productId === productId ? { ...c, quantity } : c)))
  const remove = (productId) => setCart(cart.filter((c) => c.productId !== productId))

  // Líneas con su validación (cantidad entera > 0 y dentro del stock disponible)
  let total = 0
  let hasError = false
  const lines = cart
    .filter((c) => byId[c.productId])
    .map((c) => {
      const item = byId[c.productId]
      const n = parseInt(c.quantity, 10)
      let error = null
      if (!(n > 0) || String(n) !== c.quantity.trim()) error = 'Ingresá una cantidad mayor a 0'
      else if (n > item.quantity) error = `Supera el stock disponible. Máximo: ${item.quantity}`
      if (error) hasError = true
      else total += n * item.price
      return { c, item, n, error }
    })

  const cancel = () => {
    if (cart.length) toast('Venta cancelada. No se modificó el stock.')
    setCart([])
    navigate('/')
  }

  const confirm = async () => {
    setBusy(true)
    try {
      const result = await api.createSale(active.id, lines.map((l) => ({ productId: l.c.productId, quantity: l.n })))
      setCart([])
      setReceipt(result)
      load()
      reloadAlerts()
    } catch (e) {
      toast(e.message, 'err')
      load()
    } finally {
      setBusy(false)
    }
  }

  const disabled = !lines.length || hasError || busy

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0,1.5fr) minmax(320px,1fr)', gap: 24, height: '100%', minHeight: 560 }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 14, minHeight: 0 }}>
        <div style={{ position: 'relative' }}>
          <Icon name="search" style={{ position: 'absolute', left: 16, top: '50%', transform: 'translateY(-50%)', fontSize: 22, color: '#98a1b3' }} />
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Buscar producto por nombre, marca o categoría…"
            style={{ width: '100%', height: 56, padding: '0 16px 0 48px', border: '1px solid #e1e5ec', borderRadius: 12, fontSize: 17, background: '#fff' }}
          />
        </div>
        <div style={{ flex: 1, overflow: 'auto', display: 'flex', flexDirection: 'column', gap: 10, paddingRight: 4 }}>
          {visible.length === 0 && <div style={{ padding: 30, textAlign: 'center', color: '#667085' }}>No se encontraron productos.</div>}
          {visible.map((i) => {
            const b = stockBadge(i.quantity, i.minimumStock)
            const out = i.quantity === 0
            return (
              <button
                key={i.productId}
                onClick={() => add(i)}
                disabled={out}
                className="hv-border"
                style={{ display: 'flex', alignItems: 'center', gap: 16, padding: '14px 18px', background: '#fff', border: '1px solid #e6e9ef', borderRadius: 12, cursor: out ? 'not-allowed' : 'pointer', opacity: out ? 0.55 : 1, textAlign: 'left' }}
              >
                <Icon name="package_2" style={{ width: 52, height: 52, borderRadius: 10, background: '#f1f3f7', display: 'grid', placeItems: 'center', fontSize: 26, color: '#8a93a6' }} />
                <span style={{ flex: 1, minWidth: 0 }}>
                  <span style={{ display: 'block', fontSize: 17, fontWeight: 600, color: '#172033' }}>{i.name}</span>
                  <span style={{ display: 'block', fontSize: 13, color: '#667085' }}>{i.brand} · {i.category}</span>
                </span>
                <span style={{ fontSize: 18, fontWeight: 700, color: '#172033' }}>{money(i.price)}</span>
                <Pill bg={b.bg} fg={b.fg}>{b.text}</Pill>
              </button>
            )
          })}
        </div>
      </div>

      <div style={{ background: '#fff', border: '1px solid #e6e9ef', borderRadius: 16, padding: 20, display: 'flex', flexDirection: 'column', gap: 14, minHeight: 0 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h3 style={{ margin: 0, fontSize: 19 }}>Venta en curso</h3>
          <span style={{ fontSize: 13, color: '#667085' }}>{lines.length ? `${lines.length} ${lines.length === 1 ? 'producto' : 'productos'}` : ''}</span>
        </div>
        <div style={{ flex: 1, overflow: 'auto', display: 'flex', flexDirection: 'column', gap: 10, background: '#f6f7fa', borderRadius: 12, padding: 10 }}>
          {lines.length === 0 && (
            <div style={{ margin: 'auto', textAlign: 'center', color: '#8a93a6', fontSize: 15, padding: '30px 10px' }}>
              <Icon name="shopping_cart" style={{ fontSize: 40 }} />
              <div>Tocá un producto para agregarlo</div>
            </div>
          )}
          {lines.map(({ c, item, n, error }) => (
            <div key={c.productId} style={{ background: '#fff', borderRadius: 10, padding: 12, display: 'flex', flexDirection: 'column', gap: 8 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', gap: 10 }}>
                <div style={{ minWidth: 0 }}>
                  <div style={{ fontWeight: 600, fontSize: 15 }}>{item.name}</div>
                  <div style={{ fontSize: 13, color: '#667085' }}>{money(item.price)} c/u · stock {item.quantity}</div>
                </div>
                <button onClick={() => remove(c.productId)} aria-label="Quitar" style={{ border: 0, background: 'none', color: '#dc3a3a', cursor: 'pointer', height: 28 }}>
                  <Icon name="delete" style={{ fontSize: 22 }} />
                </button>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <button
                  onClick={() => (n <= 1 ? remove(c.productId) : setQty(c.productId, String(n - 1)))}
                  style={{ width: 34, height: 34, borderRadius: '50%', border: 0, background: '#eef0f4', cursor: 'pointer' }}
                >
                  <Icon name="remove" style={{ fontSize: 20 }} />
                </button>
                <input
                  value={c.quantity}
                  onChange={(e) => setQty(c.productId, e.target.value)}
                  inputMode="numeric"
                  style={{ width: 56, height: 34, textAlign: 'center', border: '1px solid #d5dae3', borderRadius: 8, fontSize: 16, fontWeight: 600 }}
                />
                <button
                  onClick={() => (n >= item.quantity ? toast(`Stock máximo disponible: ${item.quantity}`, 'err') : setQty(c.productId, String((n || 0) + 1)))}
                  style={{ width: 34, height: 34, borderRadius: '50%', border: 0, background: '#eef0f4', cursor: 'pointer' }}
                >
                  <Icon name="add" style={{ fontSize: 20 }} />
                </button>
                <strong style={{ marginLeft: 'auto', fontSize: 16 }}>{error ? '—' : money(n * item.price)}</strong>
              </div>
              {error && <div style={{ fontSize: 13, color: '#b42323', background: '#fdecec', padding: '6px 10px', borderRadius: 7 }}>{error}</div>}
            </div>
          ))}
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', borderTop: '1px solid #eef0f4', paddingTop: 14 }}>
          <span style={{ fontSize: 17, fontWeight: 700 }}>TOTAL</span>
          <span style={{ fontSize: 32, fontWeight: 800, color: '#1f9d55', letterSpacing: '-0.02em' }}>{money(total)}</span>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 10 }}>
          <button onClick={cancel} className="hv-danger" style={{ height: 54, border: '1px solid #d5dae3', borderRadius: 12, background: '#fff', fontSize: 15, fontWeight: 600, cursor: 'pointer' }}>
            Cancelar
          </button>
          <button
            onClick={confirm}
            disabled={disabled}
            className="hv-green"
            style={{ height: 54, border: 0, borderRadius: 12, background: '#1f9d55', color: '#fff', fontSize: 17, fontWeight: 700, cursor: 'pointer', opacity: disabled ? 0.5 : 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8 }}
          >
            <Icon name="check" style={{ fontSize: 22 }} />
            {busy ? 'Registrando…' : `Confirmar venta ${lines.length && !hasError ? money(total) : ''}`}
          </button>
        </div>
      </div>

      {receipt && <SaleReceipt result={receipt} branch={active} onClose={() => setReceipt(null)} />}
    </div>
  )
}
