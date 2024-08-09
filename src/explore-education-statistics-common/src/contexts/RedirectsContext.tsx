import { Redirects } from '@common/utils/url/applyRedirectRules';
import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useMemo,
  useState,
} from 'react';

export interface RedirectsContextState {
  redirects: Redirects;
}

export const RedirectsContext = createContext<
  RedirectsContextState | undefined
>(undefined);

interface Props {
  children: ReactNode;
  refetchRedirects: () => Promise<Redirects>;
}

//let defaultRedirects = { publications: [], methodologies: [] };

export const RedirectsContextProvider = async ({
  children,
  refetchRedirects,
}: Props) => {
  const defaultRedirects = useMemo<Redirects>(
    async () => await refetchRedirects(),
    [refetchRedirects],
  );
  // const [redirectsState, setRedirectsState] = useState<RedirectsContextState>({
  //   redirects: defaultRedirects,
  // });

  // const setState = useCallback(async () => {
  //   const fetchedRedirects = await refetchRedirects();

  //   setRedirectsState({ redirects: fetchedRedirects });
  // }, [refetchRedirects]);

  return (
    <RedirectsContext.Provider value={{ redirects: defaultRedirects }}>
      {children}
    </RedirectsContext.Provider>
  );
};

export function useRedirectsContext(): RedirectsContextState {
  const context = useContext(RedirectsContext) as RedirectsContextState;

  if (!context) {
    throw new Error('Must be used with a RedirectsContextProvider');
  }

  return context;
}
