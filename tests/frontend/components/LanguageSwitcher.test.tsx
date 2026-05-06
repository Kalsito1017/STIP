import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useAppStore } from '@/store/useAppStore';
import { LanguageSwitcher } from '@/components/LanguageSwitcher';

const initialState = useAppStore.getInitialState();

beforeEach(() => {
  useAppStore.setState(initialState);
  localStorage.clear();
});

describe('LanguageSwitcher', () => {
  it('renders both language labels', () => {
    render(<LanguageSwitcher />);
    expect(screen.getByText('EN')).toBeInTheDocument();
    expect(screen.getByText('БГ')).toBeInTheDocument();
  });

  it('marks English as checked when language is en', () => {
    useAppStore.setState({ language: 'en' });
    render(<LanguageSwitcher />);
    const enRadio = screen.getByRole('radio', { name: /switch to english/i });
    expect(enRadio).toHaveAttribute('aria-checked', 'true');
  });

  it('marks Bulgarian as checked when language is bg', () => {
    useAppStore.setState({ language: 'bg' });
    render(<LanguageSwitcher />);
    const bgRadio = screen.getByRole('radio', { name: /switch to bulgarian/i });
    expect(bgRadio).toHaveAttribute('aria-checked', 'true');
  });

  it('clicking Bulgarian switches language to bg', async () => {
    useAppStore.setState({ language: 'en' });
    const user = userEvent.setup();
    render(<LanguageSwitcher />);

    await user.click(screen.getByRole('radio', { name: /switch to bulgarian/i }));
    expect(useAppStore.getState().language).toBe('bg');
  });

  it('clicking English switches language to en', async () => {
    useAppStore.setState({ language: 'bg' });
    const user = userEvent.setup();
    render(<LanguageSwitcher />);

    await user.click(screen.getByRole('radio', { name: /switch to english/i }));
    expect(useAppStore.getState().language).toBe('en');
  });

  it('compact mode renders a single toggle button', () => {
    render(<LanguageSwitcher compact />);
    expect(screen.getByRole('button')).toBeInTheDocument();
  });
});
