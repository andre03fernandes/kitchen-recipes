import { useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { ConfirmDialog } from '../../components/dialogs/ConfirmDialog'
import { PromptDialog } from '../../components/dialogs/PromptDialog'
import { t } from '../../i18n/text'
import { deleteAssistantThread, getAiProfileSettings, getAssistantThread, getAssistantThreads, renameAssistantThread, streamAssistantThreadMessage, updateAiProfileSettings } from '../../services/pantryApi'
import type {
  AiProfileSettingsDto,
  AssistantChatMessageDto,
  AssistantChatStreamEventDto,
  AssistantChatThreadSummaryDto,
} from '../../types/pantry'

const starterPrompts = [
  'What can I cook quickly with what is in my pantry?',
  'How should I use the Recipes and Pantry modules together this week?',
  'Where do I send a newsletter campaign and check campaign history?',
]

export function PantryAssistantPage() {
  const [threads, setThreads] = useState<AssistantChatThreadSummaryDto[]>([])
  const [activeThreadId, setActiveThreadId] = useState<number | null>(null)
  const [messages, setMessages] = useState<AssistantChatMessageDto[]>([])
  const [prompt, setPrompt] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingThread, setIsLoadingThread] = useState(false)
  const [isSending, setIsSending] = useState(false)
  const [feedback, setFeedback] = useState('')
  const [streamingAssistantMessageId, setStreamingAssistantMessageId] = useState<number | null>(null)
  const [profileSettings, setProfileSettings] = useState<AiProfileSettingsDto | null>(null)
  const [isUpdatingProfile, setIsUpdatingProfile] = useState(false)
  const [renameTarget, setRenameTarget] = useState<AssistantChatThreadSummaryDto | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<AssistantChatThreadSummaryDto | null>(null)
  const chatScrollRef = useRef<HTMLDivElement | null>(null)

  const latestAssistantMessage = useMemo(
    () => [...messages].reverse().find((item) => item.role === 'assistant') ?? null,
    [messages],
  )

  const followUpPrompts = latestAssistantMessage?.followUpPrompts ?? []

  const isStreaming = streamingAssistantMessageId !== null

  async function loadThreads() {
    try {
      const data = await getAssistantThreads()
      setThreads(data)
      if (!activeThreadId && data.length > 0) {
        setActiveThreadId(data[0].id)
      }
    } catch {
      setFeedback(t('pantryAssistant.errors.threadLoadFailed'))
    } finally {
      setIsLoading(false)
    }
  }

  async function loadAiProfile() {
    try {
      const settings = await getAiProfileSettings()
      setProfileSettings(settings)
    } catch {
      setFeedback(t('pantryAssistant.errors.profileLoadFailed'))
    }
  }

  async function setProfile(profile: 'fast' | 'quality') {
    if (!profileSettings || profileSettings.activeProfile === profile) {
      return
    }

    setIsUpdatingProfile(true)
    try {
      const updated = await updateAiProfileSettings({ profile })
      setProfileSettings(updated)
    } catch {
      setFeedback(t('pantryAssistant.errors.profileUpdateFailed'))
    } finally {
      setIsUpdatingProfile(false)
    }
  }

  async function openThread(threadId: number) {
    setActiveThreadId(threadId)
    setIsLoadingThread(true)
    try {
      const data = await getAssistantThread(threadId)
      setMessages(data.messages)
      setFeedback('')
    } catch {
      setFeedback(t('pantryAssistant.errors.threadOpenFailed'))
      setMessages([])
    } finally {
      setIsLoadingThread(false)
    }
  }

  function startNewThread() {
    setActiveThreadId(null)
    setMessages([])
    setPrompt('')
    setFeedback('')
  }

  async function renameThread(threadId: number, currentTitle: string, nextTitle: string) {
    const normalizedTitle = nextTitle.trim()
    if (!normalizedTitle || normalizedTitle === currentTitle) {
      return
    }

    try {
      await renameAssistantThread(threadId, { title: normalizedTitle })
      await loadThreads()
    } catch {
      setFeedback(t('pantryAssistant.errors.threadRenameFailed'))
    }
  }

  async function removeThread(threadId: number) {
    try {
      await deleteAssistantThread(threadId)
      setThreads((current) => current.filter((thread) => thread.id !== threadId))
      if (activeThreadId === threadId) {
        startNewThread()
      }
    } catch {
      setFeedback(t('pantryAssistant.errors.threadDeleteFailed'))
    }
  }

  function applyStreamEvent(event: AssistantChatStreamEventDto) {
    if (event.type === 'thread' && event.thread && event.userMessage) {
      const assistantId = -Date.now()
      setStreamingAssistantMessageId(assistantId)

      setActiveThreadId(event.thread.id)
      setThreads((current) => {
        const remaining = current.filter((item) => item.id !== event.thread!.id)
        return [event.thread!, ...remaining]
      })
      setMessages((current) => [
        ...current,
        event.userMessage!,
        {
          id: assistantId,
          role: 'assistant',
          content: '',
          createdAtUtc: new Date().toISOString(),
          pantryHighlights: [],
          suggestedRecipes: [],
          followUpPrompts: [],
        },
      ])
      return
    }

    if (event.type === 'token' && event.token && streamingAssistantMessageId !== null) {
      setMessages((current) =>
        current.map((item) =>
          item.id === streamingAssistantMessageId
            ? {
                ...item,
                content: item.content + event.token,
              }
            : item,
        ),
      )
      return
    }

    if (event.type === 'done' && event.assistantMessage && streamingAssistantMessageId !== null) {
      setMessages((current) =>
        current.map((item) =>
          item.id === streamingAssistantMessageId
            ? event.assistantMessage!
            : item,
        ),
      )
      setStreamingAssistantMessageId(null)
      if (event.thread) {
        setThreads((current) => {
          const remaining = current.filter((item) => item.id !== event.thread!.id)
          return [event.thread!, ...remaining]
        })
      }
    }
  }

  async function submitPrompt(customPrompt?: string) {
    const message = (customPrompt ?? prompt).trim()
    if (!message) {
      setFeedback(t('pantryAssistant.errors.promptRequired'))
      return
    }

    setIsSending(true)
    setFeedback('')

    try {
      await streamAssistantThreadMessage({ threadId: activeThreadId ?? undefined, message }, applyStreamEvent)

      setPrompt('')
    } catch {
      setFeedback(t('pantryAssistant.errors.messageFailed'))
      setStreamingAssistantMessageId(null)
    } finally {
      setIsSending(false)
    }
  }

  useEffect(() => {
    void loadThreads()
    void loadAiProfile()
  }, [])

  useEffect(() => {
    if (!activeThreadId) {
      return
    }

    void openThread(activeThreadId)
  }, [activeThreadId])

  useEffect(() => {
    if (!chatScrollRef.current) {
      return
    }

    chatScrollRef.current.scrollTop = chatScrollRef.current.scrollHeight
  }, [messages, isStreaming])

  return (
    <section className="rounded-2xl border border-emerald-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="font-heading text-3xl text-slate">{t('pantryAssistant.title')}</h2>
          <p className="mt-2 text-slate/80">{t('pantryAssistant.description')}</p>
          <p className="mt-2 max-w-3xl text-sm text-slate/70">{t('pantryAssistant.chat.subtitle')}</p>
          {profileSettings && (
            <div className="mt-3 flex flex-wrap items-center gap-2">
              <span className="text-xs font-bold uppercase tracking-[0.18em] text-slate/65">{t('pantryAssistant.chat.modelProfileTitle')}</span>
              <button
                className={`rounded-full px-3 py-1 text-xs font-semibold transition ${profileSettings.activeProfile === 'fast' ? 'bg-pine text-white' : 'border border-slate-300 bg-white text-slate hover:bg-slate-50'}`}
                disabled={isUpdatingProfile}
                onClick={() => {
                  void setProfile('fast')
                }}
                type="button"
              >
                {t('pantryAssistant.actions.fastMode')}
              </button>
              <button
                className={`rounded-full px-3 py-1 text-xs font-semibold transition ${profileSettings.activeProfile === 'quality' ? 'bg-pine text-white' : 'border border-slate-300 bg-white text-slate hover:bg-slate-50'}`}
                disabled={isUpdatingProfile}
                onClick={() => {
                  void setProfile('quality')
                }}
                type="button"
              >
                {t('pantryAssistant.actions.qualityMode')}
              </button>
              <span className="text-xs text-slate/70">
                {t('pantryAssistant.chat.activeModelLabel')}: {profileSettings.activeModel}
              </span>
            </div>
          )}
        </div>

        <div className="flex flex-wrap gap-2">
          <Link className="rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90" to="/pantry">
            {t('pantryAssistant.actions.backToPantry')}
          </Link>
        </div>
      </div>

      {feedback && <p className="mt-4 rounded-lg bg-amber-50 px-3 py-2 text-sm font-semibold text-ember">{feedback}</p>}

      {isLoading ? (
        <p className="mt-6 text-slate/70">{t('pantryAssistant.messages.loading')}</p>
      ) : (
        <div className="mt-6 grid gap-6 xl:grid-cols-[0.75fr_1.45fr]">
          <aside className="rounded-3xl border border-amber-200 bg-white/90 p-4 shadow-sm">
            <div className="flex items-center justify-between gap-2">
              <p className="text-xs font-bold uppercase tracking-[0.18em] text-slate/65">{t('pantryAssistant.chat.historyTitle')}</p>
              <button
                className="rounded-full border border-pine/30 px-3 py-1 text-xs font-semibold text-pine transition hover:bg-pine/5"
                onClick={startNewThread}
                type="button"
              >
                {t('pantryAssistant.actions.newChat')}
              </button>
            </div>

            <div className="mt-3 space-y-2">
              {threads.length === 0 ? (
                <p className="text-sm text-slate/65">{t('pantryAssistant.messages.noThreads')}</p>
              ) : (
                <div className="max-h-[62vh] space-y-2 overflow-y-auto pr-1">
                  {threads.map((thread) => (
                    <div
                      key={thread.id}
                      className={`w-full rounded-2xl border px-3 py-2 text-left text-sm transition ${thread.id === activeThreadId ? 'border-pine bg-pine/5 text-slate' : 'border-slate-200 bg-white hover:border-amber-300'}`}
                    >
                      <button
                        className="w-full text-left"
                        onClick={() => {
                          setActiveThreadId(thread.id)
                        }}
                        type="button"
                      >
                        <p className="truncate font-semibold">{thread.title}</p>
                        <p className="mt-1 line-clamp-2 text-xs text-slate/70">{thread.lastMessagePreview}</p>
                      </button>
                      <div className="mt-2 flex gap-2">
                        <button
                          className="rounded-lg border border-slate-200 px-2 py-1 text-[11px] font-semibold text-slate transition hover:bg-slate-100"
                          onClick={() => {
                            setRenameTarget(thread)
                          }}
                          type="button"
                        >
                          {t('pantryAssistant.actions.renameChat')}
                        </button>
                        <button
                          className="rounded-lg border border-rose-200 px-2 py-1 text-[11px] font-semibold text-rose-700 transition hover:bg-rose-50"
                          onClick={() => {
                            setDeleteTarget(thread)
                          }}
                          type="button"
                        >
                          {t('pantryAssistant.actions.deleteChat')}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </aside>

          <div className="space-y-4">
            <div className="rounded-2xl border border-emerald-200 bg-emerald-50/40 p-4">
              <p className="text-xs uppercase tracking-[0.18em] text-slate/65">{t('pantryAssistant.chat.starterPrompts')}</p>
              <div className="mt-3 flex flex-wrap gap-2">
                {starterPrompts.map((item) => (
                  <button
                    key={item}
                    className="rounded-full border border-emerald-300 bg-white px-3 py-2 text-sm font-semibold text-pine transition hover:-translate-y-0.5 hover:bg-emerald-50"
                    onClick={() => {
                      void submitPrompt(item)
                    }}
                    type="button"
                  >
                    {item}
                  </button>
                ))}
              </div>
            </div>

            <div className="rounded-3xl border border-amber-200 bg-white/85 p-4 shadow-sm">
              <div className="max-h-[56vh] space-y-3 overflow-y-auto pr-1" ref={chatScrollRef}>
                {messages.length === 0 ? (
                  <div className="rounded-2xl bg-emerald-50 px-4 py-3 text-sm leading-6 text-slate">
                    <p className="text-xs font-bold uppercase tracking-[0.18em] opacity-70">{t('pantryAssistant.chat.assistantLabel')}</p>
                    <p className="mt-2 whitespace-pre-line">{t('pantryAssistant.chat.welcome')}</p>
                  </div>
                ) : (
                  messages.map((entry) => (
                    <div
                      key={entry.id}
                      className={`rounded-2xl px-4 py-3 text-sm leading-6 ${entry.role === 'assistant' ? 'bg-emerald-50 text-slate' : 'ml-auto max-w-[85%] bg-pine text-white'}`}
                    >
                      <p className="text-xs font-bold uppercase tracking-[0.18em] opacity-70">
                        {entry.role === 'assistant' ? t('pantryAssistant.chat.assistantLabel') : t('pantryAssistant.chat.userLabel')}
                      </p>
                      <p className="mt-2 whitespace-pre-line">{entry.content}</p>

                      {entry.role === 'assistant' && entry.pantryHighlights.length > 0 && (
                        <div className="mt-3 rounded-xl border border-emerald-200 bg-white/80 p-3 text-slate">
                          <p className="text-xs font-bold uppercase tracking-[0.18em] text-slate/65">{t('pantryAssistant.chat.fridgeHighlights')}</p>
                          <p className="mt-2 text-sm">{entry.pantryHighlights.join(', ')}</p>
                        </div>
                      )}

                    </div>
                  ))
                )}

                {isLoadingThread && <p className="text-xs text-slate/60">{t('pantryAssistant.messages.loadingThread')}</p>}
                {isStreaming && <p className="text-xs text-slate/60">{t('pantryAssistant.messages.streaming')}</p>}
              </div>

              <div className="mt-4 rounded-2xl border border-slate-200 bg-slate-50/80 p-3">
                <label className="block text-xs font-bold uppercase tracking-[0.18em] text-slate/65" htmlFor="assistant-prompt">
                  {t('pantryAssistant.chat.inputLabel')}
                </label>
                <textarea
                  id="assistant-prompt"
                  className="mt-2 min-h-28 w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate outline-none transition focus:border-pine focus:ring-2 focus:ring-pine/15"
                  onChange={(event) => {
                    setPrompt(event.target.value)
                  }}
                  placeholder={t('pantryAssistant.chat.inputPlaceholder')}
                  value={prompt}
                />

                <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
                  <p className="text-xs text-slate/65">{t('pantryAssistant.chat.helper')}</p>
                  <button
                    className="rounded-xl bg-pine px-4 py-2 font-semibold text-white transition hover:bg-pine/90 disabled:cursor-not-allowed disabled:bg-pine/60"
                    disabled={isSending || isStreaming}
                    onClick={() => {
                      void submitPrompt()
                    }}
                    type="button"
                  >
                    {isSending ? t('pantryAssistant.actions.sending') : t('pantryAssistant.actions.send')}
                  </button>
                </div>
              </div>
            </div>

            {followUpPrompts.length > 0 && (
              <div className="rounded-2xl border border-emerald-200 bg-emerald-50/40 p-4">
                <p className="text-xs uppercase tracking-[0.18em] text-slate/65">{t('pantryAssistant.chat.followUpPrompts')}</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  {followUpPrompts.map((item) => (
                    <button
                      key={item}
                      className="rounded-full border border-emerald-300 bg-white px-3 py-1.5 text-xs font-semibold text-pine transition hover:bg-emerald-50"
                      onClick={() => {
                        void submitPrompt(item)
                      }}
                      type="button"
                    >
                      {item}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      <PromptDialog
        cancelLabel={t('common.cancel')}
        confirmLabel={t('common.save')}
        description={t('pantryAssistant.chat.renamePrompt')}
        isOpen={renameTarget !== null}
        label={t('pantryAssistant.actions.renameChat')}
        onCancel={() => setRenameTarget(null)}
        onSubmit={(value) => {
          if (!renameTarget) {
            return
          }

          void renameThread(renameTarget.id, renameTarget.title, value)
          setRenameTarget(null)
        }}
        placeholder={t('pantryAssistant.actions.renameChat')}
        title={t('pantryAssistant.actions.renameChat')}
        value={renameTarget?.title ?? ''}
      />

      <ConfirmDialog
        cancelLabel={t('common.cancel')}
        confirmLabel={t('common.delete')}
        description={t('pantryAssistant.chat.deleteConfirm')}
        isLoading={false}
        isOpen={deleteTarget !== null}
        onCancel={() => setDeleteTarget(null)}
        onConfirm={() => {
          if (!deleteTarget) {
            return
          }

          void removeThread(deleteTarget.id)
          setDeleteTarget(null)
        }}
        title={t('pantryAssistant.actions.deleteChat')}
      />
    </section>
  )
}
