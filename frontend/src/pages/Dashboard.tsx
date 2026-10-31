import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { downloadPdf, getMyLetters, type LetterRequest } from '../api/letters'
import { useAuth } from '../context/AuthContext'
import { ErrorText } from '../components/ui'

const statusColor: Record<string, string> = {
  Pending:  'bg-amber-50  text-amber-700  ring-amber-200',
  Approved: 'bg-green-50  text-green-700  ring-green-200',
  Rejected: 'bg-red-50    text-red-700    ring-red-200',
  Generated:'bg-indigo-50 text-indigo-700 ring-indigo-200',
}

function StatusBadge({ status }: { status: string }) {
  const cls = statusColor[status] ?? 'bg-slate-100 text-slate-600 ring-slate-200'
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ${cls}`}>
      {status}
    </span>
  )
}

export default function Dashboard() {
  const { user, signOut } = useAuth()
  const [letters, setLetters] = useState<LetterRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [downloading, setDownloading] = useState<number | null>(null)

  useEffect(() => {
    getMyLetters()
      .then(setLetters)
      .catch((e) => setError(errorMessage(e)))
      .finally(() => setLoading(false))
  }, [])

  const download = async (id: number) => {
    setDownloading(id)
    try {
      await downloadPdf(id)
    } catch (e) {
      setError(errorMessage(e))
    } finally {
      setDownloading(null)
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
          <div className="flex items-center gap-4 text-sm">
            <span className="hidden text-slate-500 sm:inline">{user?.fullName}</span>
            <button
              onClick={signOut}
              className="rounded-md px-3 py-1.5 text-sm font-medium text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50 hover:text-slate-900"
            >
              Sign out
            </button>
          </div>
        </div>
      </header>

      {/* ── Page body ─────────────────────────────────────────────── */}
      <main className="mx-auto max-w-6xl px-6 py-8">
        {/* Page header */}
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-slate-900">My requests</h1>
            <p className="mt-0.5 text-sm text-slate-500">
              {loading ? 'Loading…' : `${letters.length} request${letters.length !== 1 ? 's' : ''}`}
            </p>
          </div>
          <Link
            to="/letters/new"
            className="inline-flex items-center gap-1.5 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          >
            <svg className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z" />
            </svg>
            New request
          </Link>
        </div>

        {error && (
          <div className="mb-4">
            <ErrorText message={error} />
          </div>
        )}

        {/* Table card */}
        <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium uppercase tracking-wide text-slate-500">
                  <th className="px-5 py-3">#</th>
                  <th className="px-5 py-3">Recipient</th>
                  <th className="px-5 py-3">Bank</th>
                  <th className="px-5 py-3">Purpose</th>
                  <th className="px-5 py-3">Status</th>
                  <th className="px-5 py-3">Submitted</th>
                  <th className="px-5 py-3" />
                </tr>
              </thead>
              <tbody>
                {loading && (
                  <tr>
                    <td colSpan={7} className="px-5 py-10 text-center text-slate-400">
                      <svg className="mx-auto mb-2 h-5 w-5 animate-spin text-indigo-400" viewBox="0 0 24 24" fill="none">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                      </svg>
                      Loading your requests…
                    </td>
                  </tr>
                )}

                {!loading && letters.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-5 py-14 text-center">
                      <p className="text-slate-400">No requests yet.</p>
                      <Link to="/letters/new" className="mt-2 inline-block text-sm font-medium text-indigo-600 hover:underline">
                        Submit your first request →
                      </Link>
                    </td>
                  </tr>
                )}

                {letters.map((l, i) => (
                  <tr
                    key={l.requestId}
                    className={`transition-colors hover:bg-slate-50 ${i !== 0 ? 'border-t border-slate-100' : ''}`}
                  >
                    <td className="px-5 py-3.5 text-slate-400">{l.requestId}</td>
                    <td className="px-5 py-3.5 font-medium text-slate-900">{l.recipientName}</td>
                    <td className="px-5 py-3.5 text-slate-600">{l.recipientBankName}</td>
                    <td className="px-5 py-3.5 max-w-xs truncate text-slate-600">{l.purpose}</td>
                    <td className="px-5 py-3.5">
                      <StatusBadge status={l.status} />
                    </td>
                    <td className="px-5 py-3.5 text-slate-500">
                      {new Date(l.submittedAt).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })}
                    </td>
                    <td className="px-5 py-3.5 text-right">
                      <button
                        onClick={() => download(l.requestId)}
                        disabled={downloading === l.requestId}
                        className="text-sm font-medium text-indigo-600 hover:underline disabled:opacity-50"
                      >
                        {downloading === l.requestId ? 'Downloading…' : 'Download PDF'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </div>
  )
}
