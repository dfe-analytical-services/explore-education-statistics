import React, { ChangeEventHandler, FunctionComponent } from 'react';

interface Props {
  checked?: boolean;
  id: string;
  hint?: string;
  label: string;
  name: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  value: string | string[] | number;
}

const FormCheckbox: FunctionComponent<Props> = ({
  checked,
  id,
  hint,
  label,
  name,
  onChange,
  value,
}) => {
  return (
    <div className="govuk-checkboxes__item">
      <input
        aria-describedby={hint ? `${id}-item-hint` : undefined}
        className="govuk-checkboxes__input"
        checked={checked}
        id={id}
        name={name}
        onChange={onChange}
        type="checkbox"
        value={value}
      />
      <label className="govuk-label govuk-checkboxes__label" htmlFor={id}>
        {label}
      </label>
      {hint && (
        <span
          id={`${id}-item-hint`}
          className="govuk-hint govuk-checkboxes__hint"
        >
          {hint}
        </span>
      )}
    </div>
  );
};

export default FormCheckbox;
