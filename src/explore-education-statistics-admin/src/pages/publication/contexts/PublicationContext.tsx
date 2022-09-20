import { Publication } from '@admin/services/publicationService';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface PublicationContextState {
  publication: Publication;
  publicationId: string;
  onPublicationChange: (nextPublication: Publication) => void;
  onReload: () => void;
}

const PublicationContext = createContext<PublicationContextState | undefined>(
  undefined,
);

interface PublicationContextProviderProps {
  children: ReactNode;
  publication: Publication;
  onPublicationChange: (nextPublication: Publication) => void;
  onReload: () => void;
}

export const PublicationContextProvider = ({
  children,
  publication,
  onPublicationChange,
  onReload,
}: PublicationContextProviderProps) => {
  const value = useMemo<PublicationContextState>(() => {
    return {
      publication,
      publicationId: publication.id,
      onPublicationChange,
      onReload,
    };
  }, [publication, onPublicationChange, onReload]);

  return (
    <PublicationContext.Provider value={value}>
      {children}
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
