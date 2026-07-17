export type NewsletterSubscriberDto = {
  id: number
  email: string
  isConfirmed: boolean
  createdAtUtc: string
  updatedAtUtc: string | null
}

export type UpsertNewsletterSubscriberRequest = {
  email: string
  isConfirmed: boolean
}

export type NewsletterTemplateDto = {
  key: string
  name: string
  description: string
  subject: string
  htmlBody: string
  plainTextBody: string
}

export type SendNewsletterCampaignRequest = {
  templateKey: string
}

export type SendNewsletterCampaignResult = {
  templateKey: string
  recipientsCount: number
  sentAtUtc: string
}

export type NewsletterCampaignStatsDto = {
  totalSubscribers: number
  confirmedSubscribers: number
  totalCampaignsSent: number
  lastCampaignSentAtUtc: string | null
}

export type NewsletterCampaignHistoryItemDto = {
  id: number
  templateKey: string
  recipientsCount: number
  sentAtUtc: string
}

export type NewsletterCampaignHistoryFilters = {
  templateKey?: string
  sentFromUtc?: string
  sentToUtc?: string
  limit?: number
}
