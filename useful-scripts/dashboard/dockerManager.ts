import { ExecaChildProcess } from 'execa';
import { setTimeout as delay } from 'node:timers/promises';
import splitLines from 'split2';
import {
  allowedDockerServices,
  DockerService,
  serviceSchemas,
  ServiceName,
} from './services';
import $$ from './projectExec';

export type DockerStatus = 'running' | 'stopped' | 'unknown';

interface ComposePsEntry {
  Service: string;
  State: string;
  ExitCode?: number;
  /** The container's own name, for the `docker` calls Compose has no verb for. */
  Name?: string;
}

interface ComposeConfig {
  name: string;
  services?: Record<string, { environment?: Record<string, string> }>;
  volumes?: Record<string, { name?: string }>;
}

let composeConfigPromise: Promise<ComposeConfig> | undefined;

/**
 * The fully resolved Compose configuration - project name, per-service
 * environment, and the real (project-prefixed) names of declared volumes.
 *
 * Worth asking Compose rather than reconstructing any of it: the project name
 * comes from a `name:` key, COMPOSE_PROJECT_NAME or the directory basename
 * depending on the setup, and everything derived from it inherits that
 * ambiguity. Cached for the life of the process, so a change to
 * docker-compose.yml needs the dashboard restarting to be picked up - it's a
 * file that changes about once a year.
 */
export function getComposeConfig(): Promise<ComposeConfig> {
  composeConfigPromise ??= (async () => {
    const { stdout } = await $$`docker compose config --format json`;
    return JSON.parse(stdout) as ComposeConfig;
  })();

  return composeConfigPromise;
}

/**
 * An environment value docker-compose.yml sets for a service - database
 * credentials, mostly, so that they're configured in exactly one place rather
 * than copied into the tooling that has to authenticate with them.
 */
export async function getComposeServiceEnv(
  service: DockerService,
  key: string,
): Promise<string> {
  const config = await getComposeConfig();
  const value = config.services?.[service]?.environment?.[key];

  if (value === undefined) {
    throw new Error(
      `docker-compose.yml doesn't set '${key}' for the '${service}' service`,
    );
  }

  return value;
}

/**
 * The real name of a declared Compose volume, checked to actually exist.
 *
 * The check matters because `docker run -v` *creates* a named volume that
 * isn't there rather than failing - so a wrong name doesn't produce an error,
 * it produces an empty volume, and an operation reading from one appears to
 * succeed while backing up nothing at all.
 */
export async function getExistingVolumeName(
  declaredName: string,
): Promise<string> {
  const config = await getComposeConfig();
  const volume = config.volumes?.[declaredName]?.name;

  if (!volume) {
    throw new Error(
      `docker-compose.yml doesn't declare a '${declaredName}' volume`,
    );
  }

  const { exitCode } = await $$({
    reject: false,
  })`docker volume inspect ${volume}`;

  if (exitCode !== 0) {
    throw new Error(
      `Docker volume '${volume}' doesn't exist - start the service that owns it at least once before backing it up or restoring over it`,
    );
  }

  return volume;
}

/**
 * What `compose ps` currently knows about each Docker service, keyed by
 * service name. Services that have never been started have no container and
 * so don't appear at all.
 */
async function getComposePsEntries(): Promise<
  Map<DockerService, ComposePsEntry>
> {
  const { stdout } = await $$({
    reject: false,
  })`docker compose ps -a --format json`;

  const entries = new Map<DockerService, ComposePsEntry>();

  stdout
    .split('\n')
    .map(line => line.trim())
    .filter(Boolean)
    .forEach(line => {
      // `compose ps` output can be interleaved with unrelated warning lines
      // (e.g. while another `compose up`/`stop` is running concurrently), so
      // a line failing to parse as JSON shouldn't take down the whole
      // request - just skip it and let the next poll pick up the real state.
      try {
        const entry = JSON.parse(line) as ComposePsEntry;
        entries.set(entry.Service as DockerService, entry);
      } catch {
        // Ignore malformed lines.
      }
    });

  return entries;
}

/**
 * Get the current status of every Docker Compose service, including ones
 * that have never been started (and so won't appear in `compose ps` output).
 */
export async function getDockerStatuses(): Promise<
  Record<DockerService, DockerStatus>
> {
  const entries = await getComposePsEntries();
  const statuses = {} as Record<DockerService, DockerStatus>;

  entries.forEach((entry, service) => {
    statuses[service] = entry.State === 'running' ? 'running' : 'stopped';
  });

  return statuses;
}

/**
 * How long a container gets to stay alive before its start counts as having
 * worked, and how often it's checked in the meantime.
 *
 * `compose up -d` exits 0 once a container has been *started*, which is not
 * the same as it still being there a moment later - an entrypoint that dies
 * immediately (see `describeFailedStart`) produces a successful `up` and an
 * exited container. Long enough to catch that, and deliberately nothing to do
 * with a service becoming *ready*: Keycloak takes another 40 seconds over
 * that, and waiting for it is what the tile's status and logs are for.
 */
const STARTUP_GRACE_MS = 3_000;
const STARTUP_POLL_MS = 500;

/** Lines of an exited container's log to quote back. */
const FAILED_START_LOG_LINES = 15;

/**
 * The last failed start per Docker service, as a single line for its card.
 * Cleared whenever the service is started or stopped again, so what's on
 * screen is always about the most recent attempt.
 */
const startFailures = new Map<DockerService, string>();

/** Why this Docker service's container isn't running, if it just failed to start. */
export function getDockerStartFailure(
  service: DockerService,
): string | undefined {
  return startFailures.get(service);
}

/**
 * Container log output as something worth putting in an error message: no
 * escape codes (Keycloak and SQL Server both colour theirs), no blank lines,
 * and nothing long enough to bury the rest of the message.
 *
 * Exported for its tests.
 */
export function readableLogLines(stdout: string): string[] {
  return (
    stdout
      .split('\n')
      // eslint-disable-next-line no-control-regex
      .map(line => line.replace(/\u001B\[[0-9;]*m/g, '').trimEnd())
      .filter(Boolean)
      .slice(-FAILED_START_LOG_LINES)
      .map(line => (line.length > 200 ? `${line.slice(0, 200)}...` : line))
  );
}

export interface FailedDockerStart {
  service: DockerService;
  /** One line, for the service's card. */
  summary: string;
  /** The same thing with the log tail, for the error and the log panel. */
  detail: string;
}

/**
 * A start that Compose reported as successful but that left nothing running,
 * described from the container's exit code and the tail of its log.
 *
 * Worth the effort because the log is the only place the reason exists, and
 * the failure that prompted this reads as a single line of it: Keycloak's
 * entrypoint runs `add-user-keycloak.sh` under `set -e`, which exits 1 if a
 * previous run left an unconsumed admin user in keycloak-add-user.json - so a
 * boot interrupted part-way through wedges the container for good, and every
 * later start dies before the server is even invoked.
 *
 * Exported for its tests.
 */
export function describeExitedContainer(
  service: DockerService,
  /** The container that isn't running, or undefined if it was never created. */
  container: { exitCode?: number } | undefined,
  logLines: string[],
): FailedDockerStart {
  let exited: string;

  if (!container) {
    exited =
      "no container was created for it - check that its image builds ('docker compose build')";
  } else if (container.exitCode === undefined) {
    exited = 'its container exited (Compose reported no exit code)';
  } else {
    exited = `its container exited with code ${container.exitCode}`;
  }

  const detail = [
    `Docker service '${service}' didn't stay running - ${exited}.`,
    ...(logLines.length
      ? ['', 'The last lines of its log:', ...logLines]
      : ['', 'Its log is empty.']),
  ].join('\n');

  return {
    service,
    summary: `Failed to start: ${exited}. See its logs for the reason.`,
    detail,
  };
}

/**
 * The tail of a failed container's log, covering only the run that just
 * failed where that can be established.
 *
 * A container keeps every run's output, and the runs that matter here are
 * short - so without `--since` the tail is mostly the *previous* run shutting
 * down, with the one line explaining this failure at the very bottom. Falls
 * back to the unfiltered tail if the filter leaves nothing, since an empty
 * message would be worse than a noisy one.
 */
async function readFailedRunLog(
  service: DockerService,
  container: string | undefined,
): Promise<string[]> {
  const readLog = async (since?: string): Promise<string[]> => {
    const { stdout } = await $$({
      reject: false,
    })`docker compose logs --no-color --no-log-prefix --tail=${FAILED_START_LOG_LINES} ${since ? ['--since', since] : []} ${service}`;

    return readableLogLines(stdout);
  };

  if (container) {
    const { stdout: startedAt } = await $$({
      reject: false,
    })`docker inspect --format {{.State.StartedAt}} ${container}`;

    if (startedAt.trim()) {
      const lines = await readLog(startedAt.trim());

      if (lines.length > 0) {
        return lines;
      }
    }
  }

  return readLog();
}

async function describeFailedStart(
  service: DockerService,
  entry: ComposePsEntry | undefined,
): Promise<FailedDockerStart> {
  return describeExitedContainer(
    service,
    entry && { exitCode: entry.ExitCode },
    await readFailedRunLog(service, entry?.Name),
  );
}

/**
 * Watches the services this start actually brought up, and describes any that
 * exit within the grace period. Returns as soon as one has, rather than
 * waiting the window out, so a failure is reported at the speed it happened.
 */
async function findFailedStarts(
  services: DockerService[],
): Promise<FailedDockerStart[]> {
  if (services.length === 0) {
    return [];
  }

  const deadline = Date.now() + STARTUP_GRACE_MS;

  for (;;) {
    // eslint-disable-next-line no-await-in-loop
    const entries = await getComposePsEntries();
    const notRunning = services.filter(
      service => entries.get(service)?.State !== 'running',
    );

    if (notRunning.length > 0) {
      return Promise.all(
        notRunning.map(service =>
          describeFailedStart(service, entries.get(service)),
        ),
      );
    }

    if (Date.now() >= deadline) {
      return [];
    }

    // eslint-disable-next-line no-await-in-loop
    await delay(STARTUP_POLL_MS);
  }
}

export async function startDockerServices(
  services: DockerService[],
): Promise<void> {
  if (services.length === 0) {
    return;
  }

  services.forEach(service => startFailures.delete(service));

  // A service that was already up wasn't started by this call, so it has no
  // grace period to serve - only what `up` actually brought up can fall over
  // here, and checking the rest would hold every start behind them.
  const alreadyRunning = await getComposePsEntries();

  await $$`docker compose up -d ${services}`;

  const failures = await findFailedStarts(
    services.filter(
      service => alreadyRunning.get(service)?.State !== 'running',
    ),
  );

  if (failures.length > 0) {
    failures.forEach(({ service, summary }) =>
      startFailures.set(service, summary),
    );

    throw new Error(failures.map(({ detail }) => detail).join('\n\n'));
  }
}

export async function stopDockerServices(
  services: DockerService[],
): Promise<void> {
  if (services.length === 0) {
    return;
  }

  services.forEach(service => startFailures.delete(service));

  await $$`docker compose stop ${services}`;
}

export async function stopAllDockerServices(): Promise<void> {
  await stopDockerServices([...allowedDockerServices]);
}

export async function execInService(
  service: DockerService,
  command: string[],
): Promise<{ stdout: string; stderr: string }> {
  return $$({ reject: true })`docker compose exec -T ${service} ${command}`;
}

/**
 * Everything Compose still holds for a Docker service's container, for the
 * "download the full log" link. Unlike app processes, there's nothing to tee
 * to disk here - the container is already keeping the whole thing.
 */
export async function dockerServiceLogs(
  service: ServiceName,
): Promise<{ stdout: string }> {
  const schema = serviceSchemas[service];

  if (schema.type !== 'docker') {
    throw new Error(`'${service}' is not a Docker service`);
  }

  return $$({
    reject: false,
  })`docker compose logs --no-color --no-log-prefix ${schema.service}`;
}

/**
 * Streams a Docker service's logs to a listener: the most recent 500 lines
 * immediately, then anything new as it's written. Unlike process logs, Docker
 * logs come from the container itself, so this works whether the service is
 * running or not (a stopped container still has the logs from its last run).
 * Returns an unsubscribe function that stops the follow and frees the streams.
 */
export function subscribeDockerLogs(
  service: ServiceName,
  onLine: (line: string) => void,
): () => void {
  const schema = serviceSchemas[service];

  if (schema.type !== 'docker') {
    throw new Error(`'${service}' is not a Docker service`);
  }

  const dockerService = schema.service;

  const children: ExecaChildProcess[] = [];

  // Replay what's already been logged first, so opening the panel shows the
  // container's history rather than only new output.
  const tail = $$({
    reject: false,
  })`docker compose logs --no-log-prefix --tail=500 ${dockerService}`;
  tail.stdout?.pipe(splitLines()).on('data', onLine);
  children.push(tail);

  const follow = $$({
    reject: false,
  })`docker compose logs -f --no-log-prefix --tail=0 ${dockerService}`;
  follow.stdout?.pipe(splitLines()).on('data', onLine);
  children.push(follow);

  return () => {
    children.forEach(child => child.kill());
  };
}
