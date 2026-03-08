import { useQuery } from '@tanstack/react-query';
import { adminApi } from '../../api/admin';

export const UserManagement = () => {
  const { data: users, isLoading } = useQuery({
    queryKey: ['admin-users'],
    queryFn: () => adminApi.getUsers().then((r) => r.data.items),
  });

  if (isLoading) return <div>Cargando usuarios...</div>;

  return (
    <div>
      <h2 className="text-2xl font-bold mb-4">Gestión de Usuarios</h2>
      <table className="w-full border rounded">
        <thead className="bg-gray-100">
          <tr>
            <th className="p-2 text-left">Email</th>
            <th className="p-2 text-left">Nombre</th>
            <th className="p-2 text-left">Rol</th>
            <th className="p-2 text-left">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {users?.map((user: any) => (
            <tr key={user.id} className="border-t">
              <td className="p-2">{user.email}</td>
              <td className="p-2">{user.fullName}</td>
              <td className="p-2">{user.role}</td>
              <td className="p-2">
                <button className="bg-blue-500 text-white px-2 py-1 rounded text-sm">
                  Editar
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
