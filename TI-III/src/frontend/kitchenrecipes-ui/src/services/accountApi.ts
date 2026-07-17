import axios from 'axios'
import type {
  AuthUserDto,
  LoginRequest,
  RegisterUserRequest,
  UpdateUserRoleRequest,
} from '../types/account'

const apiClient = axios.create({
  baseURL: '/api/account',
})

export async function register(request: RegisterUserRequest): Promise<AuthUserDto> {
  const response = await apiClient.post<AuthUserDto>('/register', request)
  return response.data
}

export async function login(request: LoginRequest): Promise<AuthUserDto> {
  const response = await apiClient.post<AuthUserDto>('/login', request)
  return response.data
}

export async function logout(): Promise<void> {
  await apiClient.post('/logout')
}

export async function me(): Promise<AuthUserDto> {
  const response = await apiClient.get<AuthUserDto>('/me')
  return response.data
}

export async function getUsers(): Promise<AuthUserDto[]> {
  const response = await apiClient.get<AuthUserDto[]>('/users')
  return response.data
}

export async function updateUserRole(userId: number, request: UpdateUserRoleRequest): Promise<void> {
  await apiClient.put(`/users/${userId}/role`, request)
}

export async function deleteUser(userId: number): Promise<void> {
  await apiClient.delete(`/users/${userId}`)
}

export async function reactivateUser(userId: number): Promise<void> {
  await apiClient.post(`/users/${userId}/reactivate`)
}
