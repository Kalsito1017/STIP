import { useState } from 'react';
import { AlertTriangle, Loader2 } from 'lucide-react';
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

export function SettingsPage() {
  const { t } = useTranslation('settings');
  const user = useAppStore((s) => s.user);
  const deleteAccount = useDeleteAccount();
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  return (
    <div className="space-y-6 max-w-2xl">
      <h1 className="text-2xl font-bold text-slate-900">{t('title')}</h1>

      <Card>
        <CardHeader>
          <CardTitle>{t('account')}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div>
              <p className="text-sm font-medium text-slate-500">{t('name')}</p>
              <p className="text-sm text-slate-900">{user?.fullName}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-slate-500">{t('email')}</p>
              <p className="text-sm text-slate-900">{user?.email}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="border-red-200">
        <CardHeader className="border-b border-red-200">
          <CardTitle className="text-red-600 flex items-center gap-2">
            <AlertTriangle className="w-5 h-5" />
            {t('danger_zone')}
          </CardTitle>
        </CardHeader>
        <CardContent className="pt-6">
          <p className="text-sm text-slate-600 mb-4">
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
