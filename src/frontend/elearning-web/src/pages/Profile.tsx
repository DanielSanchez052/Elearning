import { useAuthStore } from '../store/authStore';

export const ProfilePage = () => {
  const { user } = useAuthStore();

  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">Mi Perfil</h1>

      <div className="max-w-2xl bg-white rounded shadow p-6 mb-8">
        <h2 className="text-2xl font-bold mb-4">Información Personal</h2>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-gray-600">Nombre</p>
            <p className="font-bold">{user?.fullName}</p>
          </div>
          <div>
            <p className="text-gray-600">Email</p>
            <p className="font-bold">{user?.email}</p>
          </div>
          <div>
            <p className="text-gray-600">Rol</p>
            <p className="font-bold">{user?.role}</p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="bg-white rounded shadow p-6">
          <h2 className="text-xl font-bold mb-4">Badges</h2>
          <p className="text-gray-600">No hay badges aún</p>
        </div>
        <div className="bg-white rounded shadow p-6">
          <h2 className="text-xl font-bold mb-4">Certificados</h2>
          <p className="text-gray-600">No hay certificados aún</p>
        </div>
      </div>
    </div>
  );
};
