import { ReleaseVersion } from '@admin/services/releaseVersionService';
import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface ReleaseVersionContextState {
  releaseVersion: ReleaseVersion;
  releaseVersionId: string;
  onReleaseChange: () => void;
}

const ReleaseVersionContext = createContext<
  ReleaseVersionContextState | undefined
>(undefined);

interface ReleaseVersionContextProviderProps {
  children: ReactNode;
  releaseVersion: ReleaseVersion;
  onReleaseChange?: () => void;
}

export const ReleaseVersionContextProvider = ({
  children,
  releaseVersion,
  onReleaseChange = noop,
}: ReleaseVersionContextProviderProps) => {
  const value = useMemo<ReleaseVersionContextState>(() => {
    return {
      releaseVersion,
      releaseVersionId: releaseVersion.id,
      onReleaseChange,
    };
  }, [onReleaseChange, releaseVersion]);

  return (
    <ReleaseVersionContext value={value}>{children}</ReleaseVersionContext>
  );
};

export function useReleaseVersionContext() {
  const context = useContext(ReleaseVersionContext);

  if (!context) {
    throw new Error(
      'useReleaseVersionContext must be used within a ReleaseVersionContextProvider',
    );
  }

  return context;
}
