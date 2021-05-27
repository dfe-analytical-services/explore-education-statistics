import classNames from 'classnames';
import React, { MouseEventHandler, ReactNode } from 'react';

export interface ButtonProps {
  children: ReactNode;
  className?: string;
  testId?: string;
  disabled?: boolean;
  id?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  variant?: 'secondary' | 'warning';
  type?: 'button' | 'submit' | 'reset';
}

const Button = ({
  children,
  className,
  testId,
  id,
  onClick,
  disabled = false,
  variant,
  type = 'button',
}: ButtonProps) => {
  return (
    <button
      aria-disabled={disabled}
      className={classNames(
        'govuk-button',
        {
          'govuk-button--disabled': disabled,
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
      data-testid={testId}
      disabled={disabled}
      id={id}
      onClick={onClick}
      // eslint-disable-next-line react/button-has-type
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;
