import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { TableToolbar } from '../../components/table/TableToolbar'
import { t } from '../../i18n/text'
import { getUsers, login, logout, me, register, updateUserRole } from '../../services/accountApi'
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

    try {
      const user = await register(registerForm)
      setCurrentUser(user)
      onAuthChanged?.(user)
      setRegisterForm(emptyRegisterForm)
      setFeedback(t('account.messages.registerSuccess'))

      if (user.role === 'Admin') {
        await loadUsers()
      }
    } catch {
      setFeedback(t('account.errors.registerFailed'))
    }
  }

  async function onLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback('')

    try {
      const user = await login(loginForm)
      setCurrentUser(user)
      onAuthChanged?.(user)
      setLoginForm(emptyLoginForm)
      setFeedback(t('account.messages.loginSuccess'))

      if (user.role === 'Admin') {
        await loadUsers()
      }
    } catch {
      setFeedback(t('account.errors.loginFailed'))
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
                      <th className="py-2 pr-3">{t('account.fields.actions')}</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-amber-100">
                    {usersPagination.items.map((user) => (
                      <tr key={user.id} className="text-sm text-slate/90">
                        <td className="py-3 pr-3">{user.fullName}</td>
                        <td className="py-3 pr-3">{user.email}</td>
                        <td className="py-3 pr-3">
                          <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-bold uppercase tracking-wide ${user.role === 'Admin' ? 'bg-amber-100 text-amber-800' : 'bg-slate-100 text-slate-700'}`}>
                            {user.role}
                          </span>
                        </td>
                        <td className="py-3 pr-3">
                          <div className="flex gap-2">
                            <button
                              className="rounded-lg border border-pine/30 px-3 py-1 font-semibold text-pine hover:bg-pine/5"
                              onClick={() => {
                                void onChangeRole(user.id, 'User')
                              }}
                              type="button"
                            >
                              {t('account.actions.makeUser')}
                            </button>
                            <button
                              className="rounded-lg border border-amber-300 px-3 py-1 font-semibold text-amber-700 hover:bg-amber-50"
                              onClick={() => {
                                void onChangeRole(user.id, 'Admin')
                              }}
                              type="button"
                            >
                              {t('account.actions.makeAdmin')}
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
    </section>
  )
}
