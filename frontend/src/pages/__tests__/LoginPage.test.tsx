import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import i18n from '../../i18n';
import { LoginPage } from '../../pages/LoginPage';

// Mock useLogin
const mockMutate = vi.fn();
let mockState: { isPending: boolean; error: Error | null } = {
  isPending: false,
  error: null,
};

vi.mock('../../hooks/useAuth', () => ({
  useLogin: () => ({
    mutate: mockMutate,
    isPending: mockState.isPending,
    error: mockState.error,
  }),
}));

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

function renderLoginPage() {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <LoginPage />
        </I18nextProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  mockState = { isPending: false, error: null };
  vi.clearAllMocks();
});

describe('LoginPage', () => {
  it('renders email and password inputs', () => {
    renderLoginPage();

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('renders Sign In button', () => {
    renderLoginPage();

    const signInButton = screen.getByRole('button', { name: /sign in/i });
    expect(signInButton).toBeInTheDocument();
  });

  it('shows "Signing in..." when pending', () => {
    mockState = { isPending: true, error: null };
    renderLoginPage();

    expect(screen.getByText(/signing in\.\.\./i)).toBeInTheDocument();
  });

  it('shows register link', () => {
    renderLoginPage();

    const registerLink = screen.getByRole('link', { name: /register/i });
    expect(registerLink).toBeInTheDocument();
    expect(registerLink.getAttribute('href')).toBe('/register');
  });

  it('shows server error when error prop present', () => {
    const axiosError = new Error('Invalid credentials');
    (axiosError as unknown as { response?: { data?: { error?: string } } }).response = {
      data: { error: 'Invalid credentials' },
    };
    mockState = { isPending: false, error: axiosError };

    renderLoginPage();

    expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
  });

  it('shows server error from details array', () => {
    const axiosError = new Error('Bad request');
    (axiosError as unknown as { response?: { data?: { details?: string[] } } }).response = {
      data: { details: ['Field is required', 'Email is invalid'] },
    };
    mockState = { isPending: false, error: axiosError };

    renderLoginPage();

    expect(screen.getByText('Field is required, Email is invalid')).toBeInTheDocument();
  });
});
