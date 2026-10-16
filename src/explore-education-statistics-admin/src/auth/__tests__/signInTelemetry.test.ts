import createSignInAttempt from '@admin/auth/signInTelemetry';
import { trackApplicationEvent } from '@common/services/applicationInsightsService';

jest.mock('@common/services/applicationInsightsService');
// The repository's asset transformer stubs the SDK's .cjs entry point.
jest.mock('@azure/msal-browser', () => ({
  InteractionRequiredAuthError: class extends Error {},
}));

describe('signInTelemetry', () => {
  const trackEvent = jest.fn();
  const flush = jest.fn();

  beforeEach(() => {
    sessionStorage.clear();
    jest
      .mocked(trackApplicationEvent)
      .mockImplementation((name, properties, shouldFlush) => {
        trackEvent({ name }, properties);
        if (shouldFlush) {
          flush();
        }
      });
  });

  test('deduplicates terminal outcomes and only sends allowlisted properties', () => {
    const attempt = createSignInAttempt('private-account');
    attempt.authorise();
    attempt.fail('permissions', new Error('secret token and email'));
    attempt.fail('permissions');
    attempt.succeed();

    expect(trackEvent).toHaveBeenCalledTimes(1);
    expect(trackEvent).toHaveBeenCalledWith(
      { name: 'AdminSignInOutcome' },
      {
        attemptId: expect.any(String),
        stage: 'permissions',
        outcome: 'failure',
        userContext: 'authorised',
      },
    );
    expect(flush).toHaveBeenCalledTimes(1);
  });

  test('only recognises the same previously accepted account', () => {
    createSignInAttempt('accepted').authorise();
    createSignInAttempt('accepted').fail('adminApi');
    createSignInAttempt('different').fail('adminApi');
    createSignInAttempt().fail('identityProvider');
    expect(trackEvent.mock.calls.map(call => call[1].userContext)).toEqual([
      'previouslyAuthorised',
      'unverified',
      'unverified',
    ]);
  });

  test('expires previous authorisation after 24 hours', () => {
    const now = jest.spyOn(Date, 'now');
    now.mockReturnValue(1);
    createSignInAttempt('accepted').authorise();
    now.mockReturnValue(24 * 60 * 60 * 1000 + 1);
    createSignInAttempt('accepted').fail('token');
    expect(trackEvent.mock.calls[0][1].userContext).toBe('unverified');
    now.mockRestore();
  });

  test.each(['interaction_required', 'login_required', 'consent_required'])(
    'excludes routine silent-token %s outcomes',
    errorCode => {
      createSignInAttempt().fail('token', { errorCode });
      expect(trackEvent.mock.calls[0][1].outcome).toBe('expected');
      expect(flush).not.toHaveBeenCalled();
    },
  );

  test('does not suppress an interactive login failure requiring interaction', () => {
    createSignInAttempt().fail('identityProvider', {
      errorCode: 'interaction_required',
    });
    expect(trackEvent.mock.calls[0][1].outcome).toBe('failure');
  });

  test('excludes cancelled logins and clears authorisation after access denial', () => {
    createSignInAttempt('accepted').authorise();
    createSignInAttempt('accepted').accessDenied();
    createSignInAttempt('accepted').fail('identityProvider', {
      errorCode: 'user_cancelled',
    });
    expect(trackEvent.mock.calls.map(call => call[1].outcome)).toEqual([
      'expected',
      'expected',
    ]);
    expect(trackEvent.mock.calls[1][1].userContext).toBe('unverified');
  });

  test('storage and telemetry failures do not break sign-in', () => {
    const read = jest
      .spyOn(Storage.prototype, 'getItem')
      .mockImplementation(() => {
        throw new Error('Storage blocked');
      });
    const write = jest
      .spyOn(Storage.prototype, 'setItem')
      .mockImplementation(() => {
        throw new Error('Storage blocked');
      });
    trackEvent.mockImplementation(() => {
      throw new Error('Telemetry unavailable');
    });
    expect(() => {
      const attempt = createSignInAttempt('accepted');
      attempt.authorise();
      attempt.succeed();
    }).not.toThrow();
    read.mockRestore();
    write.mockRestore();
  });
});
