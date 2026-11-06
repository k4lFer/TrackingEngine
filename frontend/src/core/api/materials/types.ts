export interface Material {
  id: string
  code: string
  name: string
  unit: string
  active: boolean
}

/** Entrada de alta (refleja CreateMaterialRequest del backend). */
export interface CreateMaterialInput {
  code: string
  name: string
  unit: string
}

/** Entrada de edición (refleja UpdateMaterialRequest del backend; sin code). */
export interface UpdateMaterialInput {
  name: string
  unit: string
  active: boolean
}