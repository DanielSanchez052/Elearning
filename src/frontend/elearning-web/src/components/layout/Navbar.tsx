import { useAuthStore } from '../../store/authStore';

export const Navbar = () => {
  const { user, clearAuth } = useAuthStore();

  return (
    <nav className="flex justify-between items-center p-4 bg-white shadow">
      <div className="text-2xl font-bold">ELearning</div>
      <div className="flex items-center gap-4">
        {user ? (
          <>
            <span>{user.fullName}</span>
            <button
              onClick={clearAuth}
              className="px-4 py-2 bg-red-500 text-white rounded"
            >
              Logout
            </button>
          </>
        ) : null}
      </div>
    </nav>
  );
};
