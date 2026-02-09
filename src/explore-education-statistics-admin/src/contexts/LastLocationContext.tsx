import usePrevious from '@common/hooks/usePrevious';
import { Location } from 'history';
import React, { createContext, ReactNode, useContext } from 'react';
import { useLocation } from 'react-router';

const LastLocationContext = createContext<Location | undefined>(undefined);

interface LastLocationContextProviderProps {
  children?: ReactNode;
}

export const LastLocationContextProvider = ({
  children,
}: LastLocationContextProviderProps) => {
  const location = useLocation();
  const lastLocation = usePrevious(location);

  return (
    <LastLocationContext value={lastLocation}>{children}</LastLocationContext>
  );
};

export function useLastLocation(): Location | undefined {
  return useContext(LastLocationContext);
}
