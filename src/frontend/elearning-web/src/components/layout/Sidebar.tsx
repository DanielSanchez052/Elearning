import { Link } from 'react-router-dom';

export const Sidebar = () => {
  return (
    <aside className="w-64 bg-gray-100 p-4 h-screen">
      <nav className="space-y-4">
        <Link to="/dashboard" className="block p-2 hover:bg-gray-200 rounded">
          Dashboard
        </Link>
        <Link to="/courses" className="block p-2 hover:bg-gray-200 rounded">
          Cursos
        </Link>
        <Link to="/profile" className="block p-2 hover:bg-gray-200 rounded">
          Perfil
        </Link>
        <Link to="/notifications" className="block p-2 hover:bg-gray-200 rounded">
          Notificaciones
        </Link>
      </nav>
    </aside>
  );
};
