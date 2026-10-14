import { Card, Pagination } from '@elearning/ui';

const noop = () => {};

export const Middle = () => (
  <div className="w-full">
    <Pagination page={5} totalPages={12} onPageChange={noop} summary="Mostrando 33–40 de 92" />
  </div>
);

export const FirstPage = () => (
  <div className="w-full">
    <Pagination page={1} totalPages={3} onPageChange={noop} />
  </div>
);

export const TableFooter = () => (
  <Card padding="none" className="w-full overflow-hidden">
    <div className="px-6 py-6 text-sm text-muted-foreground">8 cursos en esta página</div>
    <Pagination className="border-t border-border px-6 py-3" page={2} totalPages={8} onPageChange={noop} summary="Página 2 de 8" />
  </Card>
);
