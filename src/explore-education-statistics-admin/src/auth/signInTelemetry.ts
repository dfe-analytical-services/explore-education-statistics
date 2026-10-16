import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { trackApplicationEvent } from '@common/services/applicationInsightsService';

type Stage =
  'identityProvider' | 'token' | 'adminApi' | 'permissions' | 'complete';
type Outcome = 'success' | 'failure' | 'expected';
type UserContext = 'unverified' | 'previouslyAuthorised' | 'authorised';

const storageKey = 'ees.admin.lastAuthorisedAccount';
const maximumAge = 24 * 60 * 60 * 1000;

function wasAuthorised(accountId?: string): boolean {
  try {
    const previous = JSON.parse(sessionStorage.getItem(storageKey) ?? 'null');
    return Boolean(
      accountId &&
      previous?.accountId === accountId &&
      Date.now() - previous.time >= 0 &&
      Date.now() - previous.time < maximumAge,
    );
  } catch {
    return false;
  }
}

/** One terminal event per attempt. Never send account IDs, tokens or error text. */
export default function createSignInAttempt(accountId?: string) {
  const attemptId = crypto.randomUUID();
  let completed = false;
  let userContext: UserContext = wasAuthorised(accountId)
    ? 'previouslyAuthorised'
    : 'unverified';

  function finish(stage: Stage, outcome: Outcome) {
    if (completed) {
      return;
    }
    completed = true;
    try {
      trackApplicationEvent(
        'AdminSignInOutcome',
        { attemptId, stage, outcome, userContext },
        outcome === 'failure',
      );
    } catch {
      // Monitoring must never prevent a user from signing in.
    }
  }

  return {
    authorise() {
      userContext = 'authorised';
      try {
        if (accountId) {
          // Kept only in this tab, never emitted. Match the same account, not just browser history.
          sessionStorage.setItem(
            storageKey,
            JSON.stringify({ accountId, time: Date.now() }),
          );
        }
      } catch {
        // Storage can be unavailable in privacy modes.
      }
    },
    accessDenied() {
      try {
        sessionStorage.removeItem(storageKey);
      } catch {
        // The terminal outcome still excludes this attempt from paging.
      }
      finish('adminApi', 'expected');
    },
    fail(stage: Exclude<Stage, 'complete'>, error?: unknown) {
      const code =
        typeof error === 'object' && error !== null && 'errorCode' in error
          ? error.errorCode
          : undefined;
      const expected =
        code === 'user_cancelled' ||
        (stage === 'token' &&
          (error instanceof InteractionRequiredAuthError ||
            [
              'interaction_required',
              'login_required',
              'consent_required',
            ].includes(String(code))));
      finish(stage, expected ? 'expected' : 'failure');
    },
    succeed() {
      finish('complete', 'success');
    },
  };
}
