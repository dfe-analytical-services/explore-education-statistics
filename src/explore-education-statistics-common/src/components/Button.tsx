import classNames from 'classnames';
import React, { MouseEventHandler, ReactNode } from 'react';
import styles from './Button.module.scss';

export interface ButtonProps {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  id?: string;
  onClick?: MouseEventHandler;
  variant?: 'secondary' | 'warning';
  type?: 'button' | 'submit' | 'reset';
}

const Button = ({
  children,
  className,
  id,
  onClick,
  disabled = false,
  variant,
  type = 'button',
}: ButtonProps) => {
  return (
    // eslint-disable-next-line react/button-has-type
    <button
      aria-disabled={disabled}
      className={classNames(
        'govuk-button',
        styles.marginRight,
        {
          'govuk-button--disabled': disabled,
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
      disabled={disabled}
      id={id}
      onClick={onClick}
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;
