import crypto from 'node:crypto';
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
import createFileLock from '../utils/createFileLock';
import { ExecaChildProcessWithoutNullStreams } from '../utils/types';
import { startDockerServices } from './dockerManager';
import looksLikeException from './exceptionDetector';

export type ProcessStatus =
  'stopped' | 'starting' | 'running' | 'stopping' | 'error';

interface ManagedProcess {
  status: ProcessStatus;
  process?: ExecaChildProcessWithoutNullStreams;
  logs: string[];
  subscribers: Set<(line: string) => void>;
  error?: string;
}

export interface Alert {
  id: string;
  service: ServiceName;
  line: string;
  timestamp: string;
}

const MAX_LOG_LINES = 500;
const MAX_ALERTS = 200;

const registry = new Map<ServiceName, ManagedProcess>();
const alerts: Alert[] = [];
const alertSubscribers = new Set<(alert: Alert) => void>();

function getOrCreate(service: ServiceName): ManagedProcess {
  let entry = registry.get(service);

  if (!entry) {
    entry = { status: 'stopped', logs: [], subscribers: new Set() };
    registry.set(service, entry);
  }

  return entry;
}

function appendLog(
  service: ServiceName,
  entry: ManagedProcess,
  line: string,
): void {
  entry.logs.push(line);

  if (entry.logs.length > MAX_LOG_LINES) {
    entry.logs.shift();
  }

  entry.subscribers.forEach(listener => listener(line));

  if (looksLikeException(line)) {
    const alert: Alert = {
      id: crypto.randomUUID(),
      service,
      line,
      timestamp: new Date().toISOString(),
    };

    alerts.push(alert);

    if (alerts.length > MAX_ALERTS) {
      alerts.shift();
    }

    alertSubscribers.forEach(listener => listener(alert));
  }
}

export function getAlerts(): Alert[] {
  return alerts;
}

export function subscribeAlerts(listener: (alert: Alert) => void): () => void {
  alertSubscribers.add(listener);
  return () => {
    alertSubscribers.delete(listener);
  };
}

export function getStatus(service: ServiceName): ProcessStatus {
  return registry.get(service)?.status ?? 'stopped';
}

export function getError(service: ServiceName): string | undefined {
  return registry.get(service)?.error;
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

  await startDockerServices(resolveDockerServices(service, options));

  await Promise.all(
    resolveServiceDependencies(service).map(dependency =>
      startProcess(dependency, options),
    ),
  );

  const port = getServicePort(service);

  if (port) {
    await killPortHolder(port);
  }

  const runCommand = buildRunCommand(service, options);

  const unlock = runCommand.lockUntilReady
    ? await createFileLock({
        lockFile: dotnetBuildLockFile,
        lockTimeout: 300_000,
        waitTimeout: 300_000,
        onExistingLock: () =>
          appendLog(
            service,
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

  let isReady = false;

  const markReady = async () => {
    if (isReady) {
      return;
    }

    isReady = true;
    entry.status = 'running';
    await unlock?.();
  };

  if (!runCommand.lockUntilReady) {
    markReady();
  }

  childProcess.stdout.pipe(splitLines()).on('data', (line: string) => {
    appendLog(service, entry, line);

    if (!isReady && runCommand.checkReady?.(line)) {
      markReady();
    }
  });

  childProcess.stderr.pipe(splitLines()).on('data', (line: string) => {
    appendLog(service, entry, line);
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
      service,
      entry,
      `[dashboard] Process exited (code=${code}, signal=${signal})`,
    );
  });
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

  await new Promise<void>(resolve => {
    kill(pid, 'SIGTERM', () => resolve());
  });
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
 * same as stopping them individually would.
 */
export async function stopAllStartedProcesses(): Promise<void> {
  const startedServices = Array.from(registry.entries())
    .filter(([, entry]) => entry.status !== 'stopped')
    .map(([service]) => service);

  await Promise.all(startedServices.map(service => stopProcess(service)));
}
