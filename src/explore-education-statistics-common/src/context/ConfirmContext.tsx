import useToggle from '@common/hooks/useToggle';
import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';

export interface ConfirmContextValue {
  isConfirming: boolean;
  toggleConfirming(confirming?: boolean): void;
  handleConfirm(): void;
  handleCancel(): void;
  setHandlers(handlers: { onConfirm?(): void; onCancel?(): void }): void;
}

export const ConfirmContext = createContext<ConfirmContextValue | undefined>(
  undefined,
);

interface Props {
  children: ReactNode;
}

export const ConfirmContextProvider = ({ children }: Props) => {
  const [isConfirming, toggleConfirming] = useToggle(false);
  const [handlers, setHandlers] = useState<{
    onConfirm?(): void;
    onCancel?(): void;
  }>({});

  const value = useMemo<ConfirmContextValue>(() => {
    return {
      isConfirming,
      toggleConfirming,
      setHandlers,
      handleConfirm() {
        if (handlers.onConfirm) {
          handlers.onConfirm();
        }

        setHandlers({});
      },
      handleCancel() {
        if (handlers.onCancel) {
          handlers.onCancel();
        }

        setHandlers({});
      },
    };
  }, [isConfirming, toggleConfirming, handlers]);

  return (
    <ConfirmContext.Provider value={value}>{children}</ConfirmContext.Provider>
  );
};

export function useConfirmContext() {
  const context = useContext(ConfirmContext);

  if (!context) {
    throw new Error('Must be used with a ConfirmContextProvider');
  }

  const {
    isConfirming,
    toggleConfirming,
    handleCancel,
    handleConfirm,
    setHandlers,
  } = context;

  return {
    isConfirming,
    askConfirm(onConfirm?: () => void, onCancel?: () => void) {
      setHandlers({
        onConfirm,
        onCancel,
      });

      toggleConfirming(true);
    },
    confirm() {
      handleConfirm();
      toggleConfirming(false);
    },
    cancel() {
      handleCancel();
      toggleConfirming(false);
    },
  };
}
