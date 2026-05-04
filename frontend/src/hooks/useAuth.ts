import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { useAppStore } from '../store/useAppStore';
import { authApi, type RegisterRequest, type LoginRequest } from '../services/api';

export function useRegister() {
  const setAuth = useAppStore((s) => s.setAuth);
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    onSuccess: (response) => {
      setAuth(response.token, {
        userId: response.userId,
        email: response.email,
        fullName: response.fullName,
      });
      toast.success('Account created successfully');
      navigate('/dashboard');
    },
  });
}

export function useLogin() {
  const setAuth = useAppStore((s) => s.setAuth);
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      setAuth(response.token, {
        userId: response.userId,
        email: response.email,
        fullName: response.fullName,
      });
      toast.success('Signed in successfully');
      navigate('/dashboard');
    },
  });
}

export function useLogout() {
  const clearAuth = useAppStore((s) => s.clearAuth);
  const navigate = useNavigate();

  return () => {
    clearAuth();
    navigate('/login');
  };
}
