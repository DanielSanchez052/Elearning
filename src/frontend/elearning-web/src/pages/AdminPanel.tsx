export const AdminPanelPage = () => {
  return (
    <div className="p-8">
      <h1 className="text-4xl font-bold mb-4">Panel Administrativo</h1>

      <div className="grid grid-cols-3 gap-4 mb-8">
        <div className="bg-blue-100 p-6 rounded">
          <h2 className="font-bold">Usuarios</h2>
          <p className="text-3xl font-bold">0</p>
        </div>
        <div className="bg-green-100 p-6 rounded">
          <h2 className="font-bold">Cursos</h2>
          <p className="text-3xl font-bold">0</p>
        </div>
        <div className="bg-purple-100 p-6 rounded">
          <h2 className="font-bold">Enrollments</h2>
          <p className="text-3xl font-bold">0</p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="bg-white rounded shadow p-6">
          <h2 className="text-xl font-bold mb-4">Gestión de Usuarios</h2>
          <button className="bg-blue-500 text-white px-4 py-2 rounded">
            Ver Usuarios
          </button>
        </div>
        <div className="bg-white rounded shadow p-6">
          <h2 className="text-xl font-bold mb-4">Gestión de Cursos</h2>
          <button className="bg-blue-500 text-white px-4 py-2 rounded">
            Ver Cursos
          </button>
        </div>
      </div>
    </div>
  );
};
