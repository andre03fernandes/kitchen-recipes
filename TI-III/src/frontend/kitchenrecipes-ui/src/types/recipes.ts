export type RecipeDto = {
  id: number
  name: string
  description: string
  imageUrl: string | null
  preparationMinutes: number
  servings: number
  createdAtUtc: string
  updatedAtUtc: string | null
}

export type UpsertRecipeRequest = {
  name: string
  description: string
  imageUrl: string | null
  preparationMinutes: number
  servings: number
}
