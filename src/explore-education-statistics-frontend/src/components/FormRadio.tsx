import React, { ChangeEvent, FunctionComponent } from 'react';

interface Props {
  checked?: boolean;
  hint?: string;
  id: string;
  label: string;
  name: string;
  onChange?: (event: ChangeEvent) => void;
  value: string;
}

const FormRadio: FunctionComponent<Props> = ({
  checked,
  hint,
  id,
  label,
  name,
  value,
}) => {
  return (
    <div className="govuk-radios__item">
      <input
        aria-describedby={hint ? `${id}-item-hint` : undefined}
        className="govuk-radios__input"
        checked={checked}
        id={id}
        name={name}
        type="radio"
        value={value}
      />
      <label className="govuk-label govuk-radios__label" htmlFor={id}>
        {label}
      </label>
      {hint && (
        <span id={`${id}-item-hint`} className="govuk-hint govuk-radios__hint">
          {hint}
        </span>
      )}
    </div>
  );
};

export default FormRadio;
