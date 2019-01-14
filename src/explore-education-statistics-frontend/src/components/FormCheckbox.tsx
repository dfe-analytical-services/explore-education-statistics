import React from 'react';

interface Props {
  id: string;
  label: string;
  name: string;
  value: string | string[] | number;
}

const FormCheckbox = ({ id, label, name, value }: Props) => {
  return (
    <div className="govuk-checkboxes__item">
      <input
        className="govuk-checkboxes__input"
        id={id}
        name={name}
        type="checkbox"
        value={value}
      />
      <label className="govuk-label govuk-checkboxes__label" htmlFor={id}>
        {label}
      </label>
    </div>
  );
};

export default FormCheckbox;
