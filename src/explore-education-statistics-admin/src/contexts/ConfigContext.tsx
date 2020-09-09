import { Config, getConfig } from '@admin/config';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { createContext, ReactNode, useContext } from 'react';

export const ConfigContext = createContext<Config | undefined>(undefined);

interface ConfigContextProviderProps {
  children: (config: Config) => ReactNode;
}

export const ConfigContextProvider = ({
  children,
}: ConfigContextProviderProps) => {
  const { value, isLoading } = useAsyncHandledRetry(getConfig);

  return (
    <ConfigContext.Provider value={value}>
      {!isLoading && value ? children(value) : null}
    </ConfigContext.Provider>
  );
};

export function useConfig(): Config {
  const config = useContext(ConfigContext);

  if (!config) {
    throw new Error('Must have a parent ConfigContextProvider');
  }

  return config;
}
