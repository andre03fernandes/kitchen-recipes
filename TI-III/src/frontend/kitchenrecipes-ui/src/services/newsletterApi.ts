import axios from 'axios'
import type {
  NewsletterSubscriberDto,
  NewsletterCampaignStatsDto,
  NewsletterCampaignHistoryItemDto,
  NewsletterCampaignHistoryFilters,
  NewsletterTemplateDto,
  SendNewsletterCampaignRequest,
  SendNewsletterCampaignResult,
  UpsertNewsletterSubscriberRequest,
} from '../types/newsletter'

const apiClient = axios.create({
  baseURL: '/api',
})

export async function getNewsletterSubscribers(): Promise<NewsletterSubscriberDto[]> {
  const response = await apiClient.get<NewsletterSubscriberDto[]>('/newsletter-subscribers')
  return response.data
}

export async function getNewsletterSubscriberById(id: number): Promise<NewsletterSubscriberDto> {
  const response = await apiClient.get<NewsletterSubscriberDto>(`/newsletter-subscribers/${id}`)
  return response.data
}

export async function createNewsletterSubscriber(request: UpsertNewsletterSubscriberRequest): Promise<NewsletterSubscriberDto> {
  const response = await apiClient.post<NewsletterSubscriberDto>('/newsletter-subscribers', request)
  return response.data
}

export async function updateNewsletterSubscriber(id: number, request: UpsertNewsletterSubscriberRequest): Promise<void> {
  await apiClient.put(`/newsletter-subscribers/${id}`, request)
}

export async function deleteNewsletterSubscriber(id: number): Promise<void> {
  await apiClient.delete(`/newsletter-subscribers/${id}`)
}

export async function getNewsletterTemplates(): Promise<NewsletterTemplateDto[]> {
  const response = await apiClient.get<NewsletterTemplateDto[]>('/newsletter-campaigns/templates')
  return response.data
}

export async function sendNewsletterCampaign(request: SendNewsletterCampaignRequest): Promise<SendNewsletterCampaignResult> {
  const response = await apiClient.post<SendNewsletterCampaignResult>('/newsletter-campaigns/send', request)
  return response.data
}

export async function getNewsletterCampaignStats(): Promise<NewsletterCampaignStatsDto> {
  const response = await apiClient.get<NewsletterCampaignStatsDto>('/newsletter-campaigns/stats')
  return response.data
}

function buildHistoryQuery(filters: NewsletterCampaignHistoryFilters): string {
  const params = new URLSearchParams()

  if (filters.templateKey) {
    params.set('templateKey', filters.templateKey)
  }

  if (filters.sentFromUtc) {
    params.set('sentFromUtc', filters.sentFromUtc)
  }

  if (filters.sentToUtc) {
    params.set('sentToUtc', filters.sentToUtc)
  }

  params.set('limit', String(filters.limit ?? 20))
  return params.toString()
}

export async function getNewsletterCampaignHistory(filters: NewsletterCampaignHistoryFilters = {}): Promise<NewsletterCampaignHistoryItemDto[]> {
  const response = await apiClient.get<NewsletterCampaignHistoryItemDto[]>(`/newsletter-campaigns/history?${buildHistoryQuery(filters)}`)
  return response.data
}

export async function exportNewsletterCampaignHistoryCsv(filters: NewsletterCampaignHistoryFilters = {}): Promise<Blob> {
  const response = await apiClient.get(`/newsletter-campaigns/history/export?${buildHistoryQuery({ ...filters, limit: filters.limit ?? 100 })}`, {
    responseType: 'blob',
  })

  return response.data as Blob
}
