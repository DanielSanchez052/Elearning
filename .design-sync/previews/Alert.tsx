import { Alert } from '@elearning/ui';

export const Tones = () => (
  <div className="flex w-96 flex-col gap-3">
    <Alert tone="success">Tu contraseña se actualizó. Ya puedes iniciar sesión.</Alert>
    <Alert tone="danger">Email o contraseña incorrectos.</Alert>
    <Alert tone="warning">Tu email aún no está verificado.</Alert>
    <Alert>Te enviaremos un enlace para restablecer tu contraseña.</Alert>
  </div>
);

export const WithTitle = () => (
  <div className="w-96">
    <Alert tone="success" title="Cuenta creada">
      Revisa tu correo ana.gomez@empresa.com para verificar tu email.
    </Alert>
  </div>
);
