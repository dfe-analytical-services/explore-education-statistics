import React, { ReactNode } from 'react';
import { useConfig } from '@admin/contexts/ConfigContext';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { createMsalInstance } from '@admin/auth/msal';
import { MsalProvider } from '@azure/msal-react';

interface Props {
  children?: ReactNode;
}

/**
 * This component is responsible for providing child components with a
 * configured MSAL instance, accessed via the "useMsal()" hook.
 *
 * The configuration for the MSAL instance is acquired from the
 * ConfigurationController in the Admin API, and has the ability to redirect
 * the user to the Identity Provider's login page, handle post-login redirects,
 * and acquire new access tokens for the current user.
 */
export const ConfiguredMsalProvider = ({ children }: Props) => {
  const { oidc } = useConfig();
  const { value: msalInstance, isLoading } = useAsyncHandledRetry(() =>
    createMsalInstance(oidc),
  );
  return !isLoading && msalInstance ? (
    <MsalProvider instance={msalInstance}>{children}</MsalProvider>
  ) : null;
};

export default ConfiguredMsalProvider;
