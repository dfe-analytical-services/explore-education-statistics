import { Config, getConfig } from '@admin/config';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { createContext, ReactNode, useContext } from 'react';

export const ConfigContext = createContext<Config | undefined>(undefined);

interface ConfigContextProviderProps {
  children?: ReactNode;
}

export const ConfigContextProvider = ({
  children,
}: ConfigContextProviderProps) => {
  const { value, isLoading } = useAsyncHandledRetry(getConfig);

  return (
    <ConfigContext value={value}>
      {!isLoading && value ? children : null}
    </ConfigContext>
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
  appInsightsKey: '',
  publicAppUrl: 'http://localhost',
  publicApiUrl: 'http://public-api',
  publicApiDocsUrl: 'http://public-api-docs',
  permittedEmbedUrlDomains: ['https://department-for-education.shinyapps.io'],
  oidc: {
    clientId: '',
    authority: '',
    knownAuthorities: [''],
    adminApiScope: '',
    authorityMetadata: {
      authorizationEndpoint: '',
      tokenEndpoint: '',
      issuer: '',
      userInfoEndpoint: '',
      endSessionEndpoint: '',
    },
  },
};

interface TestConfigContextProps {
  children: ReactNode;
  config?: Config;
}

export const TestConfigContextProvider = ({
  children,
  config = defaultTestConfig,
}: TestConfigContextProps) => {
  return <ConfigContext value={config}>{children}</ConfigContext>;
};
