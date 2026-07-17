import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import axios from 'axios'
import { ConfirmDialog } from '../../components/dialogs/ConfirmDialog'
import { TableToolbar } from '../../components/table/TableToolbar'
import { t } from '../../i18n/text'
import { deleteUser, getUsers, login, logout, me, reactivateUser, register, updateUserRole } from '../../services/accountApi'
import type { AuthUserDto } from '../../types/account'
import {
  DEFAULT_TABLE_PAGE_SIZE,
  applySimpleFilter,
  exportItemsToDocx,
  exportItemsToExcel,
  exportItemsToPdf,
  paginateItems,
} from '../../utils/tableTools'

type RegisterFormState = {
  fullName: string
  email: string
  password: string
}

type LoginFormState = {
  email: string
  password: string
}

const emptyRegisterForm: RegisterFormState = {
  fullName: '',
  email: '',
  password: '',
}

const emptyLoginForm: LoginFormState = {
  email: '',
  password: '',
}

type AccountPageProps = {
  onAuthChanged?: (user: AuthUserDto | null) => void
}

export function AccountPage({ onAuthChanged }: AccountPageProps) {
  const [currentUser, setCurrentUser] = useState<AuthUserDto | null>(null)
  const [users, setUsers] = useState<AuthUserDto[]>([])
  const [registerForm, setRegisterForm] = useState<RegisterFormState>(emptyRegisterForm)
  const [loginForm, setLoginForm] = useState<LoginFormState>(emptyLoginForm)
  const [isBusy, setIsBusy] = useState(true)
  const [isExportingUsers, setIsExportingUsers] = useState(false)
  const [userFilterText, setUserFilterText] = useState('')
  const [usersPage, setUsersPage] = useState(1)
  const [usersPageSize, setUsersPageSize] = useState(DEFAULT_TABLE_PAGE_SIZE)
  const [deleteTarget, setDeleteTarget] = useState<AuthUserDto | null>(null)
  const [isDeletingUser, setIsDeletingUser] = useState(false)
  const [feedback, setFeedback] = useState('')

  useEffect(() => {
    void loadCurrentUser()
  }, [])

  async function loadCurrentUser() {
    setIsBusy(true)
    try {
      const user = await me()
      setCurrentUser(user)
      onAuthChanged?.(user)
      if (user.role === 'Admin') {
        await loadUsers()
      }
    } catch {
      setCurrentUser(null)
      setUsers([])
      onAuthChanged?.(null)
    } finally {
      setIsBusy(false)
    }
  }

  async function loadUsers() {
    try {
      const data = await getUsers()
      setUsers(data)
    } catch {
      setFeedback(t('account.errors.loadUsersFailed'))
    }
  }

  async function onRegister(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback('')

    const payload = {
      fullName: registerForm.fullName.trim(),
      email: registerForm.email.trim(),
      password: registerForm.password,
    }

    try {
      const user = await register(payload)
      setCurrentUser(user)
      onAuthChanged?.(user)
      setRegisterForm(emptyRegisterForm)
      setFeedback(t('account.messages.registerSuccess'))

      if (user.role === 'Admin') {
        await loadUsers()
      }
    } catch (error) {
      setFeedback(getApiErrorMessage(error, t('account.errors.registerFailed')))
    }
  }

  async function onLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback('')

    const payload = {
      email: loginForm.email.trim(),
      password: loginForm.password,
    }

    try {
      const user = await login(payload)
      setCurrentUser(user)
      onAuthChanged?.(user)
      setLoginForm(emptyLoginForm)
      setFeedback(t('account.messages.loginSuccess'))

      if (user.role === 'Admin') {
        await loadUsers()
      }
    } catch (error) {
      setFeedback(getApiErrorMessage(error, t('account.errors.loginFailed')))
    }
  }

  async function onLogout() {
    try {
      await logout()
      setCurrentUser(null)
      onAuthChanged?.(null)
      setUsers([])
      setFeedback(t('account.messages.logoutSuccess'))
    } catch {
      setFeedback(t('account.errors.logoutFailed'))
    }
  }

  async function onChangeRole(userId: number, role: string) {
    try {
      await updateUserRole(userId, { role })
      await loadUsers()
      setFeedback(t('account.messages.roleUpdated'))
    } catch {
      setFeedback(t('account.errors.roleUpdateFailed'))
    }
  }

  async function confirmDeleteUser() {
    if (!deleteTarget) {
      return
    }

    setIsDeletingUser(true)

    try {
      await deleteUser(deleteTarget.id)
      await loadUsers()
      setFeedback(t('account.messages.userDeleted'))
      setDeleteTarget(null)
    } catch {
      setFeedback(t('account.errors.userDeleteFailed'))
    } finally {
      setIsDeletingUser(false)
    }
  }

  async function onReactivateUser(userId: number) {
    try {
      await reactivateUser(userId)
      await loadUsers()
      setFeedback(t('account.messages.userReactivated'))
    } catch {
      setFeedback(t('account.errors.userReactivateFailed'))
    }
  }

  async function exportUsersTable(format: 'pdf' | 'docx' | 'excel') {
    const timestamp = new Date().toISOString().replaceAll(':', '-')
    const columns = [
      { header: t('account.fields.fullName'), value: (item: AuthUserDto) => item.fullName },
      { header: t('account.fields.email'), value: (item: AuthUserDto) => item.email },
      { header: t('account.fields.role'), value: (item: AuthUserDto) => item.role },
    ]

    setIsExportingUsers(true)

    try {
      if (format === 'pdf') {
        exportItemsToPdf(filteredUsers, columns, t('account.adminPanelTitle'), `users-${timestamp}.pdf`)
      } else if (format === 'docx') {
        await exportItemsToDocx(filteredUsers, columns, t('account.adminPanelTitle'), `users-${timestamp}.docx`)
      } else {
        exportItemsToExcel(filteredUsers, columns, 'Users', `users-${timestamp}.xlsx`)
      }

      setFeedback(t('tables.exportSuccess'))
    } catch {
      setFeedback(t('tables.exportError'))
    } finally {
      setIsExportingUsers(false)
    }
  }

  const filteredUsers = useMemo(
    () => applySimpleFilter(users, userFilterText, (user) => [user.fullName, user.email, user.role]),
    [userFilterText, users],
  )

  const usersPagination = useMemo(
    () => paginateItems(filteredUsers, usersPage, usersPageSize),
    [filteredUsers, usersPage, usersPageSize],
  )

  if (isBusy) {
    return (
      <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
        <p className="text-slate/70">{t('account.messages.loading')}</p>
      </section>
    )
  }

  return (
    <section className="rounded-2xl border border-amber-300/50 bg-white/80 p-6 shadow-sm backdrop-blur-sm">
      <h2 className="font-heading text-3xl text-slate">{t('account.title')}</h2>
      <p className="mt-2 text-slate/80">{t('account.description')}</p>

      {feedback && <p className="mt-4 rounded-lg bg-amber-50 px-3 py-2 text-sm font-semibold text-ember">{feedback}</p>}

      {currentUser ? (
        <div className="mt-6 grid gap-6">
          <div className="rounded-xl border border-amber-200 bg-amber-50/50 p-4">
            <p className="text-sm font-semibold text-slate/70">{t('account.currentUser')}</p>
            <p className="mt-1 text-lg font-semibold text-slate">{currentUser.fullName}</p>
            <p className="text-sm text-slate/80">{currentUser.email}</p>
            <p className="mt-1 text-sm text-slate/80">{t('account.roleLabel')}: {currentUser.role}</p>
            <button
              className="mt-4 rounded-lg border border-red-300 px-4 py-2 text-sm font-semibold text-red-700 hover:bg-red-50"
              onClick={() => {
                void onLogout()
              }}
              type="button"
            >
              {t('account.actions.logout')}
            </button>
          </div>

          {currentUser.role === 'Admin' && (
            <div className="rounded-xl border border-pine/20 bg-white p-4">
              <h3 className="font-heading text-2xl text-slate">{t('account.adminPanelTitle')}</h3>
              <p className="mt-1 text-sm text-slate/75">{t('account.adminPanelDescription')}</p>

              <div className="mt-4 overflow-x-auto">
                <TableToolbar
                  filterPlaceholder={t('account.fields.fullName')}
                  filterText={userFilterText}
                  isExporting={isExportingUsers}
                  onExportDocx={() => {
                    void exportUsersTable('docx')
                  }}
                  onExportExcel={() => {
                    void exportUsersTable('excel')
                  }}
                  onExportPdf={() => {
                    void exportUsersTable('pdf')
                  }}
                  onFilterTextChange={(value) => {
                    setUserFilterText(value)
                    setUsersPage(1)
                  }}
                  onNextPage={() => setUsersPage((current) => Math.min(usersPagination.totalPages, current + 1))}
                  onPageSizeChange={(value) => {
                    setUsersPageSize(value)
                    setUsersPage(1)
                  }}
                  onPreviousPage={() => setUsersPage((current) => Math.max(1, current - 1))}
                  page={usersPagination.currentPage}
                  pageSize={usersPageSize}
                  totalPages={usersPagination.totalPages}
                />

                <table className="min-w-full divide-y divide-amber-200">
                  <thead>
                    <tr className="text-left text-sm uppercase tracking-wide text-slate/65">
                      <th className="py-2 pr-3">{t('account.fields.fullName')}</th>
                      <th className="py-2 pr-3">{t('account.fields.email')}</th>
                      <th className="py-2 pr-3">{t('account.fields.role')}</th>
                      <th className="py-2 pr-3">{t('account.fields.status')}</th>
                      <th className="py-2 pr-3">{t('account.fields.actions')}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-amber-100">
                    {usersPagination.items.map((user) => (
                      <tr key={user.id} className={`text-sm ${user.isActive ? 'text-slate/90' : 'text-slate/60'}`}>
                        <td className="py-3 pr-3">{user.fullName}</td>
                        <td className="py-3 pr-3">{user.email}</td>
                        <td className="py-3 pr-3">
                          <span
                            className={`inline-flex rounded-full border px-3 py-1 text-xs font-black uppercase tracking-[0.14em] ${
                              user.role === 'Admin'
                                ? 'border-sky-300/70 bg-sky-400/25 text-sky-100'
                                : 'border-amber-300/60 bg-amber-100/25 text-amber-50'
                            }`}
                          >
                            {user.role}
                          </span>
                        </td>
                        <td className="py-3 pr-3">
                          <span className={`inline-flex rounded-full border px-3 py-1 text-xs font-black uppercase tracking-[0.14em] ${user.isActive ? 'border-emerald-300/70 bg-emerald-300/20 text-emerald-100' : 'border-slate-400/70 bg-slate-400/20 text-slate-200'}`}>
                            {user.isActive ? t('account.status.active') : t('account.status.inactive')}
                          </span>
                        </td>
                        <td className="py-3 pr-3">
                          <div className="flex gap-2">
                            <button
                              className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5 disabled:cursor-not-allowed disabled:opacity-60"
                              disabled={!user.isActive}
                              onClick={() => {
                                void onChangeRole(user.id, 'User')
                              }}
                              type="button"
                            >
                              {t('account.actions.makeUser')}
                            </button>
                            <button
                              className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-amber-700 hover:bg-amber-50 disabled:cursor-not-allowed disabled:opacity-60"
                              disabled={!user.isActive}
                              onClick={() => {
                                void onChangeRole(user.id, 'Admin')
                              }}
                              type="button"
                            >
                              {t('account.actions.makeAdmin')}
                            </button>
                            <button
                              className="rounded-lg border border-red-300 px-3 py-1 font-semibold text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
                              disabled={currentUser?.id === user.id || !user.isActive}
                              onClick={() => {
                                setDeleteTarget(user)
                              }}
                              type="button"
                            >
                              {currentUser?.id === user.id ? t('account.actions.currentUser') : t('account.actions.deleteUser')}
                            </button>
                            <button
                              className="rounded-lg border border-emerald-300 px-3 py-1 font-semibold text-emerald-700 hover:bg-emerald-50 disabled:cursor-not-allowed disabled:opacity-60"
                              disabled={user.isActive}
                              onClick={() => {
                                void onReactivateUser(user.id)
                              }}
                              type="button"
                            >
                              {t('account.actions.reactivateUser')}
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      ) : (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <form className="rounded-xl border border-amber-200 bg-amber-50/40 p-4" onSubmit={onRegister}>
            <h3 className="font-heading text-2xl text-slate">{t('account.registerTitle')}</h3>
            <p className="mt-1 text-sm text-slate/75">{t('account.registerDescription')}</p>
            <div className="mt-4 grid gap-3">
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                onChange={(event) => setRegisterForm((current) => ({ ...current, fullName: event.target.value }))}
                placeholder={t('account.fields.fullName')}
                required
                type="text"
                value={registerForm.fullName}
              />
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                onChange={(event) => setRegisterForm((current) => ({ ...current, email: event.target.value }))}
                placeholder={t('account.fields.email')}
                required
                type="email"
                value={registerForm.email}
              />
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                minLength={6}
                onChange={(event) => setRegisterForm((current) => ({ ...current, password: event.target.value }))}
                placeholder={t('account.fields.password')}
                required
                type="password"
                value={registerForm.password}
              />
              <button className="rounded-lg bg-pine px-4 py-2 font-semibold text-white hover:bg-pine/90" type="submit">
                {t('account.actions.register')}
              </button>
            </div>
          </form>

          <form className="rounded-xl border border-pine/20 bg-white p-4" onSubmit={onLogin}>
            <h3 className="font-heading text-2xl text-slate">{t('account.loginTitle')}</h3>
            <p className="mt-1 text-sm text-slate/75">{t('account.loginDescription')}</p>
            <div className="mt-4 grid gap-3">
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                onChange={(event) => setLoginForm((current) => ({ ...current, email: event.target.value }))}
                placeholder={t('account.fields.email')}
                required
                type="email"
                value={loginForm.email}
              />
              <input
                className="rounded-lg border border-amber-300 bg-white px-3 py-2 text-base"
                onChange={(event) => setLoginForm((current) => ({ ...current, password: event.target.value }))}
                placeholder={t('account.fields.password')}
                required
                type="password"
                value={loginForm.password}
              />
              <button className="rounded-lg bg-pine px-4 py-2 font-semibold text-white hover:bg-pine/90" type="submit">
                {t('account.actions.login')}
              </button>
            </div>
          </form>
        </div>
      )}

      <ConfirmDialog
        cancelLabel={t('common.cancel')}
        confirmLabel={t('account.actions.deleteUser')}
        description={deleteTarget ? t('account.messages.deleteConfirm') : ''}
        isLoading={isDeletingUser}
        isOpen={deleteTarget !== null}
        onCancel={() => {
          if (!isDeletingUser) {
            setDeleteTarget(null)
          }
        }}
        onConfirm={() => {
          void confirmDeleteUser()
        }}
        title={t('account.actions.deleteUser')}
      />
    </section>
  )
}

function getApiErrorMessage(error: unknown, fallback: string) {
  if (!axios.isAxiosError(error)) {
    return fallback
  }

  const serverMessage = error.response?.data?.message
  if (typeof serverMessage === 'string' && serverMessage.trim().length > 0) {
    return serverMessage
  }

  return fallback
}
