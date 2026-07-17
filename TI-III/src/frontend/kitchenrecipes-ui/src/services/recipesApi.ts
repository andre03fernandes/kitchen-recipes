import axios from 'axios'
import type { RecipeDto, UpsertRecipeRequest } from '../types/recipes'

const apiClient = axios.create({
  baseURL: '/api',
})

export async function getRecipes(): Promise<RecipeDto[]> {
  const response = await apiClient.get<RecipeDto[]>('/recipes')
  return response.data
}

export async function getRecipeById(id: number): Promise<RecipeDto> {
  const response = await apiClient.get<RecipeDto>(`/recipes/${id}`)
  return response.data
}

export async function createRecipe(request: UpsertRecipeRequest): Promise<RecipeDto> {
  const response = await apiClient.post<RecipeDto>('/recipes', request)
  return response.data
}

export async function updateRecipe(id: number, request: UpsertRecipeRequest): Promise<void> {
  await apiClient.put(`/recipes/${id}`, request)
}

export async function deleteRecipe(id: number): Promise<void> {
  await apiClient.delete(`/recipes/${id}`)
}
