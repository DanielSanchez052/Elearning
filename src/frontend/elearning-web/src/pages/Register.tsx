import { useAuth } from '../hooks/useAuth';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { registerSchema, type RegisterInput } from '../lib/schemas';

export const RegisterPage = () => {
  const navigate = useNavigate();
  const { register: registerUser } = useAuth();
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterInput>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = (data: RegisterInput) => {
    registerUser({
      email: data.email,
      password: data.password,
      fullName: data.fullName
    } as any, {
      onSuccess: () => navigate('/dashboard'),
    } as any);
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <form onSubmit={handleSubmit(onSubmit)} className="bg-white p-8 rounded shadow w-96">
        <h1 className="text-2xl font-bold mb-4">Registrarse</h1>
        <div className="mb-4">
          <input
            {...register('fullName')}
            type="text"
            placeholder="Nombre completo"
            className="w-full border p-2 rounded"
          />
          {errors.fullName && <p className="text-red-500 text-sm">{errors.fullName.message}</p>}
        </div>
        <div className="mb-4">
          <input
            {...register('email')}
            type="email"
            placeholder="Email"
            className="w-full border p-2 rounded"
          />
          {errors.email && <p className="text-red-500 text-sm">{errors.email.message}</p>}
        </div>
        <div className="mb-4">
          <input
            {...register('password')}
            type="password"
            placeholder="Contraseña"
            className="w-full border p-2 rounded"
          />
          {errors.password && <p className="text-red-500 text-sm">{errors.password.message}</p>}
        </div>
        <div className="mb-4">
          <input
            {...register('confirmPassword')}
            type="password"
            placeholder="Confirmar contraseña"
            className="w-full border p-2 rounded"
          />
          {errors.confirmPassword && <p className="text-red-500 text-sm">{errors.confirmPassword.message}</p>}
        </div>
        <button
          type="submit"
          className="w-full bg-blue-500 text-white p-2 rounded"
        >
          Registrarse
        </button>
      </form>
    </div>
  );
};
