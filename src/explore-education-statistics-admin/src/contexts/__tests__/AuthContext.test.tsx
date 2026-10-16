import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, useLocation } from 'react-router';
import { useMsal } from '@azure/msal-react';
import { acquireTokenSilent } from '@admin/auth/msal';
import {
  AuthContextProvider,
  useAuthContext,
} from '@admin/contexts/AuthContext';
import authService from '@admin/services/authService';
import permissionService from '@admin/services/permissionService';
import { trackApplicationEvent } from '@common/services/applicationInsightsService';
import createSignInAttempt from '@admin/auth/signInTelemetry';
import {
  expiredInviteRoute,
  noInvitationRoute,
  signInRoute,
} from '@admin/routes/routes';

jest.mock('@azure/msal-react', () => ({ useMsal: jest.fn() }));
jest.mock('@azure/msal-browser', () => ({
  InteractionRequiredAuthError: class extends Error {},
}));
jest.mock('@admin/auth/msal');
jest.mock('@admin/services/authService');
jest.mock('@admin/services/permissionService');
jest.mock('@common/services/applicationInsightsService');

function Status() {
  const { status } = useAuthContext();
  const { pathname } = useLocation();
  return <div>{`${status}:${pathname}`}</div>;
}

describe('AuthContext sign-in monitoring', () => {
  const trackEvent = jest.fn();
  const handleRedirectPromise = jest.fn();

  beforeEach(() => {
    sessionStorage.clear();
    handleRedirectPromise.mockResolvedValue(null);
    jest.mocked(useMsal).mockReturnValue({
      instance: { handleRedirectPromise },
      accounts: [{ homeAccountId: 'account' }],
      inProgress: 'none',
    } as unknown as ReturnType<typeof useMsal>);
    jest.mocked(acquireTokenSilent).mockResolvedValue({
      account: { homeAccountId: 'account' },
    } as Awaited<ReturnType<typeof acquireTokenSilent>>);
    jest.mocked(authService.signIn).mockResolvedValue({
      loginResult: 'LoginSuccess',
      userProfile: { id: 'user', firstName: 'Test' },
    });
    jest.mocked(permissionService.getGlobalPermissions).mockResolvedValue({
      canAccessSystem: true,
    } as Awaited<ReturnType<typeof permissionService.getGlobalPermissions>>);
    jest
      .mocked(trackApplicationEvent)
      .mockImplementation((name, properties) => {
        trackEvent({ name }, properties);
      });
  });

  function start() {
    render(
      <MemoryRouter>
        <AuthContextProvider>
          <Status />
        </AuthContextProvider>
      </MemoryRouter>,
    );
  }

  test('records success only after permissions load', async () => {
    start();
    await screen.findByText('authenticated:/');
    expect(trackEvent).toHaveBeenCalledTimes(1);
    expect(trackEvent.mock.calls[0][1]).toMatchObject({
      stage: 'complete',
      outcome: 'success',
      userContext: 'authorised',
    });
  });

  test('records a permissions failure for a confirmed user and returns to sign-in', async () => {
    jest
      .mocked(permissionService.getGlobalPermissions)
      .mockRejectedValue(new Error('Unavailable'));
    start();
    await screen.findByText(`unauthenticated:${signInRoute.fullPath}`);
    expect(trackEvent).toHaveBeenCalledTimes(1);
    expect(trackEvent.mock.calls[0][1]).toMatchObject({
      stage: 'permissions',
      outcome: 'failure',
      userContext: 'authorised',
    });
  });

  test('records API failure for a previously accepted account independently of 5xx', async () => {
    createSignInAttempt('account').authorise();
    jest
      .mocked(authService.signIn)
      .mockRejectedValue({ response: { status: 401 } });
    start();
    await screen.findByText(`unauthenticated:${signInRoute.fullPath}`);
    expect(trackEvent.mock.calls[0][1]).toMatchObject({
      stage: 'adminApi',
      outcome: 'failure',
      userContext: 'previouslyAuthorised',
    });
  });

  test.each([
    ['NoInvite', noInvitationRoute.fullPath],
    ['ExpiredInvite', expiredInviteRoute.fullPath],
  ] as const)('does not page for %s', async (loginResult, path) => {
    jest.mocked(authService.signIn).mockResolvedValue({ loginResult });
    start();
    await screen.findByText(`unauthenticated:${path}`);
    expect(trackEvent.mock.calls[0][1].outcome).toBe('expected');
    expect(permissionService.getGlobalPermissions).not.toHaveBeenCalled();
  });

  test('handles identity-provider redirect failure', async () => {
    handleRedirectPromise.mockRejectedValue({ errorCode: 'server_error' });
    jest.mocked(useMsal).mockReturnValue({
      instance: { handleRedirectPromise },
      accounts: [],
      inProgress: 'none',
    } as unknown as ReturnType<typeof useMsal>);
    start();
    await waitFor(() => expect(trackEvent).toHaveBeenCalledTimes(1));
    await screen.findByText(`unauthenticated:${signInRoute.fullPath}`);
    expect(trackEvent.mock.calls[0][1]).toMatchObject({
      stage: 'identityProvider',
      outcome: 'failure',
      userContext: 'unverified',
    });
  });

  test('excludes normal token expiry requiring interactive login', async () => {
    jest
      .mocked(acquireTokenSilent)
      .mockRejectedValue({ errorCode: 'login_required' });
    start();
    await screen.findByText(`unauthenticated:${signInRoute.fullPath}`);
    expect(trackEvent.mock.calls[0][1]).toMatchObject({
      stage: 'token',
      outcome: 'expected',
    });
    expect(authService.signIn).not.toHaveBeenCalled();
  });
});
