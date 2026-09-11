import { $ } from 'execa';
import fsp from 'node:fs/promises';
import https from 'node:https';
import path from 'node:path';
import process from 'node:process';
import { CORE_TOOLS_URL, MIN_CORE_TOOLS_VERSION } from './functionHostHealth';
import { projectRoot } from './services';
import {
  isOlderVersion,
  parseVersion,
  versionSatisfiesPin,
} from './utils/versions';

/**
 * A problem with the machine's tooling, as opposed to with a service - the
 * dashboard reports these in the same banner area as service issues, split by
 * severity.
 *
 * `error` is for tooling that will stop services working at all (no .NET SDK
 * that satisfies global.json, a Core Tools too old for these net10.0
 * services); `warning` is for a version that's merely drifted from what the
 * repo pins, where things mostly still work until they suddenly don't.
 */
export interface ToolIssue {
  id: string;
  severity: 'error' | 'warning';
  message: string;
}

/** What global.json pins, for the message that reports it unsatisfied. */
interface DotnetSdkPin {
  version: string;
  rollForward?: string;
}

/**
 * What .NET has to say when nothing satisfies global.json.
 *
 * `dotnet --version`, run from the project root, resolves the SDK exactly the
 * way a build would - global.json's version and rollForward policy included -
 * and fails when nothing installed satisfies them. That makes its exit code
 * the whole check: there's no version comparison to do here that wouldn't
 * just be reimplementing rollForward, wrongly.
 */
export function describeDotnetIssue(
  resolvedVersion: string | undefined,
  pin: DotnetSdkPin | undefined,
): ToolIssue | undefined {
  if (resolvedVersion || !pin) {
    return undefined;
  }

  const rollForward = pin.rollForward
    ? `, rollForward: ${pin.rollForward}`
    : '';

  return {
    id: 'tool-dotnet',
    severity: 'error',
    message:
      `'dotnet --version' fails in the project root, so no installed .NET SDK ` +
      `satisfies global.json (${pin.version}${rollForward}) - .NET services ` +
      `won't build until one is installed ` +
      `(https://dotnet.microsoft.com/download).`,
  };
}

/** Whether the Node running everything matches what .nvmrc pins. */
export function describeNodeIssue(
  installed: string,
  pinned: string | undefined,
): ToolIssue | undefined {
  if (!pinned || versionSatisfiesPin(installed, pinned)) {
    return undefined;
  }

  return {
    id: 'tool-node',
    severity: 'warning',
    message:
      `Node ${installed} is running the dashboard (and the services it ` +
      `starts), but .nvmrc pins ${pinned} - run 'nvm install' in the project ` +
      `root and restart the dashboard on the new version.`,
  };
}

/** Whether the python3 on PATH matches what .python-version pins. */
export function describePythonIssue(
  installed: string | undefined,
  pinned: string | undefined,
): ToolIssue | undefined {
  if (!pinned) {
    return undefined;
  }

  if (!installed) {
    return {
      id: 'tool-python',
      severity: 'warning',
      message:
        `python3 isn't on PATH, but .python-version pins ${pinned} - the ` +
        `Python tooling (robot tests, useful-scripts) won't run without it.`,
    };
  }

  if (versionSatisfiesPin(installed, pinned)) {
    return undefined;
  }

  return {
    id: 'tool-python',
    severity: 'warning',
    message:
      `python3 is ${installed}, but .python-version pins ${pinned} - install ` +
      `the pinned version (e.g. 'pyenv install ${pinned}') so the Python ` +
      `tooling runs against what the repo expects.`,
  };
}

/**
 * What to say about the installed Azure Functions Core Tools, given the
 * latest released version (when fetching it worked).
 *
 * Three distinct situations, in order of how loudly they're worth saying:
 * missing or older than {@link MIN_CORE_TOOLS_VERSION} means function
 * services will fail outright, which the log-scanning in functionHostHealth
 * can only report *after* a host has faulted - this is the same advice,
 * before anything is started. Merely behind the latest release is only a
 * nudge, but it's the nudge that stops the next minimum-version break
 * arriving as a mystery.
 */
export function describeCoreToolsIssue(
  installed: string | undefined,
  latest: string | undefined,
): ToolIssue | undefined {
  if (!installed) {
    return {
      id: 'tool-core-tools',
      severity: 'warning',
      message:
        `Azure Functions Core Tools isn't on PATH ('func --version' fails) - ` +
        `the function services won't start without ` +
        `${MIN_CORE_TOOLS_VERSION} or later (see ${CORE_TOOLS_URL}).`,
    };
  }

  if (isOlderVersion(installed, MIN_CORE_TOOLS_VERSION)) {
    return {
      id: 'tool-core-tools',
      severity: 'error',
      message:
        `Azure Functions Core Tools ${installed} bundles a .NET 9 host, and ` +
        `these services target net10.0, so their Functions hosts will fail ` +
        `to start - upgrade it to ${MIN_CORE_TOOLS_VERSION} or later, ` +
        `however you installed it (see ${CORE_TOOLS_URL}).`,
    };
  }

  if (latest && isOlderVersion(installed, latest)) {
    return {
      id: 'tool-core-tools',
      severity: 'warning',
      message:
        `A newer Azure Functions Core Tools is available (${installed} ` +
        `installed, ${latest} latest) - upgrade it however you installed it ` +
        `(see ${CORE_TOOLS_URL}).`,
    };
  }

  return undefined;
}

/**
 * The version a command reports, or undefined if it isn't installed (or its
 * output had no version in it - the same thing, as far as acting on it goes).
 *
 * Run from the project root so version managers that key off the pin files
 * (pyenv for .python-version, dotnet itself for global.json) resolve the same
 * tool a build run from there would.
 */
async function commandVersion(
  command: string,
  args: string[],
): Promise<string | undefined> {
  const result = await $({
    cwd: projectRoot,
    reject: false,
  })`${command} ${args}`;

  return result.failed ? undefined : parseVersion(result.stdout);
}

/** The raw contents of a pin file in the project root, if it exists. */
async function readPinFile(file: string): Promise<string | undefined> {
  try {
    return await fsp.readFile(path.join(projectRoot, file), 'utf8');
  } catch {
    return undefined;
  }
}

const LATEST_CORE_TOOLS_RELEASE_URL =
  'https://api.github.com/repos/Azure/azure-functions-core-tools/releases/latest';

/**
 * The version of the latest Core Tools release on GitHub, or undefined if
 * finding out failed - offline is a normal state for a laptop, so failure
 * here downgrades the check (installed vs latest) rather than producing an
 * issue of its own.
 */
function fetchLatestCoreToolsVersion(): Promise<string | undefined> {
  return new Promise(resolve => {
    const request = https.get(
      LATEST_CORE_TOOLS_RELEASE_URL,
      {
        headers: {
          // GitHub's API rejects requests without a User-Agent outright.
          'User-Agent': 'ees-dashboard',
          Accept: 'application/vnd.github+json',
        },
        timeout: 10_000,
      },
      response => {
        if (response.statusCode !== 200) {
          response.resume();
          resolve(undefined);
          return;
        }

        let body = '';
        response.setEncoding('utf8');
        response.on('data', chunk => {
          body += chunk;
        });
        response.on('end', () => {
          try {
            resolve(parseVersion(String(JSON.parse(body).tag_name ?? '')));
          } catch {
            resolve(undefined);
          }
        });
      },
    );

    // Destroying on timeout surfaces as an 'error', resolving undefined.
    request.on('timeout', () => request.destroy());
    request.on('error', () => resolve(undefined));
  });
}

/**
 * The latest Core Tools release, cached. Successes are kept for 6 hours -
 * releases don't move faster than that, and it keeps the dashboard from
 * leaning on GitHub's unauthenticated rate limit. Failures aren't cached at
 * all, so the next refresh retries.
 */
let latestCoreTools: { version: string; fetchedAt: number } | undefined;

const LATEST_CORE_TOOLS_CACHE_MS = 6 * 60 * 60 * 1000;

async function getLatestCoreToolsVersion(): Promise<string | undefined> {
  if (
    latestCoreTools &&
    Date.now() - latestCoreTools.fetchedAt < LATEST_CORE_TOOLS_CACHE_MS
  ) {
    return latestCoreTools.version;
  }

  const version = await fetchLatestCoreToolsVersion();

  if (version) {
    latestCoreTools = { version, fetchedAt: Date.now() };
  }

  return version;
}

async function checkDotnet(): Promise<ToolIssue | undefined> {
  const globalJson = await readPinFile('global.json');

  if (!globalJson) {
    return undefined;
  }

  let pin: DotnetSdkPin | undefined;

  try {
    const { sdk } = JSON.parse(globalJson);
    pin = sdk?.version ? sdk : undefined;
  } catch {
    return undefined;
  }

  return describeDotnetIssue(
    await commandVersion('dotnet', ['--version']),
    pin,
  );
}

async function checkNode(): Promise<ToolIssue | undefined> {
  const nvmrc = await readPinFile('.nvmrc');

  return describeNodeIssue(
    process.versions.node,
    nvmrc ? parseVersion(nvmrc) : undefined,
  );
}

async function checkPython(): Promise<ToolIssue | undefined> {
  const pythonVersion = await readPinFile('.python-version');

  if (!pythonVersion) {
    return undefined;
  }

  return describePythonIssue(
    await commandVersion('python3', ['--version']),
    parseVersion(pythonVersion),
  );
}

async function checkCoreTools(): Promise<ToolIssue | undefined> {
  const installed = await commandVersion('func', ['--version']);

  // Only worth an HTTP request when there's an installed version to compare
  // against - the missing-entirely message doesn't use it.
  const latest = installed ? await getLatestCoreToolsVersion() : undefined;

  return describeCoreToolsIssue(installed, latest);
}

/**
 * Every tooling issue worth showing, freshly checked - ordered by how much
 * else each one explains: the build tools that stop services starting first,
 * then the runtimes that have merely drifted.
 */
export default async function checkToolVersions(): Promise<ToolIssue[]> {
  const issues = await Promise.all([
    checkDotnet(),
    checkCoreTools(),
    checkNode(),
    checkPython(),
  ]);

  return issues.filter((issue): issue is ToolIssue => issue !== undefined);
}
