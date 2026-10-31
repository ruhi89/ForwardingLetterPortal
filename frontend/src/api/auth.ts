import client from './client'

export interface VerifyAccountRequest {
  accountNumber: string
  email: string
  dateOfBirth: string
}

export interface VerifyAccountResponse {
  success: boolean
  fullName?: string
  branchName?: string
  message?: string
}

export interface VerifyOtpResponse {
  success: boolean
  registrationToken?: string
  message?: string
}

export interface LoginResponse {
  token: string
  fullName: string
  username: string
}

export const verifyAccount = (data: VerifyAccountRequest) =>
  client.post<VerifyAccountResponse>('/api/auth/verify', data).then((r) => r.data)

export const verifyOtp = (accountNumber: string, otp: string) =>
  client.post<VerifyOtpResponse>('/api/auth/verify-otp', { accountNumber, otp }).then((r) => r.data)

export const register = (
  data: VerifyAccountRequest & { username: string; password: string; registrationToken: string },
) => client.post<LoginResponse>('/api/auth/register', data).then((r) => r.data)

export const login = (username: string, password: string) =>
  client.post<LoginResponse>('/api/auth/login', { username, password }).then((r) => r.data)
