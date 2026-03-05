import { useAuthStore } from '../store/authStore';

export const DashboardPage = () => {
  const { user } = useAuthStore();

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">Dashboard</h1>
      <p className="text-xl">Bienvenido, {user?.fullName}!</p>
      
      <div className="grid grid-cols-3 gap-4 mt-8">
        <div className="bg-blue-100 p-6 rounded">
          <h2 className="text-xl font-bold">Cursos</h2>
          <p className="text-3xl">0</p>
        </div>
        <div className="bg-green-100 p-6 rounded">
          <h2 className="text-xl font-bold">Badges</h2>
          <p className="text-3xl">0</p>
        </div>
        <div className="bg-purple-100 p-6 rounded">
          <h2 className="text-xl font-bold">Certificados</h2>
          <p className="text-3xl">0</p>
        </div>
      </div>
    </div>
  );
};
