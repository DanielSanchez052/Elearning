import { Skeleton } from '@elearning/ui';

export const PageHeader = () => (
  <div className="flex w-96 flex-col gap-2" aria-busy="true">
    <Skeleton className="h-4 w-48" />
    <Skeleton className="h-8 w-64" />
  </div>
);

export const CourseCard = () => (
  <div className="w-72 overflow-hidden rounded-2xl border border-border" aria-busy="true">
    <Skeleton className="aspect-video w-full rounded-none" />
    <div className="flex flex-col gap-3 p-5">
      <Skeleton className="h-5 w-48" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-40" />
    </div>
  </div>
);

export const TableRows = () => (
  <div className="flex w-full flex-col gap-3" aria-busy="true">
    <Skeleton className="h-12 rounded-xl" />
    <Skeleton className="h-12 rounded-xl" />
    <Skeleton className="h-12 rounded-xl" />
  </div>
);
