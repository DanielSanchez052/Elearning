import { Outlet } from 'react-router-dom';
import { AppRoot } from '@/components/ui';
import AppHeader from './AppHeader';

/** Shell del área del estudiante: barra superior fija y la página debajo. */
export default function StudentLayout() {
  return (
    <AppRoot className="flex min-h-screen flex-col">
      <AppHeader />
      <div className="flex flex-1 flex-col">
        <Outlet />
      </div>
    </AppRoot>
  );
}
