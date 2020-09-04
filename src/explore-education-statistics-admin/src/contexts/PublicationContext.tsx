import publicationService, {
  BasicPublicationDetails,
} from '@admin/services/publicationService';
import useAsyncHandledRetry, {
  AsyncHandledRetryState,
} from '@common/hooks/useAsyncHandledRetry';
import React, { createContext, ReactNode, useContext } from 'react';

const PublicationContext = createContext<
  AsyncHandledRetryState<BasicPublicationDetails> | undefined
>(undefined);

interface PublicationContextProviderProps {
  children:
    | ReactNode
    | ((state: AsyncHandledRetryState<BasicPublicationDetails>) => ReactNode);
  publicationId: string;
}

export const PublicationContextProvider = ({
  children,
  publicationId,
}: PublicationContextProviderProps) => {
  const state = useAsyncHandledRetry(
    () => publicationService.getPublication(publicationId),
    [publicationId],
  );

  return (
    <PublicationContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </PublicationContext.Provider>
  );
};

export default function usePublicationContext() {
  const context = useContext(PublicationContext);

  if (!context) {
    throw new Error('Must have a parent PublicationContextProvider');
  }

  return context;
}
