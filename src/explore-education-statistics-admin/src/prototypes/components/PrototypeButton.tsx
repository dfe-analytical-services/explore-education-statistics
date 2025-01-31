import styles from '@common/components/Button.module.scss';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, {
  forwardRef,
  MouseEventHandler,
  ReactNode,
  Ref,
  useCallback,
} from 'react';

export interface ButtonProps {
  ariaControls?: string;
  ariaDisabled?: boolean;
  ariaExpanded?: boolean;
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

function Button(
  {
    ariaControls,
    ariaDisabled,
    ariaExpanded,
    children,
    className,
    disabled = false,
    disableDoubleClick = true,
    id,
    testId,
    type = 'button',
    variant,
    onClick,
  }: ButtonProps,
  ref: Ref<HTMLButtonElement>,
) {
  const [isClicking, toggleClicking] = useToggle(false);
  const isMountedRef = useMountedRef();

  const handleClick: MouseEventHandler<HTMLButtonElement> = useCallback(
    async event => {
      if (ariaDisabled || disabled) {
        return;
      }

      if (disableDoubleClick) {
        toggleClicking.on();
      }

      await onClick?.(event);

      if (disableDoubleClick && isMountedRef.current) {
        toggleClicking.off();
      }
    },
    [
      ariaDisabled,
      disabled,
      disableDoubleClick,
      isMountedRef,
      onClick,
      toggleClicking,
    ],
  );

  const isDisabled = ariaDisabled || disabled || isClicking;

  return (
    <button
      aria-controls={ariaControls}
      aria-disabled={isDisabled}
      aria-expanded={ariaExpanded}
      className={classNames(
        'govuk-button',
        {
          [styles.disabled]: isDisabled,
          'govuk-button--disabled': isDisabled,
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
      data-testid={testId}
      disabled={ariaDisabled ? undefined : disabled || isClicking}
      id={id}
      ref={ref}
      onClick={handleClick}
      // eslint-disable-next-line react/button-has-type
      type={type}
    >
      {children}
    </button>
  );
}

export default forwardRef(Button);
