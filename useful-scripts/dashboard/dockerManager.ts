import { $ } from 'execa';
import { DockerService, projectRoot } from '../services';

export type DockerStatus = 'running' | 'stopped' | 'unknown';

interface ComposePsEntry {
  Service: string;
  State: string;
}

const $$ = $({ cwd: projectRoot });

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

export async function execInService(
  service: DockerService,
  command: string[],
): Promise<{ stdout: string; stderr: string }> {
  return $$({ reject: true })`docker compose exec -T ${service} ${command}`;
}
