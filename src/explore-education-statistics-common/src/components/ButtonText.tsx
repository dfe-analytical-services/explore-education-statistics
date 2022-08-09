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
import styles from './ButtonText.module.scss';

interface Props {
  ariaControls?: string;
  ariaDisabled?: boolean;
  ariaExpanded?: boolean;
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  disableDoubleClick?: boolean;
  id?: string;
  testId?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  type?: 'button' | 'submit' | 'reset';
  underline?: boolean;
  variant?: 'warning';
}

const ButtonText = (
  {
    ariaControls,
    ariaDisabled,
    ariaExpanded,
    children,
    className,
    disabled,
    disableDoubleClick,
    id,
    testId,
    type = 'button',
    underline = true,
    variant,
    onClick,
  }: Props,
  ref: Ref<HTMLButtonElement>,
) => {
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
        styles.button,
        {
          [styles.noUnderline]: !underline,
          [styles.warning]: variant === 'warning',
          [styles.disabled]: disabled,
        },
        className,
      )}
      data-testid={testId}
      disabled={ariaDisabled ? undefined : disabled || isClicking}
      id={id}
      onClick={handleClick}
      ref={ref}
      // eslint-disable-next-line react/button-has-type
      type={type}
    >
      {children}
    </button>
  );
};

export default forwardRef(ButtonText);
