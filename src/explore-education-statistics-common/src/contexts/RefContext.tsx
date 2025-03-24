import {
  createContext,
  MutableRefObject,
  ReactNode,
  useContext,
  useRef,
} from 'react';

export const RefContext = createContext<MutableRefObject<null> | undefined>(
  undefined,
);

export const RefContextProvider = ({ children }: { children: ReactNode }) => {
  const ref = useRef(null);

  return <RefContext.Provider value={ref}>{children}</RefContext.Provider>;
};

export function useRefContext(): MutableRefObject<null> | undefined {
  const context = useContext(RefContext) as MutableRefObject<null> | undefined;

  if (!context) {
    throw new Error('Must be used within a RefContextProvider');
  }

  return context;
}
