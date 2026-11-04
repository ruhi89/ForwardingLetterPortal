import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { createLetter, type CreateLetterRequest } from '../api/letters'
import { Button, ErrorText, Field } from '../components/ui'
import { useAuth } from '../context/AuthContext'

const PURPOSES = ['ATM Card', 'Cheque Book', 'Other']

const empty:CreateLetterRequest = {
  purpose: '',
  senderName: '',
}

export default function NewLetter() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [form, setForm] = useState<CreateLetterRequest>({ ...empty, senderName: user?.fullName ?? '' })
  const [purposeChoice, setPurposeChoice] = useState('')
  const [otherPurpose, setOtherPurpose] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const set = (key: keyof CreateLetterRequest) => (e: { target: { value: string } }) =>
    setForm((f) => ({ ...f, [key]: e.target.value }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await createLetter({
        ...form,
        purpose: purposeChoice === 'Other' ? otherPurpose.trim() : purposeChoice,
      })
      navigate('/dashboard')
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {/* ── Top nav ──────────────────────────────────────────────── */}
      <header className="sticky top-0 z-10 border-b border-slate-200 bg-white/80 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-3">
          <div className="flex items-center gap-2">
            <span className="inline-flex h-7 w-7 items-center justify-center rounded-lg bg-gradient-to-br from-indigo-600 to-violet-600 text-xs font-bold text-white">
              FL
            </span>
            <span className="font-semibold text-slate-900">Forwarding Letter Portal</span>
          </div>
          <Link
            to="/dashboard"
            className="flex items-center gap-1.5 text-sm font-medium text-slate-600 hover:text-slate-900"
          >
            <svg className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path fillRule="evenodd" d="M17 10a.75.75 0 01-.75.75H5.612l4.158 3.96a.75.75 0 11-1.04 1.08l-5.5-5.25a.75.75 0 010-1.08l5.5-5.25a.75.75 0 111.04 1.08L5.612 9.25H16.25A.75.75 0 0117 10z" clipRule="evenodd" />
            </svg>
            Back to dashboard
          </Link>
        </div>
      </header>

      {/* ── Page body ─────────────────────────────────────────────── */}
      <main className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
        <div className="grid items-start gap-8 lg:grid-cols-[320px_1fr]">

          {/* Left info panel */}
          <aside className="hidden lg:block">
            <div className="sticky top-24 rounded-2xl bg-gradient-to-br from-indigo-700 via-indigo-600 to-violet-700 p-8 text-white">
              <h2 className="text-xl font-semibold leading-snug">Before you submit</h2>
              <p className="mt-3 text-sm text-indigo-100 leading-relaxed">
                Enter the sender name and purpose so the letter can be generated.
              </p>
              <ul className="mt-6 space-y-3 text-sm text-indigo-100">
                {[
                  'Use your full name as it appears on bank records',
                  'Be specific about the purpose to avoid delays',
                ].map((tip) => (
                  <li key={tip} className="flex items-start gap-2">
                    <svg className="mt-0.5 h-4 w-4 shrink-0 text-indigo-300" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M16.704 4.153a.75.75 0 01.143 1.052l-8 10.5a.75.75 0 01-1.127.075l-4.5-4.5a.75.75 0 011.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 011.05-.143z" clipRule="evenodd" />
                    </svg>
                    {tip}
                  </li>
                ))}
              </ul>
            </div>
          </aside>

          {/* Form card */}
          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
            <h1 className="text-xl font-semibold text-slate-900">New forwarding letter request</h1>
            <p className="mt-1 text-sm text-slate-500">All fields are required.</p>

            <form onSubmit={submit} className="mt-6 space-y-8">
              <ErrorText message={error} />

              <fieldset className="space-y-4">
                <legend className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-400">Request</legend>
                <Field label="Sender name" value={form.senderName} onChange={set('senderName')} required />
                <label className="block">
                  <span className="block text-sm font-medium text-slate-700 mb-1">Purpose</span>
                  <select
                    value={purposeChoice}
                    onChange={(e) => setPurposeChoice(e.target.value)}
                    required
                    className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200"
                  >
                    <option value="" disabled>Select a purpose</option>
                    {PURPOSES.map((p) => (
                      <option key={p} value={p}>{p}</option>
                    ))}
                  </select>
                </label>
                {purposeChoice === 'Other' && (
                  <Field label="Specify purpose" value={otherPurpose} onChange={(e) => setOtherPurpose(e.target.value)} required />
                )}
              </fieldset>

              <div className="flex flex-col-reverse gap-3 border-t border-slate-100 pt-6 sm:flex-row sm:justify-end">
                <Link
                  to="/dashboard"
                  className="rounded-lg px-4 py-2.5 text-center text-sm font-medium text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50"
                >
                  Cancel
                </Link>
                <div className="sm:w-48">
                  <Button type="submit" loading={loading}>Submit request</Button>
                </div>
              </div>
            </form>
          </div>
        </div>
      </main>
    </div>
  )
}
