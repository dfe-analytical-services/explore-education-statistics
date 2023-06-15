import useToggle from '@common/hooks/useToggle';
import React, {
  createContext,
  MutableRefObject,
  ReactNode,
  useContext,
  useMemo,
  useRef,
} from 'react';

export interface ConfirmContextValue {
  isConfirming: boolean;
  askConfirm(): Promise<boolean>;
  confirm(): void;
  cancel(): void;
}

export const ConfirmContext = createContext<ConfirmContextValue | undefined>(
  undefined,
);

interface Props {
  children: ReactNode | ((contextProps: ConfirmContextValue) => ReactNode);
}

export const ConfirmContextProvider = ({ children }: Props) => {
  const [isConfirming, toggleConfirming] = useToggle(false);

  const resolver: MutableRefObject<((confirmed: boolean) => void) | null> =
    useRef(null);

  const value = useMemo<ConfirmContextValue>(() => {
    return {
      isConfirming,
      askConfirm() {
        toggleConfirming(true);

        return new Promise<boolean>(resolve => {
          resolver.current = resolve;
        });
      },
      confirm() {
        if (resolver.current) {
          resolver.current(true);
          toggleConfirming(false);
          resolver.current = null;
        }
      },
      cancel() {
        if (resolver.current) {
          resolver.current(false);
          toggleConfirming(false);
          resolver.current = null;
        }
      },
    };
  }, [isConfirming, toggleConfirming]);

  return (
    <ConfirmContext.Provider value={value}>
      {typeof children === 'function' ? children(value) : children}
    </ConfirmContext.Provider>
  );
};

export function useConfirmContext(): ConfirmContextValue {
  const context = useContext(ConfirmContext) as ConfirmContextValue;

  if (!context) {
    throw new Error('Must be used with a ConfirmContextProvider');
  }

  return context;
}
