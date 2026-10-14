import { AppRoot, Card, CardHeader, CardTitle, CardDescription, Button } from '@elearning/ui';

export const Page = () => (
  <AppRoot className="w-96 rounded-xl p-6">
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Hola, Ana</h1>
        <p className="mt-1 text-sm text-muted-foreground">Continúa donde lo dejaste.</p>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Excel Avanzado</CardTitle>
          <CardDescription>Lección 3 de 4 · Funciones BUSCARX</CardDescription>
        </CardHeader>
        <Button>Continuar</Button>
      </Card>
    </div>
  </AppRoot>
);
