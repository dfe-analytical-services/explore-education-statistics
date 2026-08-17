import { $ } from 'execa';
import { projectRoot } from '../services';

/**
 * execa bound to the project root, for the `docker`/`docker compose` calls
 * that have to run from the directory holding `docker-compose.yml`.
 *
 * Shared rather than re-declared per module because with EES_PROJECT_ROOT set
 * `projectRoot` is a *different* checkout from the one running this code, and
 * a call that forgot the `cwd` would quietly act on the wrong container stack.
 */
const projectExec = $({ cwd: projectRoot });

export default projectExec;
