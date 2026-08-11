import { $ } from 'execa';
import path from 'node:path';
import splitLines from 'split2';
import kill from 'tree-kill';
import {
  buildRunCommand,
  dotnetBuildLockFile,
  projectRoot,
  resolveDockerServices,
  ServiceName,
  serviceSchemas,
  StartOptions,
} from '../services';
import createFileLock from '../utils/createFileLock';
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

  entry.status = 'starting';
  entry.error = undefined;
  entry.logs = [];

  await startDockerServices(resolveDockerServices(service, options));

  const runCommand = buildRunCommand(service, options);

  const unlock = runCommand.lockUntilReady
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
