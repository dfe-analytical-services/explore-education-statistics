import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, { MouseEventHandler, ReactNode, useCallback } from 'react';

export interface ButtonProps {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  disableDoubleClick?: boolean;
  id?: string;
  testId?: string;
  type?: 'button' | 'submit' | 'reset';
  variant?: 'secondary' | 'warning';
  onClick?: MouseEventHandler<HTMLButtonElement>;
}

const Button = ({
  children,
  className,
  disabled = false,
  disableDoubleClick = true,
  id,
  testId,
  type = 'button',
  variant,
  onClick,
}: ButtonProps) => {
  const [isClicking, toggleClicking] = useToggle(false);
  const isMountedRef = useMountedRef();

  const handleClick: MouseEventHandler<HTMLButtonElement> = useCallback(
    async event => {
      if (disableDoubleClick) {
        toggleClicking.on();
      }

      await onClick?.(event);

      if (disableDoubleClick && isMountedRef.current) {
        toggleClicking.off();
      }
    },
    [disableDoubleClick, isMountedRef, onClick, toggleClicking],
  );

  return (
    <button
      aria-disabled={disabled || isClicking}
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
      disabled={disabled || isClicking}
      id={id}
      onClick={handleClick}
      // eslint-disable-next-line react/button-has-type
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;
