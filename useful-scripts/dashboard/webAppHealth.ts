import { projectRoot, ServiceName } from './services';

/**
 * The services that run a JavaScript app, and what to call that app when
 * reporting on it. Only these are worth scanning: the .NET APIs and Function
 * hosts have no node_modules or build output to go stale.
 *
 * `admin` is in here because it doesn't serve its own UI in development - it
 * shells out to the admin app's `start` script and proxies to the dev server
 * that comes up (see `spa.UseReactDevelopmentServer` in Startup.cs), so a
 * broken install over there surfaces as an admin failure.
 */
const webApps: Partial<Record<ServiceName, string>> = {
  admin: "the admin app's dev server",
  frontend: 'the public frontend',
  frontendProd: 'the public frontend',
};

/**
 * Whether a module specifier names an installed package rather than a file in
 * the repo. A relative or absolute specifier failing to resolve is a source
 * problem - a typo'd import, a file that hasn't been written yet - and saying
 * "reinstall your dependencies" to someone whose import path is simply wrong
 * would send them off for five minutes to learn nothing.
 */
function isPackageSpecifier(specifier: string): boolean {
  return !/^[.]|^\/|^[a-zA-Z]:[\\/]/.test(specifier);
}

interface FailurePattern {
  pattern: RegExp;
  /**
   * What a matching line means, or undefined to reject the match after
   * looking at its captures (e.g. a module specifier that turns out to be a
   * relative path rather than a package).
   */
  describe: (match: RegExpMatchArray) => string | undefined;
  /**
   * Whether to quote the matched line alongside the description. Worth it
   * where the line carries context the description can't (a stack frame, the
   * app that logged it), and just noise where `describe` has already pulled
   * out everything the line had to say.
   */
  quoteLine?: boolean;
}

/**
 * Log signatures of a JavaScript app failing for want of a working install or
 * build, most specific cause first.
 *
 * Order is what makes this useful rather than merely accurate. The thing you
 * see - admin returning a 500, the frontend returning "Internal Server Error"
 * - is several steps downstream of the thing that's wrong, and the line naming
 * the actual cause is logged once, at startup, while the symptom repeats on
 * every request. Matching pattern-by-pattern (rather than taking whichever
 * line comes first) means the cause wins wherever it happens to sit in the
 * log, and the symptom is only ever reported when nothing better was found.
 */
const FAILURE_PATTERNS: FailurePattern[] = [
  {
    // Node's own resolution failure - `Cannot find module 'swc-loader'` is
    // what a dependency that's in the lockfile but not on disk looks like.
    pattern: /Cannot find (?:module|package) '([^']+)'/,
    describe: ([, specifier]) =>
      isPackageSpecifier(specifier)
        ? `the '${specifier}' package isn't installed`
        : undefined,
  },
  {
    // The same failure seen through a bundler (webpack for the admin app,
    // Next.js for the frontend) rather than through Node.
    pattern: /Module not found: Can't resolve '([^']+)'/,
    describe: ([, specifier]) =>
      isPackageSpecifier(specifier)
        ? `the '${specifier}' package isn't installed`
        : undefined,
  },
  {
    pattern: /ERR_MODULE_NOT_FOUND/,
    describe: () => "a package it imports isn't installed",
  },
  {
    // Next.js reading a manifest that its own build should have written. The
    // dev server starts and compiles quite happily either way, so this only
    // shows up per-request, as a 500.
    pattern:
      /ENOENT: no such file or directory, \w+ '([^']*[\\/]\.next[\\/][^']*)'/,
    describe: ([, file]) => `its build output is missing ${file}`,
    // The line is a bare ENOENT for that same path, so quoting it would put
    // the same long path in the banner twice, the second time truncated.
    quoteLine: false,
  },
  {
    // The symptom, kept last. Admin's SPA middleware reports this on every
    // request once the dev server has died, whatever killed it - so it's the
    // line you'll still have when the cause has scrolled out of the buffer,
    // and the least informative one when you haven't.
    pattern:
      /The npm script '.*' exited without indicating that the create-react-app server was listening/,
    describe: () => 'its dev server exited instead of starting up',
  },
];

/** Long paths and stack frames make for an unreadable banner. */
function truncate(line: string, maxLength = 160): string {
  const trimmed = line.trim();

  return trimmed.length > maxLength
    ? `${trimmed.slice(0, maxLength - 1)}…`
    : trimmed;
}

/**
 * Whether the given service's logs show its JavaScript app failing because
 * the checkout's node_modules or build output don't match what it needs -
 * returning the issue to report, or undefined if nothing matched.
 *
 * Scanned regardless of the service's status, because these failures don't
 * stop it: admin starts, reports itself healthy, and only shells out to the
 * dev server when the first request arrives, so by the time this is detectable
 * the service has been sitting there 'running' for some time. The logs are
 * cleared whenever a service is started, so a restart after the fix clears the
 * issue with it.
 */
export default function findWebAppFailure(
  service: ServiceName,
  lines: readonly string[],
): { cause: string; message: string } | undefined {
  const app = webApps[service];

  if (!app) {
    return undefined;
  }

  // eslint-disable-next-line no-restricted-syntax
  for (const { pattern, describe, quoteLine = true } of FAILURE_PATTERNS) {
    // eslint-disable-next-line no-restricted-syntax
    for (const line of lines) {
      const match = line.match(pattern);
      const cause = match && describe(match);

      if (cause) {
        return {
          cause,
          message:
            `${app} isn't working: ${cause}` +
            `${quoteLine ? ` ("${truncate(line)}")` : ''}. ` +
            `Usually means node_modules or the build output are out of date - ` +
            `run 'pnpm clean && pnpm i' in ${projectRoot}, then start it again.`,
        };
      }
    }
  }

  return undefined;
}

/** The services this scans, for callers that check all of them. */
export const webAppServices = Object.keys(webApps) as ServiceName[];
