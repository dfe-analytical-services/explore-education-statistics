import { allowedServiceNames, ServiceName, serviceSchemas } from './services';

/**
 * The oldest Azure Functions Core Tools whose bundled host runs on .NET 10.
 *
 * 4.12.1 and earlier ship a host built against .NET 9, so the framework
 * assemblies it loads the app's WebJobs extensions against are 9.x - and this
 * repo's services target net10.0 (see src/Directory.Build.props). The
 * extensions then fail to bind, which takes the whole script host down with
 * them.
 */
export const MIN_CORE_TOOLS_VERSION = '4.13.0';

/** Where to go to get a newer one, for the message that says to. */
const CORE_TOOLS_URL = 'https://github.com/Azure/azure-functions-core-tools';

/**
 * The services this applies to: every one the dashboard starts through a
 * Functions host. Derived rather than listed, so a new function app is
 * covered by adding it to the schemas and nothing else.
 */
export const functionHostServices = allowedServiceNames.filter(
  name => serviceSchemas[name].type === 'func',
);

interface FailurePattern {
  pattern: RegExp;
  /** What a matching line means. */
  describe: (match: RegExpMatchArray) => string;
  /**
   * Whether to quote the matched line alongside the description. Worth it
   * where the line carries context the description can't (which assembly, at
   * which version), and just noise where `describe` has already pulled out
   * everything the line had to say.
   */
  quoteLine?: boolean;
  /**
   * Whether this signature says the *host* is wrong for the app, rather than
   * the app's own startup having thrown for reasons of its own. Only the
   * former is worth answering with "upgrade Core Tools" - telling someone to
   * do that when their startup threw on a bad connection string would send
   * them off after the wrong thing entirely.
   */
  versionMismatch?: boolean;
}

/**
 * Log signatures of a Functions host that came up and then failed to
 * configure itself, most specific cause first.
 *
 * The ordering matters for the same reason it does for web apps: the line
 * naming the actual cause is logged once, during the first startup attempt,
 * while the host goes on repeating the symptom - a faulted health check every
 * 30 seconds - for as long as it's left running. Matching pattern-by-pattern
 * rather than taking whichever line comes first means the cause wins wherever
 * it sits in the log, and the symptom is only reported when nothing better
 * was found.
 */
const FAILURE_PATTERNS: FailurePattern[] = [
  {
    // The host loading the app's extensions against its own, older framework
    // assemblies. `Microsoft.Extensions.Options, Version=10.0.0.0` is what a
    // net10.0 app's extensions look like to a host that's still on net9.0.
    pattern: /Could not load file or assembly '([^,']+), Version=(\d+)\./,
    describe: ([, assembly, major]) =>
      `the Functions host can't load '${assembly}' ${major}.x`,
    versionMismatch: true,
  },
  {
    // Whatever the reason, the app's own startup class threw - so no
    // functions are indexed, and none of them will ever run.
    pattern: /Error configuring services in an external startup class/,
    describe: () => "its startup class couldn't configure the host's services",
  },
  {
    pattern: /A host error has occurred during startup operation/,
    describe: () => 'the Functions host errored during startup',
    // The line is that sentence plus a correlation guid, so quoting it adds
    // nothing but the guid.
    quoteLine: false,
  },
  {
    // Core Tools' own crash once the host it launched won't start, and the
    // signature the README's troubleshooting section is written around.
    pattern: /Value cannot be null\. \(Parameter:? '?provider'?\)/,
    describe: () => "the Functions host couldn't build its service provider",
    versionMismatch: true,
  },
  {
    // The symptom, kept last. This is the host's periodic health check, and
    // it repeats for as long as the host stays faulted - so it's the line
    // that will still be in the buffer when the cause has scrolled out of it,
    // and the least informative one when the cause hasn't.
    pattern: /"azure\.functions\.script_host\.lifecycle":{"status":"Unhealthy"/,
    describe: () => 'its script host is sitting in an error state',
    quoteLine: false,
  },
];

/** Long assembly load errors make for an unreadable banner. */
function truncate(line: string, maxLength = 160): string {
  const trimmed = line.trim();

  return trimmed.length > maxLength
    ? `${trimmed.slice(0, maxLength - 1)}…`
    : trimmed;
}

/**
 * Whether one line of a Functions host's output says the host has failed to
 * start its functions - returning what it says, or undefined if the line is
 * unremarkable.
 *
 * Split out from the whole-log scan below so a service can be marked
 * unhealthy the moment the line arrives, rather than only once something
 * asks. Both matter: this is what stops the dashboard reporting a faulted
 * host as 'Running', and the scan is what explains why.
 */
export function findFunctionHostFailureLine(line: string): string | undefined {
  // eslint-disable-next-line no-restricted-syntax
  for (const { pattern, describe } of FAILURE_PATTERNS) {
    const match = line.match(pattern);

    if (match) {
      return describe(match);
    }
  }

  return undefined;
}

/**
 * The Core Tools version a Functions host reported on startup, if its banner
 * is still in the given lines.
 *
 * Read out of the log rather than by running `func --version`, because what
 * matters is the version that actually started this service - PATH being what
 * it is, that isn't necessarily the one the dashboard would resolve.
 */
export function findCoreToolsVersion(
  lines: readonly string[],
): string | undefined {
  const banner = lines.find(line => /^Core Tools Version:/.test(line));

  return banner?.match(/^Core Tools Version:\s+(\d+\.\d+\.\d+)/)?.[1];
}

/** Whether `version` is older than {@link MIN_CORE_TOOLS_VERSION}. */
function isCoreToolsTooOld(version: string): boolean {
  const parts = version.split('.').map(Number);
  const minimum = MIN_CORE_TOOLS_VERSION.split('.').map(Number);
  const differing = parts.findIndex((part, index) => part !== minimum[index]);

  return differing !== -1 && parts[differing] < minimum[differing];
}

/**
 * What to tell the user to do about it.
 *
 * Three cases, because how confident the advice can be depends on what's
 * actually known. A Core Tools we can see is too old earns a flat
 * instruction; a version-mismatch signature on a Core Tools that looks new
 * enough earns a hedged one; and a startup that threw for some other reason
 * earns none at all - naming the version it's running is what makes the
 * difference checkable, and "upgrade Core Tools" is worse than silence for
 * someone whose problem is a bad connection string.
 */
function describeRemedy(
  version: string | undefined,
  versionMismatch: boolean,
): string {
  if (version && isCoreToolsTooOld(version)) {
    return (
      `Azure Functions Core Tools ${version} bundles a .NET 9 host, and these ` +
      `services target net10.0 - upgrade it to ${MIN_CORE_TOOLS_VERSION} or ` +
      `later, however you installed it (see ${CORE_TOOLS_URL}), then start it ` +
      `again.`
    );
  }

  if (versionMismatch) {
    return (
      `Usually means Azure Functions Core Tools is older than the ` +
      `${MIN_CORE_TOOLS_VERSION} these net10.0 services need` +
      `${version ? ` (this host reported ${version})` : ''} - check ` +
      `'func --version' and upgrade it (see ${CORE_TOOLS_URL}), then start it ` +
      `again.`
    );
  }

  return (
    `Download the full log to see what its startup threw. If Azure Functions ` +
    `Core Tools is older than ${MIN_CORE_TOOLS_VERSION} that's the usual ` +
    `cause, since these services target net10.0 - otherwise the failure is ` +
    `the app's own.`
  );
}

/**
 * Whether the given service's logs show its Functions host failing to start
 * its functions - returning the issue to report, or undefined if nothing
 * matched.
 *
 * Scanned regardless of the service's status, for the same reason the web app
 * scan is: `func` prints the banner the dashboard reads as "ready" before it
 * loads the app's extensions, so a host that faults on those has already been
 * marked running by the time it does. It then stays up, holding its port and
 * retrying the same failing startup, until someone stops it - which is why
 * this reads as "started fine, does nothing" from the outside.
 */
export default function findFunctionHostFailure(
  service: ServiceName,
  lines: readonly string[],
): { cause: string; message: string } | undefined {
  if (!functionHostServices.includes(service)) {
    return undefined;
  }

  // eslint-disable-next-line no-restricted-syntax
  for (const {
    pattern,
    describe,
    quoteLine = true,
    versionMismatch = false,
  } of FAILURE_PATTERNS) {
    // eslint-disable-next-line no-restricted-syntax
    for (const line of lines) {
      const match = line.match(pattern);

      if (match) {
        const cause = describe(match);
        const version = findCoreToolsVersion(lines);

        const quoted = quoteLine ? ` ("${truncate(line)}")` : '';

        return {
          cause,
          message:
            `${service}'s Functions host isn't running any functions: ` +
            `${cause}${quoted}. ${describeRemedy(version, versionMismatch)}`,
        };
      }
    }
  }

  return undefined;
}
