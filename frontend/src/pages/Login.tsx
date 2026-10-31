import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login } from '../api/auth'
import { errorMessage } from '../api/client'
import { Button, ErrorText, Field } from '../components/ui'
import { useAuth } from '../context/AuthContext'

export default function Login() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const res = await login(username, password)
      signIn(res.token, { username: res.username, fullName: res.fullName })
      navigate('/dashboard')
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen grid lg:grid-cols-2 bg-slate-50">
      <aside className="hidden lg:flex flex-col justify-between bg-gradient-to-br from-indigo-700 via-indigo-600 to-violet-700 p-12 text-white">
        <div className="text-lg font-semibold tracking-tight">Forwarding Letter Portal</div>
        <div>
          <h2 className="text-4xl font-semibold leading-tight">Welcome back.</h2>
          <p className="mt-4 max-w-md text-indigo-100">
            Sign in to manage your forwarding letters and track their status in one place.
          </p>
        </div>
        <p className="text-sm text-indigo-200">Your information is verified securely and never shared.</p>
      </aside>

      <main className="flex items-center justify-center p-6">
        <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl ring-1 ring-slate-200">
          <h1 className="text-2xl font-semibold text-slate-900">Sign in</h1>
          <p className="mt-1 text-sm text-slate-500">Enter your credentials to continue</p>

          <form onSubmit={submit} className="mt-6 space-y-4">
            <ErrorText message={error} />
            <Field label="Username" autoComplete="username" value={username} onChange={(e) => setUsername(e.target.value)} required />
            <Field label="Password" type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} required />
            <Button type="submit" loading={loading}>Sign in</Button>
          </form>

          <p className="mt-6 border-t border-slate-100 pt-4 text-center text-sm text-slate-500">
            New here? <Link to="/register" className="font-medium text-indigo-600 hover:underline">Create an account</Link>
          </p>
        </div>
      </main>
    </div>
  )
}
