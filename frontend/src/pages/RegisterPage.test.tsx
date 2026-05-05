import { renderWithProviders } from '@/test-utils';
import { RegisterPage } from './RegisterPage';
import * as useAuthModule from '../hooks/useAuth';

vi.mock('../hooks/useAuth', () => ({
  useLogin: vi.fn(),
  useRegister: vi.fn(),
  useLogout: vi.fn(),
  useDeleteAccount: vi.fn(),
}));

describe('RegisterPage', () => {
  it('shows spinner icon when form is pending', () => {
    vi.mocked(useAuthModule.useRegister).mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
      error: null,
    } as unknown as ReturnType<typeof useAuthModule.useRegister>);

    const { container } = renderWithProviders(<RegisterPage />);
    const spinner = container.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('shows UserPlus icon when form is not pending', () => {
    vi.mocked(useAuthModule.useRegister).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useAuthModule.useRegister>);

    const { container } = renderWithProviders(<RegisterPage />);
    const spinner = container.querySelector('.animate-spin');
    expect(spinner).toBeNull();
  });

  it('disables button when pending', () => {
    vi.mocked(useAuthModule.useRegister).mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
      error: null,
    } as unknown as ReturnType<typeof useAuthModule.useRegister>);

    const { getByRole } = renderWithProviders(<RegisterPage />);
    expect(getByRole('button', { name: /creating account/i })).toBeDisabled();
  });
});
