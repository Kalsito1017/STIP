import { useState } from 'react';
import { AlertTriangle, Loader2, Sun, Moon, Globe } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { useDeleteAccount } from '../hooks/useAuth';
import { Button } from '../components/ui/button';
import { Card, CardHeader, CardTitle, CardContent } from '../components/ui/card';
import {
  Dialog,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogClose,
} from '../components/ui/dialog';
import { useTranslation } from 'react-i18next';
import { type Locale, SUPPORTED_LOCALES } from '../i18n';

const localeLabels: Record<Locale, string> = {
  en: 'English',
  bg: 'Български',
};

export function SettingsPage() {
  const { t } = useTranslation('settings');
  const user = useAppStore((s) => s.user);
  const darkMode = useAppStore((s) => s.darkMode);
  const toggleDarkMode = useAppStore((s) => s.toggleDarkMode);
  const language = useAppStore((s) => s.language);
  const setLanguage = useAppStore((s) => s.setLanguage);
  const deleteAccount = useDeleteAccount();
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  return (
    <div className="space-y-6 max-w-2xl">
      <h1 className="text-2xl font-bold text-foreground">{t('title')}</h1>

      <Card>
        <CardHeader>
          <CardTitle>{t('account')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div>
              <p className="text-sm font-medium text-muted-foreground">{t('name')}</p>
              <p className="text-sm text-foreground">{user?.fullName}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">{t('email')}</p>
              <p className="text-sm text-foreground">{user?.email}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Globe className="w-4 h-4" />
            Language
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2">
            {SUPPORTED_LOCALES.map((loc) => (
              <Button
                key={loc}
                variant={language === loc ? 'default' : 'outline'}
                size="sm"
                onClick={() => setLanguage(loc)}
              >
                {localeLabels[loc]}
              </Button>
            ))}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {darkMode ? <Moon className="w-4 h-4" /> : <Sun className="w-4 h-4" />}
            Appearance
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-foreground font-medium">Dark mode</p>
              <p className="text-xs text-muted-foreground">
                {darkMode ? 'Dark theme is active' : 'Light theme is active'}
              </p>
            </div>
            <Button variant="outline" size="sm" onClick={toggleDarkMode}>
              {darkMode ? 'Switch to light' : 'Switch to dark'}
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card className="border-destructive/30">
        <CardHeader className="border-b border-destructive/20">
          <CardTitle className="text-destructive flex items-center gap-2">
            <AlertTriangle className="w-5 h-5" />
            {t('danger_zone')}
          </CardTitle>
        </CardHeader>
        <CardContent className="pt-6">
          <p className="text-sm text-muted-foreground mb-4">
            {t('danger_description')}
          </p>
          <Button
            variant="destructive"
            onClick={() => setDeleteDialogOpen(true)}
            disabled={deleteAccount.isPending}
          >
            {deleteAccount.isPending && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
            {t('delete_account')}
          </Button>
        </CardContent>
      </Card>

      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogHeader>
          <DialogTitle>{t('delete_dialog_title')}</DialogTitle>
          <DialogDescription>
            {t('delete_dialog_description')}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose onClick={() => setDeleteDialogOpen(false)} />
          <Button variant="outline" onClick={() => setDeleteDialogOpen(false)}>
            {t('cancel', { ns: 'common' })}
          </Button>
          <Button
            variant="destructive"
            onClick={() => {
              deleteAccount.mutate();
              setDeleteDialogOpen(false);
            }}
            disabled={deleteAccount.isPending}
          >
            {deleteAccount.isPending && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
            {t('yes_delete')}
          </Button>
        </DialogFooter>
      </Dialog>
    </div>
  );
}
