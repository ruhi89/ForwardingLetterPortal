import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { register, verifyAccount, verifyOtp } from '../api/auth'
import { errorMessage } from '../api/client'
import { Button, ErrorText, Field } from '../components/ui'
import { useAuth } from '../context/AuthContext'

type Step = 'account' | 'otp' | 'credentials'

export default function Register() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [step, setStep] = useState<Step>('account')
  const [accountNumber, setAccountNumber] = useState('')
  const [email, setEmail] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [otp, setOtp] = useState('')
  const otpRefs = useRef<(HTMLInputElement | null)[]>([])
  const [secondsLeft, setSecondsLeft] = useState(0)

  useEffect(() => {
    if (step !== 'otp') return
    const id = setInterval(() => setSecondsLeft((s) => Math.max(0, s - 1)), 1000)
    return () => clearInterval(id)
  }, [step])
  const [registrationToken, setRegistrationToken] = useState('')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [info, setInfo] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const run = async (fn: () => Promise<void>) => {
    setError('')
    setLoading(true)
    try {
      await fn()
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  const sendCode = () =>
    run(async () => {
      const res = await verifyAccount({ accountNumber, email, dateOfBirth })
      if (!res.success) return setError(res.message ?? 'Verification failed.')
      setInfo(res.message ?? 'Code sent.')
      setOtp('')
      setSecondsLeft(600)
      setStep('otp')
    })

  const submitAccount = (e: FormEvent) => {
    e.preventDefault()
    sendCode()
  }

  const submitOtp = (e: FormEvent) => {
    e.preventDefault()
    run(async () => {
      const res = await verifyOtp(accountNumber, otp)
      if (!res.success || !res.registrationToken) return setError(res.message ?? 'Invalid code.')
      setRegistrationToken(res.registrationToken)
      setStep('credentials')
    })
  }

  const submitCredentials = (e: FormEvent) => {
    e.preventDefault()
    run(async () => {
      const res = await register({ accountNumber, email, dateOfBirth, username, password, registrationToken })
      signIn(res.token, { username: res.username, fullName: res.fullName })
      navigate('/dashboard')
    })
  }

  const titles: Record<Step, string> = {
    account: 'Verify your account',
    otp: 'Enter verification code',
    credentials: 'Choose your login',
  }

  const passwordChecks = [
    { label: 'At least 6 characters', ok: password.length >= 6, required: true },
    { label: 'An uppercase and a lowercase letter', ok: /[a-z]/.test(password) && /[A-Z]/.test(password), required: false },
    { label: 'A number', ok: /\d/.test(password), required: false },
    { label: 'A symbol (e.g. ! @ #)', ok: /[^A-Za-z0-9]/.test(password), required: false },
  ]

  const steps: { key: Step; label: string }[] = [
    { key: 'account', label: 'Account' },
    { key: 'otp', label: 'Verify' },
    { key: 'credentials', label: 'Login' },
  ]
  const current = steps.findIndex((s) => s.key === step)

  return (
    <div className="min-h-screen grid lg:grid-cols-2 bg-slate-50">
      <aside className="hidden lg:flex flex-col justify-between bg-gradient-to-br from-indigo-700 via-indigo-600 to-violet-700 p-12 text-white">
        <div className="text-lg font-semibold tracking-tight">Forwarding Letter Portal</div>
        <div>
          <h2 className="text-4xl font-semibold leading-tight">Create your account in three quick steps.</h2>
          <p className="mt-4 max-w-md text-indigo-100">
            Confirm your account details, verify with a one-time code, and choose your login to get started.
          </p>
        </div>
        <p className="text-sm text-indigo-200">Your information is verified securely and never shared.</p>
      </aside>

      <main className="flex items-center justify-center p-6">
        <div className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl ring-1 ring-slate-200">
          <ol className="mb-8 flex items-center">
            {steps.map((s, i) => (
              <li key={s.key} className={`flex items-center ${i < steps.length - 1 ? 'flex-1' : ''}`}>
                <span
                  className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-sm font-medium ${
                    i < current
                      ? 'bg-indigo-600 text-white'
                      : i === current
                        ? 'bg-indigo-600 text-white ring-4 ring-indigo-100'
                        : 'bg-slate-100 text-slate-400'
                  }`}
                >
                  {i < current ? '✓' : i + 1}
                </span>
                <span className={`ml-2 text-xs font-medium ${i <= current ? 'text-slate-900' : 'text-slate-400'}`}>{s.label}</span>
                {i < steps.length - 1 && <span className={`mx-3 h-px flex-1 ${i < current ? 'bg-indigo-600' : 'bg-slate-200'}`} />}
              </li>
            ))}
          </ol>

          <h1 className="text-2xl font-semibold text-slate-900">{titles[step]}</h1>
          <p className="mt-1 text-sm text-slate-500">Step {current + 1} of 3</p>

          <div className="mt-6 space-y-4">
            <ErrorText message={error} />

        {step === 'account' && (
          <form onSubmit={submitAccount} className="space-y-4">
            <Field label="Account number" value={accountNumber} onChange={(e) => setAccountNumber(e.target.value)} required />
            <Field label="Registered email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
            <Field label="Date of birth" type="date" value={dateOfBirth} onChange={(e) => setDateOfBirth(e.target.value)} required />
            <Button type="submit" loading={loading}>Send verification code</Button>
          </form>
        )}

        {step === 'otp' && (
          <form onSubmit={submitOtp} className="space-y-4">
            <p className="text-sm text-slate-500">{info}</p>
            <div>
              <span className="mb-2 block text-sm font-medium text-slate-700">6-digit code</span>
              <div className="flex justify-between gap-2">
                {Array.from({ length: 6 }, (_, i) => (
                  <input
                    key={i}
                    ref={(el) => {
                      otpRefs.current[i] = el
                    }}
                    value={otp[i] ?? ''}
                    inputMode="numeric"
                    autoComplete={i === 0 ? 'one-time-code' : 'off'}
                    maxLength={1}
                    aria-label={`Digit ${i + 1}`}
                    onFocus={(e) => e.target.select()}
                    onChange={(e) => {
                      const d = e.target.value.replace(/\D/g, '').slice(-1)
                      if (!d) return
                      setOtp((otp.slice(0, i) + d + otp.slice(i + 1)).slice(0, 6))
                      otpRefs.current[i + 1]?.focus()
                    }}
                    onKeyDown={(e) => {
                      if (e.key === 'Backspace') {
                        e.preventDefault()
                        setOtp(otp.slice(0, i) + otp.slice(i + 1))
                        if (!otp[i]) otpRefs.current[i - 1]?.focus()
                      } else if (e.key === 'ArrowLeft') otpRefs.current[i - 1]?.focus()
                      else if (e.key === 'ArrowRight') otpRefs.current[i + 1]?.focus()
                    }}
                    onPaste={(e) => {
                      e.preventDefault()
                      const text = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6)
                      setOtp(text)
                      otpRefs.current[Math.min(text.length, 5)]?.focus()
                    }}
                    className="h-12 w-full min-w-0 rounded-lg border border-slate-300 bg-slate-50 text-center text-xl font-semibold text-slate-900 outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-200"
                  />
                ))}
              </div>
            </div>
            <p
              className={`rounded-lg px-3 py-2 text-center text-sm font-medium ${
                secondsLeft === 0 ? 'bg-red-50 text-red-700' : 'bg-indigo-50 text-indigo-700'
              }`}
            >
              {secondsLeft === 0
                ? 'Code expired. Please request a new one.'
                : `Code expires in ${String(Math.floor(secondsLeft / 60)).padStart(2, '0')}:${String(secondsLeft % 60).padStart(2, '0')}`}
            </p>
            <Button type="submit" loading={loading} disabled={otp.length < 6 || secondsLeft === 0}>Verify</Button>
            <button type="button" onClick={sendCode} disabled={loading} className="w-full text-sm text-indigo-600 hover:underline">
              Resend code
            </button>
          </form>
        )}

        {step === 'credentials' && (
          <form onSubmit={submitCredentials} className="space-y-4">
            <Field label="Username" value={username} onChange={(e) => setUsername(e.target.value)} required />
            <Field label="Password" type="password" minLength={6} value={password} onChange={(e) => setPassword(e.target.value)} required />
            {password && (
              <ul className="space-y-1 text-xs" aria-live="polite">
                {passwordChecks.map((c) => (
                  <li key={c.label} className={c.ok ? 'text-green-600' : c.required ? 'text-red-600' : 'text-slate-500'}>
                    {c.ok ? '✓' : c.required ? '✗' : '○'} {c.label}
                    {!c.required && !c.ok && ' (suggested)'}
                  </li>
                ))}
              </ul>
            )}
            <Button type="submit" loading={loading} disabled={password.length < 6}>Create account</Button>
          </form>
        )}
          </div>
          <p className="mt-6 border-t border-slate-100 pt-4 text-center text-sm text-slate-500">
            Already registered? <Link to="/login" className="font-medium text-indigo-600 hover:underline">Sign in</Link>
          </p>
        </div>
      </main>
    </div>
  )
}
