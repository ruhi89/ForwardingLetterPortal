import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { createLetter, type CreateLetterRequest } from '../api/letters'
import { Button, ErrorText, Field } from '../components/ui'

const empty: CreateLetterRequest = {
  recipientName: '',
  recipientBankName: '',
  recipientBranchName: '',
  recipientAddress: '',
  purpose: '',
  documentDescription: '',
}

export default function NewLetter() {
  const navigate = useNavigate()
  const [form, setForm] = useState(empty)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const set = (key: keyof CreateLetterRequest) => (e: { target: { value: string } }) =>
    setForm((f) => ({ ...f, [key]: e.target.value }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await createLetter(form)
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
                Fill in the recipient details and purpose so the letter can be generated and dispatched to the correct bank branch.
              </p>
              <ul className="mt-6 space-y-3 text-sm text-indigo-100">
                {[
                  'Use the official bank name as it appears on documents',
                  'Include the full branch address when known',
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
                <legend className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-400">Recipient</legend>
                <Field label="Recipient name" value={form.recipientName} onChange={set('recipientName')} required />
                <div className="grid gap-4 sm:grid-cols-2">
                  <Field label="Bank" value={form.recipientBankName} onChange={set('recipientBankName')} required />
                  <Field label="Branch" value={form.recipientBranchName} onChange={set('recipientBranchName')} required />
                </div>
                <Field label="Address" value={form.recipientAddress} onChange={set('recipientAddress')} required />
              </fieldset>

              <fieldset className="space-y-4">
                <legend className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-400">Request</legend>
                <Field label="Purpose" value={form.purpose} onChange={set('purpose')} required />
                <Field label="Document description" value={form.documentDescription} onChange={set('documentDescription')} required />
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
