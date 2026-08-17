import { ExecaChildProcess } from 'execa';
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
 * Get the current status of every Docker Compose service, including ones
 * that have never been started (and so won't appear in `compose ps` output).
 */
export async function getDockerStatuses(): Promise<
  Record<DockerService, DockerStatus>
> {
  const { stdout } = await $$({
    reject: false,
  })`docker compose ps -a --format json`;

  const statuses = {} as Record<DockerService, DockerStatus>;

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
        statuses[entry.Service as DockerService] =
          entry.State === 'running' ? 'running' : 'stopped';
      } catch {
        // Ignore malformed lines.
      }
    });

  return statuses;
}

export async function startDockerServices(
  services: DockerService[],
): Promise<void> {
  if (services.length === 0) {
    return;
  }

  await $$`docker compose up -d ${services}`;
}

export async function stopDockerServices(
  services: DockerService[],
): Promise<void> {
  if (services.length === 0) {
    return;
  }

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
