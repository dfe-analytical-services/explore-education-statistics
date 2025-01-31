import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import { MouseEvent, MouseEventHandler, ReactNode, useCallback } from 'react';

export interface ButtonOptions {
  ariaControls?: string;
  ariaCurrent?: 'page';
  ariaDisabled?: boolean;
  ariaExpanded?: boolean;
  ariaLabel?: string;
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  id?: string;
  preventDoubleClick?: boolean;
  testId?: string;
  type?: 'button' | 'submit' | 'reset';
  onClick?: (event: MouseEvent<HTMLButtonElement>) => void | Promise<void>;
}

export default function useButton({
  ariaControls,
  ariaCurrent,
  ariaDisabled,
  ariaExpanded,
  ariaLabel,
  children,
  className,
  disabled,
  id,
  preventDoubleClick = true,
  testId,
  type = 'button',
  onClick,
}: ButtonOptions) {
  const [isClicking, toggleClicking] = useToggle(false);
  const isMountedRef = useMountedRef();

  const isDisabled = ariaDisabled || disabled || isClicking;

  const handleClick: MouseEventHandler<HTMLButtonElement> = useCallback(
    async event => {
      if (isDisabled) {
        event.preventDefault();
        return;
      }

      if (preventDoubleClick) {
        toggleClicking.on();
      }

      await onClick?.(event);

      if (preventDoubleClick && isMountedRef.current) {
        toggleClicking.off();
      }
    },
    [isDisabled, preventDoubleClick, onClick, isMountedRef, toggleClicking],
  );

  return {
    'aria-controls': ariaControls,
    'aria-current': ariaCurrent,
    'aria-disabled': isDisabled,
    'aria-expanded': ariaExpanded,
    'aria-label': ariaLabel,
    children,
    className,
    'data-testid': testId,
    disabled,
    id,
    type,
    onClick: handleClick,
  };
}
