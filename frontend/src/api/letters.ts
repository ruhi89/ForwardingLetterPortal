import client from './client'

export interface CreateLetterRequest {
  purpose: string
  senderName: string
}

export interface LetterRequest {
  requestId: number
  recipientName: string
  recipientBankName: string
  purpose: string
  status: string
  submittedAt: string
  hasGeneratedLetter: boolean
  downloadCount: number
  lastDownloadAt: string | null
}

export const createLetter = (data: CreateLetterRequest) =>
  client.post<LetterRequest>('/api/letters', data).then((r) => r.data)

export const getMyLetters = () => client.get<LetterRequest[]>('/api/letters').then((r) => r.data)

export async function downloadPdf(requestId: number) {
  const res = await client.get(`/api/letters/${requestId}/generate-pdf`, { responseType: 'blob' })
  const url = URL.createObjectURL(res.data)
  const a = document.createElement('a')
  a.href = url
  a.download = `forwarding-letter-${requestId}.pdf`
  a.click()
  URL.revokeObjectURL(url)
}
