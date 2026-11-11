// El backend manda el rubro como texto del enum Industry; acá se traduce para mostrarlo
export const INDUSTRIES = [
  { value: 'Kiosk', label: 'Kiosco' },
  { value: 'Grocery', label: 'Almacén' },
  { value: 'Hardware', label: 'Ferretería' },
  { value: 'Clothing', label: 'Indumentaria' },
  { value: 'HairSalon', label: 'Peluquería' },
  { value: 'Other', label: 'Otro' },
]

export const industryLabel = (value) => INDUSTRIES.find((i) => i.value === value)?.label ?? value
