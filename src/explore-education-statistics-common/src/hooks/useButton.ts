import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import { MouseEventHandler, ReactNode, useCallback } from 'react';

export interface ButtonOptions {
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
  variant?: 'secondary' | 'warning';
}

export default function useButton({
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
  underline = true,
  variant,
  onClick,
}: ButtonOptions) {
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

  return {
    'aria-controls': ariaControls,
    'aria-disabled': isDisabled,
    'aria-expanded': ariaExpanded,
    children,
    className,
    'data-testid': testId,
    disabled: ariaDisabled ? undefined : disabled || isClicking,
    id,
    isDisabled,
    type,
    underline,
    variant,
    onClick: handleClick,
  };
}
