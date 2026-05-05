import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { I18nextProvider } from 'react-i18next';
import i18n from '../../i18n';
import { ErrorAlert } from '../ErrorAlert';

function renderWithI18n(ui: React.ReactElement) {
  return render(
    <I18nextProvider i18n={i18n}>
      {ui}
    </I18nextProvider>,
  );
}

describe('ErrorAlert', () => {
  it('renders error message', () => {
    renderWithI18n(<ErrorAlert message="Failed to load data" />);
    expect(screen.getByText('Failed to load data')).toBeInTheDocument();
  });

  it('shows retry button when onRetry provided', () => {
    renderWithI18n(<ErrorAlert message="Error" onRetry={() => {}} />);
    // ErrorAlert uses useTranslation() without namespace, so t('errors.try_again')
    // returns the raw key. Find the button containing the refresh icon.
    const button = screen.getByText('errors.try_again').closest('button');
    expect(button).toBeInTheDocument();
  });

  it('does not show retry button when onRetry not provided', () => {
    renderWithI18n(<ErrorAlert message="Error" />);
    expect(screen.queryByText('errors.try_again')).not.toBeInTheDocument();
  });

  it('calls onRetry when retry button clicked', async () => {
    const onRetry = vi.fn();
    const user = userEvent.setup();

    renderWithI18n(<ErrorAlert message="Error" onRetry={onRetry} />);
    const button = screen.getByText('errors.try_again').closest('button')!;
    await user.click(button);

    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it('falls back to translated message when message is empty', () => {
    renderWithI18n(<ErrorAlert message="" />);
    // ErrorAlert uses useTranslation() (defaults to 'common' NS),
    // so t('errors.something_wrong') returns the raw key
    expect(screen.getByText('errors.something_wrong')).toBeInTheDocument();
  });

  it('falls back to translated message when message is "Error"', () => {
    renderWithI18n(<ErrorAlert message="Error" />);
    expect(screen.getByText('errors.something_wrong')).toBeInTheDocument();
  });
});
