import { $ } from 'execa';
import path from 'node:path';
import process from 'node:process';
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
} from './services';
import createFileLock, { UnlockCallback } from '../utils/createFileLock';
import errorMessage from './errorMessage';
import { ExecaChildProcessWithoutNullStreams } from '../utils/types';
import { startDockerServices } from './dockerManager';
import { ServiceLogFile } from './logFiles';

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
  /**
   * Identifies the in-flight `startProcess` call that currently owns this
   * entry, or undefined once nothing is starting it.
   *
   * A start spends most of its life in awaits - Docker, dependencies, the
   * dotnet build lock - before it has a process to signal, so a stop arriving
   * during that window has nothing to kill and used to be a no-op that the
   * start then blithely undid by spawning anyway. Clearing the token cancels
   * it instead. It's a token rather than a boolean flag so that a *new* start
   * also supersedes the old one: the outgoing call sees a token that is no
   * longer its own and unwinds without touching state the new one now owns.
   */
  startToken?: number;
  /**
   * The service's on-disk log for this run. The in-memory `logs` buffer is
   * capped at MAX_LOG_LINES, which a service can blow through during startup
   * alone, so the thing you actually want to read when a start goes wrong is
   * often already gone by the time you open the panel.
   */
  logFile?: ServiceLogFile;
}

const MAX_LOG_LINES = 500;

/**
 * How long a service gets to honour SIGTERM before it's killed outright.
 *
 * Generous, because a Functions host draining in-flight work legitimately
 * takes a while and killing it early is worse than waiting. Bounded at all
 * because every destructive operation stops services first, so an unbounded
 * wait means one stubborn process hangs a backup or an import indefinitely.
 */
const STOP_GRACE_MS = 60_000;

/**
 * How long a service may hold the dotnet build mutex before it's taken back.
 *
 * The mutex is held until the service is *ready*, not until it has finished
 * building, because there's no reliable marker for the end of the build in
 * `dotnet run` output. That's fine when a service either starts or dies, and
 * useless when one builds successfully and then hangs - admin unable to reach
 * the database being the usual way - because nothing else can build for as
 * long as it sits there. Long enough for a cold build plus startup, short
 * enough that one wedged service doesn't take the rest of the dashboard with
 * it.
 */
const BUILD_MUTEX_MAX_HOLD_MS = 600_000;

const registry = new Map<ServiceName, ManagedProcess>();

let nextStartToken = 0;

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

  entry.logFile?.write(line);
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
 *
 * Best-effort on every platform: nothing here rejects, so a missing tool or a
 * port nobody holds just leaves the port alone and lets the service that's
 * about to start fail on the bind itself, which says far more about what's
 * wrong than a failure here would.
 *
 * Whatever it kills is reported through `log`, because "whatever's currently
 * holding it" is not always something stale - a service being debugged from
 * an IDE holds the same port, and having that killed from under you with no
 * explanation anywhere is baffling.
 */
async function killPortHolder(
  port: number,
  log: (line: string) => void,
): Promise<void> {
  const reportKilled = (pids: string[]): void => {
    if (pids.length > 0) {
      log(
        `[dashboard] Freed port ${port} by killing pid ${pids.join(', ')} - if you were debugging this service from an IDE, that's what just stopped`,
      );
    }
  };

  switch (process.platform) {
    // No `fuser`, so look the holders up and kill them by pid. `netstat`
    // rather than `Get-NetTCPConnection` to avoid paying PowerShell's startup
    // cost on every single service start.
    case 'win32': {
      const { stdout } = await $({ reject: false })`netstat -ano -p tcp`;

      const pids = new Set(
        stdout
          .split(/\r?\n/)
          .map(line => line.trim().split(/\s+/))
          // Columns are [proto, local address, foreign address, state, pid].
          // Matched on the local address alone: the state column is localised
          // on non-English Windows, and the foreign address would match
          // whatever is *connected* to the port rather than holding it.
          .filter(
            ([proto, local, , , pid]) =>
              proto === 'TCP' &&
              local?.endsWith(`:${port}`) &&
              pid &&
              pid !== '0',
          )
          .map(columns => columns[4]),
      );

      await Promise.all(
        Array.from(pids, pid => $({ reject: false })`taskkill /F /PID ${pid}`),
      );

      reportKilled(Array.from(pids));
      break;
    }
    // macOS ships `fuser`, but without the `-k` flag, so it also has to go via
    // pids. Restricted to listeners because `lsof` would otherwise report
    // every client connected to the port too.
    case 'darwin': {
      const { stdout } = await $({
        reject: false,
      })`lsof -ti ${`tcp:${port}`} -sTCP:LISTEN`;

      const pids = stdout.split('\n').filter(Boolean);

      if (pids.length) {
        await $({ reject: false })`kill -9 ${pids}`;
      }

      reportKilled(pids);
      break;
    }
    default: {
      // `fuser` prints the pids it matched to stdout and everything else to
      // stderr, so the one call both kills and says what it killed.
      const { stdout } = await $({ reject: false })`fuser -k ${port}/tcp`;

      reportKilled(stdout.trim().split(/\s+/).filter(Boolean));
      break;
    }
  }
}

/**
 * Signals a service's whole process tree, synchronously.
 *
 * Services are spawned detached, so the shell execa starts leads a process
 * group that everything below it inherits - `dotnet run`, the Functions host
 * it launches, and that host's own worker. Signalling the negated pid reaches
 * the entire group in one syscall.
 *
 * Being a syscall rather than a walk is what makes it usable from the exit
 * path. `tree-kill` enumerates descendants by shelling out to `ps` before it
 * signals anything, so it needs the event loop to turn at least once - which
 * a handler running as the process exits never gets. It stays as the fallback
 * for a pid that leads no group, where there is time to walk the tree.
 *
 * Windows has no process groups at all, so there is nothing to signal by
 * negated pid and `tree-kill` (which shells out to `taskkill /T`) is the only
 * option. Branching on the platform rather than letting the call fail and
 * fall through keeps that deliberate: whether a negative pid comes back as
 * ESRCH or some other errno is a libuv implementation detail, and relying on
 * it would have made Windows work only by accident. The consequence is that
 * the shutdown path can't stop services on Windows - it needs the event loop
 * `taskkill` requires - which is the same as it was before services were
 * spawned detached.
 */
function killProcessTree(
  pid: number,
  signal: NodeJS.Signals = 'SIGTERM',
): void {
  if (process.platform === 'win32') {
    kill(pid, signal);
    return;
  }

  try {
    process.kill(-pid, signal);
  } catch {
    // Any failure here means the group signal didn't land - typically ESRCH
    // for a pid that leads no group. Walking the tree is slower but works
    // from anywhere that isn't the exit path.
    kill(pid, signal);
  }
}

/**
 * Tail of the queue serialising dotnet builds *within this process*.
 *
 * The file lock these builds also take only coordinates with other processes
 * (a `start:dashboard` run): cross-process-lock hands the lock straight over
 * when the requesting pid matches the one holding it, and every service here is
 * started from the dashboard's single Node process - so between two services
 * the dashboard starts, it's a no-op. Anything that starts several at once
 * (restarting everything after a backup, say) would otherwise build them
 * concurrently, and concurrent builds corrupt each other through the shared
 * `src/artifacts` output tree - see https://github.com/dotnet/sdk/issues/9487.
 */
let buildQueueTail: Promise<void> = Promise.resolve();

/**
 * Waits for any in-progress build to finish, resolving to the function that
 * lets the next one start. Callers must always call it, including on failure,
 * or every later build queues behind them forever.
 */
function enterBuildQueue(): Promise<() => void> {
  const previous = buildQueueTail;

  let leave!: () => void;
  buildQueueTail = new Promise<void>(resolve => {
    leave = resolve;
  });

  return previous.then(() => leave);
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

  // Opened here rather than at spawn time so the file also captures what
  // happened *before* the process existed - waiting on the build lock, a
  // dependency failing, the start being cancelled - which is exactly the part
  // that's hard to diagnose after the fact.
  entry.logFile?.close();
  entry.logFile = new ServiceLogFile(service);

  nextStartToken += 1;
  const startToken = nextStartToken;
  entry.startToken = startToken;

  // Hoisted so a failure part-way through can hand the dotnet build
  // mutex back rather than holding it for the life of the dashboard. Clears
  // itself when called, so the release sites below can't double-release.
  let releaseBuildMutex: UnlockCallback | undefined;
  let buildMutexTimer: NodeJS.Timeout | undefined;

  /** Hands the build mutex back, cancelling the deadline that would have. */
  const releaseBuildMutexNow = async (): Promise<void> => {
    if (buildMutexTimer) {
      clearTimeout(buildMutexTimer);
      buildMutexTimer = undefined;
    }

    await releaseBuildMutex?.();
  };

  /**
   * Whether this start has been cancelled or superseded, unwinding it if so.
   * Called after every await that happens before the process is spawned - the
   * whole point is to notice a stop that arrived while there was nothing yet
   * to signal, and to notice it before a process exists to leak.
   *
   * Deliberately touches no entry state beyond the build mutex: whoever
   * invalidated the token owns the entry now.
   */
  const cancelled = async (): Promise<boolean> => {
    if (entry.startToken === startToken) {
      return false;
    }

    await releaseBuildMutexNow();
    appendLog(entry, '[dashboard] Start cancelled');

    return true;
  };

  try {
    await startDockerServices(resolveDockerServices(service, options));

    if (await cancelled()) {
      return;
    }

    await Promise.all(
      resolveServiceDependencies(service, options).map(dependency =>
        startProcess(dependency, options),
      ),
    );

    if (await cancelled()) {
      return;
    }

    const port = getServicePort(service);

    if (port) {
      await killPortHolder(port, line => appendLog(entry, line));
    }

    if (await cancelled()) {
      return;
    }

    const runCommand = buildRunCommand(service, options);

    entry.publicDataDbExists =
      options.env?.PublicDataDbExists === undefined
        ? undefined
        : options.env.PublicDataDbExists === 'true';

    if (runCommand.lockUntilReady) {
      const leaveBuildQueue = await enterBuildQueue();

      try {
        // Generous timeouts because this is held until the service is ready,
        // not just built, and a queue of cold dotnet builds takes a while.
        // `lockTimeout` expiring would hand the lock to another process
        // mid-build, which is the very thing it's here to prevent - it stays
        // well clear of BUILD_MUTEX_MAX_HOLD_MS so that the deadline below,
        // which releases the lock properly, always gets there first.
        const unlockFile = await createFileLock({
          lockFile: dotnetBuildLockFile,
          lockTimeout: 1_800_000,
          waitTimeout: 1_800_000,
          onExistingLock: () =>
            appendLog(
              entry,
              '[dashboard] Waiting for another build to finish...',
            ),
        });

        releaseBuildMutex = async () => {
          releaseBuildMutex = undefined;
          await unlockFile();
          leaveBuildQueue();
        };
      } catch (err) {
        leaveBuildQueue();
        throw err;
      }
    }

    // The most important of these checks: waiting for the build lock is the
    // longest a start ever sits without a process, so it's where a stop is
    // most likely to land.
    if (await cancelled()) {
      return;
    }

    const childProcess = $({
      cwd: path.join(projectRoot, schema.root),
      env: runCommand.env,
      shell: true,
      cleanup: false,
      // Makes the spawned shell a process group leader, so everything it goes
      // on to start - `dotnet run`, the Functions host, that host's worker -
      // shares one group that `killProcessTree` can signal in one go. Not on
      // Windows, which has no process groups to lead: there it only detaches
      // the child into a console window of its own, which buys nothing and
      // litters the desktop with one per service.
      detached: process.platform !== 'win32',
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

    // Nothing releases the mutex between here and the service becoming ready
    // or exiting, so a service that does neither would hold it indefinitely.
    // Unref'd because this is a backstop, not a reason to keep the process
    // alive.
    if (releaseBuildMutex) {
      buildMutexTimer = setTimeout(() => {
        appendLog(
          entry,
          `[dashboard] Still not ready ${BUILD_MUTEX_MAX_HOLD_MS / 60_000} minutes after starting - releasing the build lock so other services aren't blocked behind it`,
        );

        releaseBuildMutexNow().catch(err =>
          appendLog(
            entry,
            `[dashboard] Failed to release the build lock: ${errorMessage(err)}`,
          ),
        );
      }, BUILD_MUTEX_MAX_HOLD_MS);

      buildMutexTimer.unref();
    }

    let isReady = false;
    let hasExited = false;

    const markReady = async () => {
      // stdout that was buffered when the process died still arrives after
      // the exit handler has run, so a ready line in it would otherwise
      // resurrect a service that has already stopped.
      if (isReady || hasExited) {
        return;
      }

      isReady = true;
      entry.status = 'running';
      await releaseBuildMutexNow();
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
      hasExited = true;

      if (!isReady) {
        await releaseBuildMutexNow();
      }

      const wasStopping = entry.status === 'stopping';

      entry.process = undefined;
      entry.startToken = undefined;
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
    entry.error = errorMessage(err);

    if (entry.process) {
      // The process spawned before whatever went wrong here, so it is still
      // out there holding its port and its database connections. Keep
      // tracking it - with the error attached for the UI to show - rather
      // than marking it 'error': that status means "nothing is running", so
      // the pid would be forgotten, no Stop button would be offered, and
      // (being detached) it would go on to outlive the dashboard entirely.
      // Its own exit handler still owns the mutex and the settled/exited
      // promises, so nothing is unwound here.
      entry.status = 'running';
      appendLog(
        entry,
        `[dashboard] Start failed after the process had spawned - it is still running: ${entry.error}`,
      );

      throw err;
    }

    // Everything above runs with the service already marked 'starting', and
    // the guard at the top of this function treats 'starting' as "already on
    // its way up" - so without resetting it here a failure would leave the
    // service wedged, with every later Start silently doing nothing until the
    // dashboard was restarted.
    //
    // Never got as far as spawning, so nothing else will hand the dotnet
    // build mutex back or settle these for us.
    await releaseBuildMutexNow();
    entry.settled = Promise.resolve();
    entry.exited = Promise.resolve();
    entry.startToken = undefined;
    entry.status = 'error';
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

/**
 * Resolves true if `promise` settles within `ms`, false if it times out.
 *
 * The timer is cleared on the happy path so a service that stops promptly
 * doesn't leave a minute-long handle behind holding the event loop open.
 */
function settledWithin(
  promise: Promise<unknown> | undefined,
  ms: number,
): Promise<boolean> {
  return new Promise(resolve => {
    const timer = setTimeout(() => resolve(false), ms);

    Promise.resolve(promise).then(() => {
      clearTimeout(timer);
      resolve(true);
    });
  });
}

/**
 * Stops a service and waits for it to actually be gone.
 *
 * Returns whether it had to be forced. A service that ignores SIGTERM is
 * killed outright after {@link STOP_GRACE_MS} rather than waited on forever,
 * because backup, restore and import all stop services before they touch any
 * data - so an unbounded wait here hangs the whole operation, with the UI
 * left sitting on "Backing up..." and no indication why.
 */
export async function stopProcess(service: ServiceName): Promise<boolean> {
  const entry = registry.get(service);

  if (!entry) {
    return false;
  }

  const pid = entry.process?.pid;

  if (!pid) {
    // Nothing has spawned. If a start is in flight it's parked in one of its
    // awaits - Docker, dependencies, the build lock - and would go on to
    // spawn the very process this stop is meant to prevent. Dropping the
    // token is what makes it notice and unwind instead.
    if (entry.startToken !== undefined) {
      appendLog(
        entry,
        '[dashboard] Stop requested while starting - cancelling',
      );
    }

    entry.startToken = undefined;
    entry.status = 'stopped';
    entry.settled = Promise.resolve();
    entry.exited = Promise.resolve();

    return false;
  }

  entry.status = 'stopping';

  const { exited } = entry;

  killProcessTree(pid, 'SIGTERM');

  // Signalling only says the process was told to go; wait for it to actually
  // exit, otherwise callers that restart the service immediately after race
  // against this process's own delayed 'exit' handler stomping on the new
  // one's state.
  if (await settledWithin(exited, STOP_GRACE_MS)) {
    return false;
  }

  appendLog(
    entry,
    `[dashboard] Still running ${STOP_GRACE_MS / 1000}s after SIGTERM - sending SIGKILL`,
  );

  killProcessTree(pid, 'SIGKILL');
  await exited;

  return true;
}

/**
 * Stops every service without waiting for any of them, for the shutdown path.
 *
 * Nothing here may be asynchronous. This runs from `signal-exit`, by which
 * point the process is already on its way out and the event loop won't turn
 * again - so any work deferred to a later tick is simply dropped, and the
 * services it was meant to stop outlive the dashboard as orphans.
 */
export function stopAllProcesses(): void {
  registry.forEach(entry => {
    if (entry.process?.pid) {
      killProcessTree(entry.process.pid);
    }

    // Synchronous, like everything else on this path, so the tail of each log
    // is flushed rather than lost with the process.
    entry.logFile?.close();
  });
}

export interface StopAllResult {
  /**
   * The services that were genuinely up beforehand, for callers that put
   * things back afterwards (a backup restore, a test-data import).
   */
  restartable: ServiceName[];
  /** The services that ignored SIGTERM and had to be killed. */
  forced: ServiceName[];
}

/**
 * Gracefully stops every app-process service, for the dashboard's "Stop all"
 * button and for the operations that have to quiesce things before touching
 * data. Leaves Docker services running, same as stopping them individually
 * would.
 *
 * What gets stopped and what gets restarted are deliberately different sets.
 * Anything holding a process is stopped, whatever its status - a start that
 * failed after spawning leaves one running under an 'error' status, still on
 * its port and still connected to the databases a backup is about to read.
 * Only what was actually running or starting is worth restoring though;
 * resurrecting a service that had already crashed isn't restoring state, it's
 * inventing it.
 */
export async function stopAllStartedProcesses(): Promise<StopAllResult> {
  const entries = Array.from(registry.entries());

  const restartable = entries
    .filter(
      ([, entry]) => entry.status === 'running' || entry.status === 'starting',
    )
    .map(([service]) => service);

  const toStop = entries
    .filter(([, entry]) => entry.process?.pid || entry.status !== 'stopped')
    .map(([service]) => service);

  const wasForced = await Promise.all(
    toStop.map(service => stopProcess(service)),
  );

  return {
    restartable,
    forced: toStop.filter((_, index) => wasForced[index]),
  };
}
