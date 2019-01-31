import classNames from 'classnames';
import React, { MouseEventHandler, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  onClick?: MouseEventHandler;
  type?: 'button' | 'submit' | 'reset';
}

const Button = ({
  children,
  className,
  onClick,
  disabled = false,
  type = 'button',
}: Props) => {
  const classes = classNames(
    'govuk-button',
    {
      'govuk-button--disabled': disabled,
    },
    className,
  );

  return (
    <button
      aria-disabled={disabled}
      className={classes}
      disabled={disabled}
      onClick={onClick}
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;
