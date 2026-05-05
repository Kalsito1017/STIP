import { useState, useRef, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { UserPlus, Loader2, Mail, Lock, User, MailCheck, ArrowRight, Pencil } from 'lucide-react';
import { useRegister } from '../hooks/useAuth';
import { Input } from '../components/ui/input';
import { Button } from '../components/ui/button';
import { Card, CardContent } from '../components/ui/card';
import { useTranslation } from 'react-i18next';
import type { AxiosError } from 'axios';

export function RegisterPage() {
  const { t } = useTranslation('auth');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const emailInputRef = useRef<HTMLInputElement>(null);
  const { mutate, isPending, error } = useRegister();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    mutate({ email, password, fullName });
  };

  const serverError =
    error instanceof Error
      ? ((error as AxiosError<{ error?: string; details?: string[] }>).response?.data?.details?.join?.(', ')
        ?? (error as AxiosError<{ error?: string }>).response?.data?.error
        ?? (error as AxiosError).message)
      : null;

  const isEmailTaken = serverError?.toLowerCase().includes('already exists');

  const handleTryAnother = () => {
    setEmail('');
    setPassword('');
    emailInputRef.current?.focus();
  };

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4 sm:p-6">
      <div className="w-full max-w-sm">
        <div className="text-center mb-6 sm:mb-8">
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900">{t('create_account')}</h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1">{t('appSubtitle', { ns: 'common' })}</p>
        </div>

        <Card>
          <CardContent className="p-4 sm:p-6">
          {isEmailTaken ? (
            <div className="space-y-4">
              <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
                <div className="flex items-start gap-3">
                  <div className="w-9 h-9 flex items-center justify-center rounded-full bg-amber-100 text-amber-600 flex-shrink-0">
                    <MailCheck className="w-5 h-5" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-amber-900">
                      {t('account_exists')}
                    </p>
                    <p className="text-sm text-amber-700 mt-0.5 truncate">
                      {email}
                    </p>
                  </div>
                </div>
              </div>

              <div className="flex flex-col gap-2">
                <Link
                  to={`/login?email=${encodeURIComponent(email)}`}
                  className="w-full"
                >
                  <Button className="w-full">
                    {t('log_in_instead')}
                    <ArrowRight className="w-4 h-4" />
                  </Button>
                </Link>
                <Button
                  type="button"
                  variant="outline"
                  className="w-full"
                  onClick={handleTryAnother}
                >
                  <Pencil className="w-4 h-4" />
                  {t('try_another_email')}
                </Button>
              </div>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label htmlFor="fullName" className="block text-sm font-medium text-slate-700 mb-1">
                  {t('full_name')}
                </label>
                <div className="relative">
                  <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 z-10" />
                  <Input
                    id="fullName"
                    type="text"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    required
                    autoComplete="name"
                    placeholder={t('full_name_placeholder')}
                    className="pl-10"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="email" className="block text-sm font-medium text-slate-700 mb-1">
                  {t('email')}
                </label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 z-10" />
                  <Input
                    ref={emailInputRef}
                    id="email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                    autoComplete="email"
                    placeholder={t('email_placeholder')}
                    className="pl-10"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="password" className="block text-sm font-medium text-slate-700 mb-1">
                  {t('password')}
                </label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 z-10" />
                  <Input
                    id="password"
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    minLength={6}
                    autoComplete="new-password"
                    placeholder={t('password_placeholder')}
                    className="pl-10"
                  />
                  <p className="text-xs text-slate-400 mt-1">{t('password_hint')}</p>
                </div>
              </div>

              {serverError && !isEmailTaken && (
                <p className="text-red-600 text-sm bg-red-50 border border-red-200 rounded-md px-3 py-2">
                  {serverError}
                </p>
              )}

              <Button
                type="submit"
                disabled={isPending}
                className="w-full"
              >
                {isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <UserPlus className="w-4 h-4" />}
                {isPending ? 'Creating account...' : 'Create Account'}
              </Button>
            </form>
          )}
          </CardContent>
        </Card>

        {!isEmailTaken && (
          <p className="mt-5 text-center text-sm text-slate-500">
            {t('has_account')}{' '}
            <Link to="/login" className="text-blue-600 hover:text-blue-800 font-medium">
              {t('sign_in_link')}
            </Link>
          </p>
        )}
      </div>
    </div>
  );
}
