import { Badge, RadioCard } from '@elearning/ui';

const noop = () => {};

export const CourseScope = () => (
  <div role="radiogroup" aria-label="Alcance del curso" className="grid w-full gap-3 sm:grid-cols-2">
    <RadioCard name="scope" value="global" checked onChange={noop} label="Global" description="Visible para estudiantes de todos los países." />
    <RadioCard name="scope" value="pais" checked={false} onChange={noop} label="Por país" description="Solo en los países que elijas." />
  </div>
);

export const QuizOptions = () => (
  <div role="radiogroup" aria-label="¿Qué función busca un valor en una tabla?" className="flex w-96 flex-col gap-2">
    <RadioCard name="q1" value="a" checked={false} onChange={noop} label="SUMAR.SI" />
    <RadioCard name="q1" value="b" checked onChange={noop} label="BUSCARX" />
    <RadioCard name="q1" value="c" checked={false} onChange={noop} label="CONTAR.SI" />
    <RadioCard name="q1" value="d" disabled label="CONCATENAR" />
  </div>
);

export const ResultReview = () => (
  <div className="flex w-96 flex-col gap-2">
    <RadioCard name="r1" value="b" tone="success" checked onChange={noop} label="BUSCARX" trailing={<Badge tone="success">Correcta</Badge>} />
    <RadioCard name="r1" value="c" tone="danger" checked={false} onChange={noop} label="CONSULTAV" trailing={<Badge tone="danger">Tu respuesta</Badge>} />
  </div>
);
