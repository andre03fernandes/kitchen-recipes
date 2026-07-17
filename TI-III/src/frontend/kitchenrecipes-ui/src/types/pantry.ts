export type PantryItemDto = {
  id: number
  name: string
  imageUrl: string | null
  availableQuantity: number
  unit: string
  expirationDate: string | null
  createdAtUtc: string
  updatedAtUtc: string | null
}

export type UpsertPantryItemRequest = {
  name: string
  imageUrl: string | null
  availableQuantity: number
  unit: string
  expirationDate: string | null
}

export type PantryRecipeSuggestionDto = {
  recipeId: number
  recipeName: string
  recipeDescription: string
  preparationMinutes: number
  servings: number
  matchScore: number
  quantityCoverageScore: number
  matchedPantryItems: string[]
  missingIngredients: string[]
  insufficientIngredients: string[]
  conversionInsights: string[]
  suggestionReason: string
}

export type PantryAssistantChatRequestDto = {
  message: string
  limit?: number
}

export type PantryAssistantChatResponseDto = {
  userMessage: string
  assistantMessage: string
  pantryItemsCount: number
  recipesEvaluated: number
  pantryHighlights: string[]
  suggestedRecipes: PantryRecipeSuggestionDto[]
  followUpPrompts: string[]
}

export type PantryAssistantResultDto = {
  pantryItemsCount: number
  recipesEvaluated: number
  suggestions: PantryRecipeSuggestionDto[]
}

export type AssistantChatMessageDto = {
  id: number
  role: string
  content: string
  createdAtUtc: string
  pantryHighlights: string[]
  suggestedRecipes: PantryRecipeSuggestionDto[]
  followUpPrompts: string[]
}

export type AssistantChatThreadSummaryDto = {
  id: number
  title: string
  lastMessagePreview: string
  createdAtUtc: string
  updatedAtUtc: string
}

export type AssistantChatThreadDto = {
  id: number
  title: string
  createdAtUtc: string
  updatedAtUtc: string
  messages: AssistantChatMessageDto[]
}

export type SendAssistantChatMessageRequestDto = {
  threadId?: number
  message: string
}

export type RenameAssistantChatThreadRequestDto = {
  title: string
}

export type AssistantChatExchangeDto = {
  thread: AssistantChatThreadSummaryDto
  userMessage: AssistantChatMessageDto
  assistantMessage: AssistantChatMessageDto
}

export type AssistantChatStreamEventDto = {
  type: 'thread' | 'token' | 'done'
  token: string | null
  thread: AssistantChatThreadSummaryDto | null
  userMessage: AssistantChatMessageDto | null
  assistantMessage: AssistantChatMessageDto | null
}

export type AiProfileSettingsDto = {
  enabled: boolean
  provider: string
  activeProfile: 'fast' | 'quality'
  fastModel: string
  qualityModel: string
  activeModel: string
}

export type UpdateAiProfileRequestDto = {
  profile: 'fast' | 'quality'
}
