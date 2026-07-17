export type AuthUserDto = {
  id: number
  fullName: string
  email: string
  role: string
  isActive: boolean
}

export type RegisterUserRequest = {
  fullName: string
  email: string
  password: string
}

export type LoginRequest = {
  email: string
  password: string
}

export type UpdateUserRoleRequest = {
  role: string
}
