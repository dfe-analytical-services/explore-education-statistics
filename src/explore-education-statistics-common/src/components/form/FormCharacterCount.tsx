import classNames from 'classnames';
import React from 'react';

interface Props {
  id: string;
  maxLength: number;
  value?: string;
}
export default function FormCharacterCount({ id, maxLength, value }: Props) {
  const remaining = maxLength - (value?.trim().length ?? 0);

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
