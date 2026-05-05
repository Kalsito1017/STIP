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
  it('renders current language label EN', () => {
    useAppStore.setState({ language: 'en' });
    render(<LanguageSwitcher />);
    expect(screen.getByText('EN')).toBeInTheDocument();
  });

  it('renders current language label БГ', () => {
    useAppStore.setState({ language: 'bg' });
    render(<LanguageSwitcher />);
    expect(screen.getByText('БГ')).toBeInTheDocument();
  });

  it('shows next language label', () => {
    useAppStore.setState({ language: 'en' });
    render(<LanguageSwitcher />);
    expect(screen.getByText('→ БГ')).toBeInTheDocument();
  });

  it('shows next language label when current is bg', () => {
    useAppStore.setState({ language: 'bg' });
    render(<LanguageSwitcher />);
    expect(screen.getByText('→ EN')).toBeInTheDocument();
  });

  it('clicking triggers setLanguage to next locale', async () => {
    useAppStore.setState({ language: 'en' });
    const user = userEvent.setup();

    render(<LanguageSwitcher />);
    await user.click(screen.getByRole('button'));

    expect(useAppStore.getState().language).toBe('bg');
  });

  it('clicking toggles from bg to en', async () => {
    useAppStore.setState({ language: 'bg' });
    const user = userEvent.setup();

    render(<LanguageSwitcher />);
    await user.click(screen.getByRole('button'));

    expect(useAppStore.getState().language).toBe('en');
  });
});
