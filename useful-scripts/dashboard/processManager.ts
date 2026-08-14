import { $ } from 'execa';
import path from 'node:path';
import splitLines from 'split2';
import kill from 'tree-kill';
import {
  buildRunCommand,
  dotnetBuildLockFile,
  getServicePort,
  projectRoot,
  resolveDockerServices,
  resolveServiceDependencies,
  ServiceName,
  serviceSchemas,
  StartOptions,
} from '../services';
import createFileLock, { UnlockCallback } from '../utils/createFileLock';
import { ExecaChildProcessWithoutNullStreams } from '../utils/types';
import { startDockerServices } from './dockerManager';

export type ProcessStatus =
  'stopped' | 'starting' | 'running' | 'stopping' | 'error';

interface ManagedProcess {
  status: ProcessStatus;
  process?: ExecaChildProcessWithoutNullStreams;
  logs: string[];
  subscribers: Set<(line: string) => void>;
  error?: string;
  exited?: Promise<void>;
  settled?: Promise<void>;
  /**
   * The `PublicDataDbExists` env override the process was started with, if
   * any (i.e. admin forced to run with or without the public API), so the
   * dashboard can report what it's actually running with rather than just
   * the configured default. Only meaningful while it's started - a stale
   * value from a previous run says nothing about the next one.
   */
  publicDataDbExists?: boolean;
}

const MAX_LOG_LINES = 500;

const registry = new Map<ServiceName, ManagedProcess>();

function getOrCreate(service: ServiceName): ManagedProcess {
  let entry = registry.get(service);

  if (!entry) {
    entry = { status: 'stopped', logs: [], subscribers: new Set() };
    registry.set(service, entry);
  }

  return entry;
}

function appendLog(entry: ManagedProcess, line: string): void {
  entry.logs.push(line);

  if (entry.logs.length > MAX_LOG_LINES) {
    entry.logs.shift();
  }

  entry.subscribers.forEach(listener => listener(line));
}

export function getStatus(service: ServiceName): ProcessStatus {
  return registry.get(service)?.status ?? 'stopped';
}

export function getError(service: ServiceName): string | undefined {
  return registry.get(service)?.error;
}

export function getPublicDataDbOverride(
  service: ServiceName,
): boolean | undefined {
  return registry.get(service)?.publicDataDbExists;
}

/**
 * The options needed to start a service back up the way it was last started,
 * for callers that stop services and restore them afterwards (e.g. a backup
 * restore) - otherwise admin would come back without the `PublicDataDbExists`
 * override it was running with, silently losing the public API.
 */
export function getRestartOptions(service: ServiceName): StartOptions {
  const override = getPublicDataDbOverride(service);

  return override === undefined
    ? {}
    : { env: { PublicDataDbExists: String(override) } };
}

export function getLogs(service: ServiceName): string[] {
  return getOrCreate(service).logs;
}

export function subscribeLogs(
  service: ServiceName,
  listener: (line: string) => void,
): () => void {
  const entry = getOrCreate(service);
  entry.subscribers.add(listener);
  return () => {
    entry.subscribers.delete(listener);
  };
}

/**
 * Frees up a port from whatever's currently holding it - typically a stale
 * process left over from a dashboard restart that didn't get a chance to
 * shut its children down gracefully (e.g. after a `kill -9` on the server).
 */
async function killPortHolder(port: number): Promise<void> {
  await $({ reject: false })`fuser -k ${port}/tcp`;
}

export async function startProcess(
  service: ServiceName,
  options: StartOptions = {},
): Promise<void> {
  const schema = serviceSchemas[service];

  if (schema.type === 'docker') {
    throw new Error(
      `'${service}' is a Docker service; use dockerManager instead`,
    );
  }

  const entry = getOrCreate(service);

  if (entry.status === 'running' || entry.status === 'starting') {
    return;
  }

  const runningConflict = (schema.conflictsWith ?? []).find(conflict => {
    const conflictStatus = getStatus(conflict);
    return conflictStatus === 'running' || conflictStatus === 'starting';
  });

  if (runningConflict) {
    throw new Error(
      `Cannot start '${service}' while '${runningConflict}' is running - stop it first.`,
    );
  }

  entry.status = 'starting';
  entry.error = undefined;
  entry.logs = [];

  // Hoisted so a failure part-way through can hand the dotnet build
  // mutex back rather than holding it for the life of the dashboard.
  let unlock: UnlockCallback | undefined;

  try {
    await startDockerServices(resolveDockerServices(service, options));

    await Promise.all(
      resolveServiceDependencies(service, options).map(dependency =>
        startProcess(dependency, options),
      ),
    );

    const port = getServicePort(service);

    if (port) {
      await killPortHolder(port);
    }

    const runCommand = buildRunCommand(service, options);

    entry.publicDataDbExists =
      options.env?.PublicDataDbExists === undefined
        ? undefined
        : options.env.PublicDataDbExists === 'true';

    unlock = runCommand.lockUntilReady
      ? await createFileLock({
          lockFile: dotnetBuildLockFile,
          lockTimeout: 300_000,
          waitTimeout: 300_000,
          onExistingLock: () =>
            appendLog(
              entry,
              '[dashboard] Waiting for another build to finish...',
            ),
        })
      : undefined;

    const childProcess = $({
      cwd: path.join(projectRoot, schema.root),
      env: runCommand.env,
      shell: true,
      cleanup: false,
    })`${runCommand.command} ${runCommand.args}` as ExecaChildProcessWithoutNullStreams;

    entry.process = childProcess;

    let resolveExited: () => void;
    entry.exited = new Promise<void>(resolve => {
      resolveExited = resolve;
    });

    let resolveSettled: () => void;
    entry.settled = new Promise<void>(resolve => {
      resolveSettled = resolve;
    });

    let isReady = false;

    const markReady = async () => {
      if (isReady) {
        return;
      }

      isReady = true;
      entry.status = 'running';
      await unlock?.();
      resolveSettled();
    };

    // lockUntilReady only governs the dotnet-build mutex - readiness itself
    // should wait for checkReady wherever one's defined, regardless of that.
    if (!runCommand.checkReady) {
      markReady();
    }

    childProcess.stdout.pipe(splitLines()).on('data', (line: string) => {
      appendLog(entry, line);

      if (!isReady && runCommand.checkReady?.(line)) {
        markReady();
      }
    });

    childProcess.stderr.pipe(splitLines()).on('data', (line: string) => {
      appendLog(entry, line);
    });

    childProcess.on('exit', async (code, signal) => {
      if (!isReady) {
        await unlock?.();
      }

      const wasStopping = entry.status === 'stopping';

      entry.process = undefined;
      entry.status = wasStopping || code === 0 || signal ? 'stopped' : 'error';

      if (entry.status === 'error') {
        entry.error = `Process exited with code ${code}`;
      }

      appendLog(
        entry,
        `[dashboard] Process exited (code=${code}, signal=${signal})`,
      );

      resolveExited();
      resolveSettled();
    });
  } catch (err) {
    // Everything above runs with the service already marked 'starting', and
    // the guard at the top of this function treats 'starting' as "already on
    // its way up" - so without resetting it here a failure would leave the
    // service wedged, with every later Start silently doing nothing until the
    // dashboard was restarted.
    if (!entry.process) {
      // Never got as far as spawning, so nothing else will hand the dotnet
      // build mutex back or settle these for us.
      await unlock?.();
      entry.settled = Promise.resolve();
      entry.exited = Promise.resolve();
    }

    entry.status = 'error';
    entry.error = err instanceof Error ? err.message : String(err);
    appendLog(entry, `[dashboard] Failed to start: ${entry.error}`);

    throw err;
  }
}

/**
 * Resolves once a service started via startProcess() has reached a terminal
 * outcome - either running (ready) or exited/errored before getting there.
 * Lets callers that restart a batch of services (e.g. a test-data import)
 * wait for the restart to have actually finished, rather than just for the
 * new processes to have been spawned.
 */
export async function waitUntilSettled(service: ServiceName): Promise<void> {
  await registry.get(service)?.settled;
}

export async function stopProcess(service: ServiceName): Promise<void> {
  const entry = registry.get(service);
  const pid = entry?.process?.pid;

  if (!entry || !pid) {
    if (entry) {
      entry.status = 'stopped';
    }
    return;
  }

  entry.status = 'stopping';

  const { exited } = entry;

  await new Promise<void>(resolve => {
    kill(pid, 'SIGTERM', () => resolve());
  });

  // tree-kill's callback fires once the signal's been sent, not once the
  // process has actually exited - wait for the real exit too, otherwise
  // callers that restart the service immediately after race against this
  // process's own delayed 'exit' handler stomping on the new one's state.
  await exited;
}

export function stopAllProcesses(): void {
  registry.forEach(entry => {
    if (entry.process?.pid) {
      kill(entry.process.pid);
    }
  });
}

/**
 * Gracefully stops every app-process service that isn't already stopped,
 * for the dashboard's "Stop all" button. Leaves Docker services running,
 * same as stopping them individually would. Returns the services that were
 * running/starting beforehand, so callers that need to restore state
 * afterwards (e.g. a backup restore or test-data import) know what to
 * restart.
 */
export async function stopAllStartedProcesses(): Promise<ServiceName[]> {
  const startedServices = Array.from(registry.entries())
    .filter(([, entry]) => entry.status !== 'stopped')
    .map(([service]) => service);

  await Promise.all(startedServices.map(service => stopProcess(service)));

  return startedServices;
}
