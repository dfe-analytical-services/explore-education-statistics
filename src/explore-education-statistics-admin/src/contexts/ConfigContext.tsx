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

// Config provider for testing

const defaultTestConfig: Config = {
  AppInsightsKey: '',
  PublicAppUrl: 'http://localhost',
  PermittedEmbedUrlDomains: ['https://department-for-education.shinyapps.io'],
};

interface TestConfigContextProps {
  children: ReactNode;
  config?: Config;
}

export const TestConfigContextProvider = ({
  children,
  config = defaultTestConfig,
}: TestConfigContextProps) => {
  return (
    <ConfigContext.Provider value={config}>{children}</ConfigContext.Provider>
  );
};
