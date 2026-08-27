import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import findFunctionHostFailure, {
  findCoreToolsVersion,
  findFunctionHostFailureLine,
  findIndexedFunction,
  functionHostServices,
  MIN_CORE_TOOLS_VERSION,
  readFunctionHostLine,
} from './functionHostHealth';

/**
 * The log lines here are real ones, copied from data/dashboard-logs after a
 * publisher and a processor had both been started against Core Tools 4.12.1 -
 * whose host is built for .NET 9, so it can't load the net10.0 extensions
 * these services build. That matters more than usual for this module:
 * everything it does is pattern matching against output nothing in this repo
 * controls, so a test written against invented lines would only prove the
 * patterns match themselves.
 */
const publisherLogs = [
  'Azure Functions Core Tools',
  'Core Tools Version:       4.12.1+7f573ec7a43edc895383eb81b78fe86ead2b707b (64-bit)',
  'Function Runtime Version: 4.1048.200.26180',
  '[2026-08-25T13:23:48.613Z] csproj (or fsproj) not found in /home/x/ees/src/artifacts/bin/GovUk.Education.ExploreEducationStatistics.Publisher/debug directory tree. Skipping user secrets file configuration.',
  '[2026-08-25T13:23:49.134Z] Error configuring services in an external startup class.',
  "[2026-08-25T13:23:49.134Z] Error configuring services in an external startup class. Microsoft.Azure.WebJobs.Extensions.Storage.Queues: Could not load file or assembly 'Microsoft.Extensions.Options, Version=10.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'. The system cannot find the file specified.",
  "[2026-08-25T13:23:49.135Z] A host error has occurred during startup operation '23ea45b7-6f9b-43ce-a782-fd88b51054d1'.",
  "Value cannot be null. (Parameter 'provider')",
  'Press any key to continue....',
];

// The host's own readiness probe, logged every 30 seconds for as long as it
// stays faulted. Note that it quotes the startup error inside its JSON, so
// anything testing the fallback has to drop this line too.
const healthCheckLine =
  '[2026-08-25T13:23:55.255Z] [Tag=\'\'] Process reporting unhealthy: Unhealthy. Health check entries are {"azure.functions.web_host.lifecycle":{"status":"Healthy","description":null},"azure.functions.script_host.lifecycle":{"status":"Unhealthy","description":"Script host in error state: \\nError configuring services in an external startup class.","errorCode":"Faulted"},"azure.functions.webjobs.storage":{"status":"Healthy","description":null}}';

// publicProcessor on the same machine, at the same Core Tools version. It uses
// the Durable Task extension rather than Azure Storage Queues, so it has
// nothing to fail on and starts perfectly happily - which is what made the
// two that didn't so confusing.
const healthyLogs = [
  'Azure Functions Core Tools',
  'Core Tools Version:       4.12.1+7f573ec7a43edc895383eb81b78fe86ead2b707b (64-bit)',
  'Function Runtime Version: 4.1048.200.26180',
  '[2026-08-25T13:06:32.658Z] Worker process started and initialized.',
  'Functions:',
  '\tCreateDataSet: [POST] http://localhost:7074/api/CreateDataSet',
];

/**
 * processor, an hour and a half into a run that had started cleanly on Core
 * Tools 4.14.0 and already executed a function. Its language worker then
 * exited, and the host re-launched it - printing, as it does, the same lines
 * it prints when a startup fails. It was serving queues again nine seconds
 * later. Also copied from data/dashboard-logs, and the log that prompted all
 * of this.
 */
const recycledWorkerLogs = [
  'Azure Functions Core Tools',
  'Core Tools Version:       4.14.0+4a17060ecc915f1672d86717a487ace30f535e74 (64-bit)',
  'Function Runtime Version: 4.1052.200.26352',
  '[2026-08-27T09:54:27.257Z] Worker process started and initialized.',
  'Functions:',
  '\tCancelImports: queueTrigger',
  '\tProcessUploads: queueTrigger',
  "[2026-08-27T09:54:33.292Z] Executed 'Functions.RestartImports' (Succeeded, Id=eccf2451-8bbe-4173-b1f2-8c9b981a6142, Duration=5440ms)",
  '[2026-08-27T11:33:29.279Z] Language Worker Process exited. Pid=198054.',
  '[2026-08-27T11:33:29.287Z] Error building configuration in an external startup class.',
  "[2026-08-27T11:33:29.290Z] A host error has occurred during startup operation '1ad3c1ab-f72e-4016-b951-170f7de96ed3'.",
  '[2026-08-27T11:33:38.879Z] Worker process started and initialized.',
];

describe('findFunctionHostFailure', () => {
  it('names the assembly the host could not load', () => {
    const failure = findFunctionHostFailure('publisher', publisherLogs);

    assert.ok(failure);
    assert.match(failure.cause, /can't load 'Microsoft\.Extensions\.Options'/);
    assert.match(failure.cause, /10\.x/);
  });

  it('tells you to upgrade Core Tools, and to what', () => {
    const failure = findFunctionHostFailure('publisher', publisherLogs);

    assert.ok(failure);
    assert.match(failure.message, /Core Tools 4\.12\.1/);
    assert.match(failure.message, /4\.13\.0 or later/);
    assert.match(failure.message, /github\.com\/Azure/);
  });

  it('prefers the cause over the symptom it produces', () => {
    // The line that repeats forever is the faulted health check, and the one
    // that says what's actually wrong is logged once. Reporting the former
    // would leave you exactly where the dashboard already had you: something
    // is wrong with publisher, no idea what.
    const failure = findFunctionHostFailure('publisher', [
      ...publisherLogs,
      healthCheckLine,
    ]);

    assert.ok(failure);
    assert.match(failure.cause, /can't load/);
  });

  it('still finds the cause quoted inside the repeating symptom', () => {
    // The health check embeds the startup error in its `description`, so once
    // everything else has scrolled out of the buffer the line that's left
    // still says what went wrong - and it's worth knowing that the more
    // specific pattern picks it out of the JSON rather than giving up and
    // reporting "something is unhealthy".
    const failure = findFunctionHostFailure('publisher', [
      'Core Tools Version:       4.12.1+7f573ec (64-bit)',
      healthCheckLine,
    ]);

    assert.ok(failure);
    assert.match(failure.cause, /startup class/);
  });

  it('falls back to the symptom when it carries no cause of its own', () => {
    const failure = findFunctionHostFailure('publisher', [
      '[2026-08-25T13:23:55.258Z] [Tag=\'azure.functions.readiness\'] Process reporting unhealthy: Unhealthy. Health check entries are {"azure.functions.script_host.lifecycle":{"status":"Unhealthy","description":null,"errorCode":"Faulted"}}',
    ]);

    assert.ok(failure);
    assert.match(failure.cause, /script host/);
  });

  it("reports the Core Tools crash the README's troubleshooting names", () => {
    const failure = findFunctionHostFailure('processor', [
      "Value cannot be null. (Parameter 'provider')",
    ]);

    assert.ok(failure);
    assert.match(failure.cause, /service provider/);
  });

  it('matches that crash however Core Tools punctuates it', () => {
    // The README quotes it with a colon, the version we have prints it
    // without. Neither spelling is ours to rely on.
    ["(Parameter: 'provider')", "(Parameter 'provider')"].forEach(suffix => {
      assert.ok(
        findFunctionHostFailure('processor', [
          `Value cannot be null. ${suffix}`,
        ]),
        `${suffix} should be recognised`,
      );
    });
  });

  it('hedges when the host is on a Core Tools new enough to work', () => {
    // Same symptom on a current Core Tools is a different problem, and
    // flatly telling someone to upgrade what they just upgraded is worse
    // than saying nothing.
    const failure = findFunctionHostFailure(
      'publisher',
      publisherLogs.map(line =>
        line.replace('4.12.1+7f573ec', '4.14.0+7f573ec'),
      ),
    );

    assert.ok(failure);
    assert.match(failure.message, /Usually means/);
    assert.match(failure.message, /this host reported 4\.14\.0/);
  });

  it("doesn't blame Core Tools for a startup that threw on its own", () => {
    // A startup class can fail for reasons that have nothing to do with the
    // host - a connection string it can't parse, a missing setting. The
    // service still needs marking broken, but sending someone off to upgrade
    // Core Tools over it would waste their afternoon.
    const failure = findFunctionHostFailure('publisher', [
      'Core Tools Version:       4.14.0+4a17060 (64-bit)',
      '[2026-08-25T13:23:49.134Z] Error configuring services in an external startup class.',
    ]);

    assert.ok(failure);
    assert.doesNotMatch(failure.message, /upgrade it/);
    assert.match(failure.message, /full log/);
    // And with the banner right there in the log, Core Tools doesn't even
    // need raising as a suspect - naming the minimum would only send someone
    // off to check a version this has already read.
    assert.match(failure.message, /4\.14\.0 is new enough/);
    assert.doesNotMatch(failure.message, new RegExp(MIN_CORE_TOOLS_VERSION));
  });

  it('stays quiet about a host that recycled its worker and carried on', () => {
    assert.equal(
      findFunctionHostFailure('processor', recycledWorkerLogs),
      undefined,
    );
  });

  it('believes the caller over a buffer the function list has left', () => {
    // What the dashboard passes is what it watched across the whole run. What
    // it has left to show is the last 500 lines, which for a recycle this far
    // in is all symptom and no history.
    const scrolled = recycledWorkerLogs.slice(-4);

    assert.ok(findFunctionHostFailure('processor', scrolled));
    assert.equal(
      findFunctionHostFailure('processor', scrolled, true),
      undefined,
    );
  });

  it('still mentions the version requirement when nothing rules it out', () => {
    // Same failure with no banner left to say what Core Tools is running:
    // worth naming the minimum, since it can't be ruled out.
    const failure = findFunctionHostFailure('publisher', [
      '[2026-08-25T13:23:49.134Z] Error configuring services in an external startup class.',
    ]);

    assert.ok(failure);
    assert.match(failure.message, new RegExp(MIN_CORE_TOOLS_VERSION));
  });

  it('says nothing about a Functions host that is working', () => {
    assert.equal(
      findFunctionHostFailure('publicProcessor', healthyLogs),
      undefined,
    );
  });

  it('ignores services that run no Functions host', () => {
    // 'A host error has occurred' out of a .NET web API means something else
    // entirely, and upgrading Core Tools would do nothing for it.
    assert.equal(
      findFunctionHostFailure('admin', [
        "[2026-08-25T13:23:49.135Z] A host error has occurred during startup operation 'x'.",
      ]),
      undefined,
    );
  });

  it('trims the length off the line it quotes', () => {
    const failure = findFunctionHostFailure('publisher', [
      `Could not load file or assembly 'Foo, Version=10.0.0.0 ${'x'.repeat(500)}`,
    ]);

    assert.ok(failure);
    assert.ok(
      failure.message.length < 500,
      `banner message was ${failure.message.length} characters`,
    );
  });

  it('covers exactly the services that run a Functions host', () => {
    assert.deepEqual(functionHostServices, [
      'analytics',
      'processor',
      'publicProcessor',
      'publisher',
      'notifier',
      'searchFunctionApp',
    ]);
  });
});

describe('findFunctionHostFailureLine', () => {
  // This is the half that keeps a faulted host off 'Running', so it has to
  // match on the line as it arrives, with no other lines for context.
  it('recognises each failure line on its own', () => {
    const failing = publisherLogs.filter(line =>
      findFunctionHostFailureLine(line),
    );

    assert.equal(failing.length, 4);
  });

  it('passes over an ordinary startup', () => {
    healthyLogs.forEach(line => {
      assert.equal(
        findFunctionHostFailureLine(line),
        undefined,
        `${line} should not be read as a failure`,
      );
    });
  });
});

describe('readFunctionHostLine', () => {
  // The line the processor logged an hour and a half into a clean run, as its
  // host re-launched a language worker that had died. Nine seconds later it
  // was serving queues again - and this is the line that had the dashboard
  // calling it broken until someone restarted it.
  const hostErrorLine =
    "[2026-08-27T11:33:29.290Z] A host error has occurred during startup operation '1ad3c1ab-f72e-4016-b951-170f7de96ed3'.";

  it('reads a startup failure as one before any functions are up', () => {
    assert.deepEqual(readFunctionHostLine(hostErrorLine, false), {
      kind: 'failed',
      cause: 'the Functions host errored during startup',
    });
  });

  it('passes over that same line once the functions are up', () => {
    assert.deepEqual(readFunctionHostLine(hostErrorLine, true), {
      kind: 'unremarkable',
    });
  });

  it('reports an indexed function whatever it thought before', () => {
    // A host can fail its first startup attempt and index everything on the
    // next, so this has to be worth reporting even from a standing start.
    [false, true].forEach(hasIndexedFunctions => {
      assert.deepEqual(
        readFunctionHostLine(
          '\tProcessUploads: queueTrigger',
          hasIndexedFunctions,
        ),
        { kind: 'indexed', name: 'ProcessUploads' },
      );
    });
  });

  it('passes over an ordinary line either way', () => {
    const line =
      '[2026-08-27T11:33:38.879Z] Worker process started and initialized.';

    assert.deepEqual(readFunctionHostLine(line, false), {
      kind: 'unremarkable',
    });
    assert.deepEqual(readFunctionHostLine(line, true), {
      kind: 'unremarkable',
    });
  });
});

describe('findIndexedFunction', () => {
  it('reads the name out of each shape the host lists', () => {
    assert.equal(
      findIndexedFunction('\tCancelImports: queueTrigger'),
      'CancelImports',
    );
    assert.equal(
      findIndexedFunction('\tPrepareScheduledReleaseVersions: timerTrigger'),
      'PrepareScheduledReleaseVersions',
    );
    assert.equal(
      findIndexedFunction(
        '\tCreateDataSet: [POST] http://localhost:7074/api/CreateDataSet',
      ),
      'CreateDataSet',
    );
  });

  it('takes the header on its own as evidence of nothing', () => {
    // A header names no function, and it's the names that say the host loaded
    // the app's extensions and found something in them.
    assert.equal(findIndexedFunction('Functions:'), undefined);
  });

  it('passes over a function the host says it could not load', () => {
    assert.equal(
      findIndexedFunction(
        "[2026-08-27T11:26:07.338Z] Worker failed to load function: 'ProcessUploads' with functionId: '1404160121'.",
      ),
      undefined,
    );
  });
});

describe('findCoreToolsVersion', () => {
  it('reads the version out of the host banner', () => {
    assert.equal(findCoreToolsVersion(publisherLogs), '4.12.1');
  });

  it('returns nothing when the banner has scrolled away', () => {
    assert.equal(findCoreToolsVersion(['Functions:']), undefined);
  });
});
