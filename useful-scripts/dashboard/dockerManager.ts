import { ExecaChildProcess } from 'execa';
import splitLines from 'split2';
import {
  allowedDockerServices,
  DockerService,
  serviceSchemas,
  ServiceName,
} from '../services';
import $$ from '../utils/projectExec';

export type DockerStatus = 'running' | 'stopped' | 'unknown';

interface ComposePsEntry {
  Service: string;
  State: string;
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
