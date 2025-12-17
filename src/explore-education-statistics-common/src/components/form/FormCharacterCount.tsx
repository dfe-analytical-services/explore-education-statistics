import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import classNames from 'classnames';
import React from 'react';

interface Props {
  id?: string;
  maxLength: number;
  name: string;
  value?: string;
}
export default function FormCharacterCount({
  id: customId,
  maxLength,
  name,
  value,
}: Props) {
  const remaining = maxLength - (value?.trim().length ?? 0);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, customId);

  return (
    <div
      aria-live="polite"
      className={classNames('govuk-character-count__message', {
        'govuk-hint': remaining >= 0,
        'govuk-error-message': remaining < 0,
      })}
      id={`${id}-info`}
    >
      {remaining >= 0
        ? `You have ${remaining} character${
            remaining !== 1 ? 's' : ''
          } remaining`
        : `You have ${Math.abs(remaining)} character${
            remaining !== -1 ? 's' : ''
          } too many`}
    </div>
  );
}
