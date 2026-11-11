import { createContext, useContext, useState } from 'react'

const CartContext = createContext(null)

// Carrito de la venta en curso: vive acá para que el badge del menú lo vea desde cualquier pantalla.
// Cada línea es { productId, quantity } y quantity es texto, porque se edita a mano.
export function CartProvider({ children }) {
  const [cart, setCart] = useState([])

  return <CartContext.Provider value={{ cart, setCart }}>{children}</CartContext.Provider>
}

export const useCart = () => useContext(CartContext)
