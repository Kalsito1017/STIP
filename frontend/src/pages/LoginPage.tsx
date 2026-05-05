import { useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { LogIn, Loader2, Mail, Lock } from 'lucide-react';
import { useLogin } from '../hooks/useAuth';
import { Input } from '../components/ui/input';
import { Button } from '../components/ui/button';
import { useTranslation } from 'react-i18next';
import type { AxiosError } from 'axios';

export function LoginPage() {
  const { t } = useTranslation('auth');
  const [searchParams] = useSearchParams();
  const prefilledEmail = searchParams.get('email') || '';
  const [email, setEmail] = useState(prefilledEmail);
  const [password, setPassword] = useState('');
  const { mutate, isPending, error } = useLogin();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    mutate({ email, password });
  };

  const serverError =
    error instanceof Error
      ? ((error as AxiosError<{ error?: string; details?: string[] }>).response?.data?.details?.join?.(', ')
        ?? (error as AxiosError<{ error?: string }>).response?.data?.error
        ?? (error as AxiosError).message)
      : null;

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4 sm:p-6">
      <div className="w-full max-w-sm">
        <div className="text-center mb-6 sm:mb-8">
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900">{t('sign_in')}</h1>
          <p className="text-xs sm:text-sm text-slate-500 mt-1">{t('appSubtitle', { ns: 'common' })}</p>
        </div>

        <div className="bg-white rounded-lg shadow-sm border border-slate-200 p-4 sm:p-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-slate-700 mb-1">
                {t('email')}
              </label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 z-10" />
                <Input
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
                  autoComplete="current-password"
                  placeholder={t('password_placeholder')}
                  className="pl-10"
                />
              </div>
            </div>

            {serverError && (
              <p className="text-red-600 text-sm bg-red-50 border border-red-200 rounded-md px-3 py-2">
                {serverError}
              </p>
            )}

            <Button
              type="submit"
              disabled={isPending}
              className="w-full"
            >
              {isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <LogIn className="w-4 h-4" />}
              {isPending ? 'Signing in...' : 'Sign In'}
            </Button>
          </form>

          <p className="mt-5 text-center text-sm text-slate-500">
            {t('no_account')}{' '}
            <Link to="/register" className="text-blue-600 hover:text-blue-800 font-medium">
              {t('register_link')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
