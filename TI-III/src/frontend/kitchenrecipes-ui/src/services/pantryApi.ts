import axios from 'axios'
import type {
  AiProfileSettingsDto,
  AssistantChatExchangeDto,
  AssistantChatStreamEventDto,
  AssistantChatThreadDto,
  AssistantChatThreadSummaryDto,
  PantryAssistantChatRequestDto,
  PantryAssistantChatResponseDto,
  PantryAssistantResultDto,
  PantryItemDto,
  RenameAssistantChatThreadRequestDto,
  SendAssistantChatMessageRequestDto,
  UpdateAiProfileRequestDto,
  UpsertPantryItemRequest,
} from '../types/pantry'

const apiClient = axios.create({
  baseURL: '/api',
})

export async function getPantryItems(): Promise<PantryItemDto[]> {
  const response = await apiClient.get<PantryItemDto[]>('/pantry-items')
  return response.data
}

export async function getPantryItemById(id: number): Promise<PantryItemDto> {
  const response = await apiClient.get<PantryItemDto>(`/pantry-items/${id}`)
  return response.data
}

export async function createPantryItem(request: UpsertPantryItemRequest): Promise<PantryItemDto> {
  const response = await apiClient.post<PantryItemDto>('/pantry-items', request)
  return response.data
}

export async function updatePantryItem(id: number, request: UpsertPantryItemRequest): Promise<void> {
  await apiClient.put(`/pantry-items/${id}`, request)
}

export async function deletePantryItem(id: number): Promise<void> {
  await apiClient.delete(`/pantry-items/${id}`)
}

export async function getPantryAssistantSuggestions(limit = 5): Promise<PantryAssistantResultDto> {
  const response = await apiClient.get<PantryAssistantResultDto>(`/assistant/suggestions?limit=${limit}`)
  return response.data
}

export async function sendPantryAssistantMessage(request: PantryAssistantChatRequestDto): Promise<PantryAssistantChatResponseDto> {
  const response = await apiClient.post<PantryAssistantChatResponseDto>('/assistant/chat', request)
  return response.data
}

export async function getAssistantThreads(): Promise<AssistantChatThreadSummaryDto[]> {
  const response = await apiClient.get<AssistantChatThreadSummaryDto[]>('/assistant/threads')
  return response.data
}

export async function getAssistantThread(threadId: number): Promise<AssistantChatThreadDto> {
  const response = await apiClient.get<AssistantChatThreadDto>(`/assistant/threads/${threadId}`)
  return response.data
}

export async function sendAssistantThreadMessage(request: SendAssistantChatMessageRequestDto): Promise<AssistantChatExchangeDto> {
  const response = await apiClient.post<AssistantChatExchangeDto>('/assistant/threads/message', request)
  return response.data
}

export async function renameAssistantThread(threadId: number, request: RenameAssistantChatThreadRequestDto): Promise<void> {
  await apiClient.patch(`/assistant/threads/${threadId}`, request)
}

export async function deleteAssistantThread(threadId: number): Promise<void> {
  await apiClient.delete(`/assistant/threads/${threadId}`)
}

export async function streamAssistantThreadMessage(
  request: SendAssistantChatMessageRequestDto,
  onEvent: (event: AssistantChatStreamEventDto) => void,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch('/api/assistant/threads/message/stream', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'same-origin',
    body: JSON.stringify(request),
    signal,
  })

  if (!response.ok || !response.body) {
    throw new Error('Unable to stream assistant reply.')
  }

  const reader = response.body.getReader()
  const decoder = new TextDecoder()
  let buffer = ''

  while (true) {
    const { done, value } = await reader.read()
    if (done) {
      break
    }

    buffer += decoder.decode(value, { stream: true })
    const lines = buffer.split('\n')
    buffer = lines.pop() ?? ''

    for (const line of lines) {
      const trimmed = line.trim()
      if (!trimmed) {
        continue
      }

      const parsed = JSON.parse(trimmed) as AssistantChatStreamEventDto
      onEvent(parsed)
    }
  }

  if (buffer.trim()) {
    const parsed = JSON.parse(buffer.trim()) as AssistantChatStreamEventDto
    onEvent(parsed)
  }
}

export async function getAiProfileSettings(): Promise<AiProfileSettingsDto> {
  const response = await apiClient.get<AiProfileSettingsDto>('/assistant/profile')
  return response.data
}

export async function updateAiProfileSettings(request: UpdateAiProfileRequestDto): Promise<AiProfileSettingsDto> {
  const response = await apiClient.put<AiProfileSettingsDto>('/assistant/profile', request)
  return response.data
}
