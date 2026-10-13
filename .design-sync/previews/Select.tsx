import { Select } from '@elearning/ui';

export const Default = () => (
  <div className="w-72">
    <Select defaultValue="co">
      <option value="co">Colombia</option>
      <option value="mx">México</option>
      <option value="pe">Perú</option>
    </Select>
  </div>
);

export const Disabled = () => (
  <div className="w-72">
    <Select defaultValue="student" disabled>
      <option value="student">Estudiante</option>
      <option value="instructor">Instructor</option>
    </Select>
  </div>
);
