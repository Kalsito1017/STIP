import { renderWithProviders } from '@/test-utils';
import { LoginPage } from './LoginPage';
import * as useAuthModule from '../hooks/useAuth';

vi.mock('../hooks/useAuth', () => ({
  useLogin: vi.fn(),
  useRegister: vi.fn(),
  useLogout: vi.fn(),
  useDeleteAccount: vi.fn(),
}));

describe('LoginPage', () => {
  it('shows spinner icon when form is pending', () => {
    vi.mocked(useAuthModule.useLogin).mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
      error: null,
    } as unknown as ReturnType<typeof useAuthModule.useLogin>);

    const { container } = renderWithProviders(<LoginPage />);
    const spinner = container.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('shows LogIn icon when form is not pending', () => {
    vi.mocked(useAuthModule.useLogin).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useAuthModule.useLogin>);

    const { container } = renderWithProviders(<LoginPage />);
    const spinner = container.querySelector('.animate-spin');
    expect(spinner).toBeNull();
  });

  it('disables button when pending', () => {
    vi.mocked(useAuthModule.useLogin).mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
      error: null,
    } as unknown as ReturnType<typeof useAuthModule.useLogin>);

    const { getByRole } = renderWithProviders(<LoginPage />);
    expect(getByRole('button', { name: /signing in/i })).toBeDisabled();
  });

  it('shows error message when login fails', () => {
    vi.mocked(useAuthModule.useLogin).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      error: new Error('Invalid credentials'),
    } as unknown as ReturnType<typeof useAuthModule.useLogin>);

    const { getByText } = renderWithProviders(<LoginPage />);
    expect(getByText('Invalid credentials')).toBeInTheDocument();
  });
});
