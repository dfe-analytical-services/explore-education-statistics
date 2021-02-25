import { Release } from '@admin/services/releaseService';
import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface ReleaseContextState {
  release: Release;
  releaseId: string;
  onReleaseChange: (nextRelease: Release) => void;
}

const ReleaseContext = createContext<ReleaseContextState | undefined>(
  undefined,
);

interface ReleaseContextProviderProps {
  children: ReactNode;
  release: Release;
  onReleaseChange?: (nextRelease: Release) => void;
}

export const ReleaseContextProvider = ({
  children,
  release,
  onReleaseChange = noop,
}: ReleaseContextProviderProps) => {
  const value = useMemo<ReleaseContextState>(() => {
    return {
      release,
      releaseId: release.id,
      onReleaseChange,
    };
  }, [onReleaseChange, release]);

  return (
    <ReleaseContext.Provider value={value}>{children}</ReleaseContext.Provider>
  );
};

export function useReleaseContext() {
  const context = useContext(ReleaseContext);

  if (!context) {
    throw new Error(
      'useReleaseContext must be used within a ReleaseContextProvider',
    );
  }
  return context;
}
