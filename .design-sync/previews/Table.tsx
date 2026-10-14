import { Avatar, Badge, Button, Card, Table, TableBody, TableCell, TableHead, TableHeader, TableRow, PencilIcon } from '@elearning/ui';

const users = [
  { name: 'Ana Gómez', email: 'ana.gomez@empresa.com', role: 'Estudiante', tone: 'neutral', verified: true },
  { name: 'Carlos Rivas', email: 'carlos.rivas@empresa.com', role: 'Instructor', tone: 'info', verified: true },
  { name: 'Luisa Pérez', email: 'luisa.perez@empresa.com', role: 'Estudiante', tone: 'neutral', verified: false },
] as const;

export const Users = () => (
  <Card padding="none" className="w-full overflow-hidden">
    <Table>
      <TableHead>
        <TableRow>
          <TableHeader>Usuario</TableHeader>
          <TableHeader>Rol</TableHeader>
          <TableHeader>Estado</TableHeader>
          <TableHeader className="text-right">Acciones</TableHeader>
        </TableRow>
      </TableHead>
      <TableBody>
        {users.map((u) => (
          <TableRow key={u.email}>
            <TableCell>
              <span className="flex items-center gap-3">
                <Avatar name={u.name} tone="neutral" />
                <span>
                  <span className="block font-medium text-foreground">{u.name}</span>
                  <span className="block text-xs text-subtle-foreground">{u.email}</span>
                </span>
              </span>
            </TableCell>
            <TableCell><Badge tone={u.tone}>{u.role}</Badge></TableCell>
            <TableCell>
              <Badge tone={u.verified ? 'success' : 'warning'}>{u.verified ? 'Verificado' : 'Sin verificar'}</Badge>
            </TableCell>
            <TableCell className="text-right">
              <Button variant="ghost" size="icon" aria-label={`Editar ${u.name}`}><PencilIcon /></Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </Card>
);

export const Attempts = () => (
  <Card padding="none" className="w-full overflow-hidden">
    <Table>
      <TableHead>
        <TableRow>
          <TableHeader>Intento</TableHeader>
          <TableHeader>Fecha</TableHeader>
          <TableHeader>Puntaje</TableHeader>
          <TableHeader>Resultado</TableHeader>
        </TableRow>
      </TableHead>
      <TableBody>
        <TableRow>
          <TableCell>2</TableCell>
          <TableCell>8 oct 2026</TableCell>
          <TableCell className="tabular-nums">90%</TableCell>
          <TableCell><Badge tone="success">Aprobado</Badge></TableCell>
        </TableRow>
        <TableRow>
          <TableCell>1</TableCell>
          <TableCell>6 oct 2026</TableCell>
          <TableCell className="tabular-nums">60%</TableCell>
          <TableCell><Badge tone="danger">No aprobado</Badge></TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </Card>
);
