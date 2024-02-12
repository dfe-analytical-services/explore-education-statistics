import { ButtonOptions } from '@common/hooks/useButton';
import Button from '@common/components/Button';
import React from 'react';

interface Props extends Omit<ButtonOptions, 'children'> {
  label: string;
}

export default function StartButton({ label, ...props }: Props) {
  return (
    <Button className="govuk-button--start" {...props}>
      {label}
      <svg
        className="govuk-button__start-icon"
        xmlns="http://www.w3.org/2000/svg"
        width="17.5"
        height="19"
        viewBox="0 0 33 40"
        aria-hidden="true"
        focusable="false"
      >
        <path fill="currentColor" d="M0 0h13l20 20-20 20H0l20-20z" />
      </svg>
    </Button>
  );
}
