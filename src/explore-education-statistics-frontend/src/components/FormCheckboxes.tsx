import React from 'react';
import FormCheckbox from './FormCheckbox';

interface Checkbox {
  id: string;
  label: string;
  value: string | string[] | number;
}

interface Props {
  name: string;
  items: Checkbox[];
}

const FormCheckboxes = ({ name, items }: Props) => {
  return (
    <div className="govuk-checkboxes">
      {items.map(item => (
        <FormCheckbox name={name} key={item.id} {...item} />
      ))}
    </div>
  );
};

export default FormCheckboxes;
