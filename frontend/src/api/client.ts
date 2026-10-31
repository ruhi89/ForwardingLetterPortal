import axios from 'axios'

export const TOKEN_KEY = 'flp_token'

const client = axios.create({ baseURL: import.meta.env.VITE_API_URL })

client.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

client.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401 && localStorage.getItem(TOKEN_KEY)) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem('flp_user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  },
)

export function errorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    if (!err.response) return 'Unable to reach the server. Check your connection and try again.'
    const data = err.response.data
    if (typeof data === 'string' && data.trim() && !data.trim().startsWith('<')) return data
    if (data?.errors && typeof data.errors === 'object') {
      const msgs = Object.values(data.errors).flat().filter((m): m is string => typeof m === 'string')
      if (msgs.length) return msgs.join(' ')
    }
    return data?.message ?? data?.detail ?? data?.title ?? `Request failed (${err.response.status}).`
  }
  return 'Something went wrong.'
}

export default client
