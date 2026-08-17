import { $ } from 'execa';
import fs from 'node:fs';
import fsp from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { projectRoot } from './services';
import errorMessage from './errorMessage';
import $$ from './projectExec';

/**
 * The `db` container's bind-mounted data directory. Exported because the
 * test-data import writes into the same directory, and two independently
 * declared copies of this path silently drifting apart would point the health
 * check and the import at different places.
 */
export const MSSQL_DATA_DIR = path.join(projectRoot, 'data/ees-mssql');

/**
 * The `db` (MSSQL) container runs as a dedicated non-root user that has
 * nothing to do with the host account running the dashboard, so the
 * bind-mounted data directory has to be writable by that UID regardless of
 * who happens to own it on the host.
 */
const MSSQL_UID = 10001;
const MSSQL_GID = 0;

/**
 * Base image for the `db` compose service (see docker/mssql-server/Dockerfile).
 * Used as a throwaway root shell to repair the host directory when the host
 * user isn't its owner (and so can't chmod/chown it directly) - the Docker
 * daemon runs as root, so this works whatever the host ownership is.
 */
const MSSQL_IMAGE = 'mcr.microsoft.com/mssql/server:2019-latest';

/**
 * Log signatures of SQL Server's bootstrap dying because it can't write its
 * system data files - the first symptom to show up in the `db` container's
 * logs when data/ees-mssql has the wrong ownership/permissions.
 */
const BOOTSTRAP_FAILURE_PATTERNS = [
  /BootstrapSystemDataDirectories\(\) failure/,
  /Setup FAILED copying system data file/,
  /HRESULT 0x80070005/,
] as const;

/**
 * Log signatures of an app failing to log in to SQL Server because the
 * required logins/databases don't exist - i.e. a freshly bootstrapped mssql
 * data directory that hasn't been populated from a test-data zip. Surfaced
 * as a hint that the user needs to import one (the mssql volume itself is
 * fine; there's just no data in it).
 */
const LOGIN_FAILURE_PATTERNS = [
  /Login failed for user /,
  /Error Number:18456/,
] as const;

/**
 * Whether the given app-process log lines show a service failing to connect
 * to SQL Server because the database logins aren't set up. The admin app's
 * logs (which already capture its full startup failure) are the obvious
 * source.
 */
export function findMissingDatabaseLoginLine(
  lines: readonly string[],
): string | undefined {
  return lines
    .find(line => LOGIN_FAILURE_PATTERNS.some(pattern => pattern.test(line)))
    ?.trim();
}

export type MssqlVolumeHealth =
  | { status: 'ok' }
  | {
      status: 'error';
      message: string;
      /**
       * Whether the fix is to import a db test data zip (which creates and
       * populates the data directory) rather than repairing the existing one
       * - the directory should never be created empty outside an import.
       */
      requiresImport: boolean;
    };

// The dashboard UI polls /api/services every few seconds, and a healthy
// check doesn't need to be recomputed every time (nor should it need to
// shell out to Docker each poll), so the result is cached briefly.
let cachedHealth: MssqlVolumeHealth | undefined;
let cachedAt = 0;
const HEALTH_TTL_MS = 5_000;

function modeString(mode: number): string {
  // eslint-disable-next-line no-bitwise
  return (mode & 0o777).toString(8).padStart(3, '0');
}

/**
 * Whether the given uid/gid could create entries inside a directory with the
 * given mode, applying the same owner/group/other resolution the kernel uses.
 * The mssql container user is uid 10001 in group 0, so e.g. a root-owned
 * directory only needs group-write to be usable, while a user-owned one needs
 * to be world-writable.
 */
// eslint-disable-next-line no-bitwise -- permission bits are the whole point
function canWriteIn(uid: number, gid: number, mode: number): boolean {
  let bits: number;

  // eslint-disable-next-line no-bitwise
  if (uid === MSSQL_UID) {
    // eslint-disable-next-line no-bitwise
    bits = (mode & 0o700) >> 6;
  } else if (gid === MSSQL_GID) {
    // eslint-disable-next-line no-bitwise
    bits = (mode & 0o070) >> 3;
  } else {
    // eslint-disable-next-line no-bitwise
    bits = mode & 0o007;
  }

  // Writing a new data file requires the write bit; reaching anything already
  // inside the directory requires the execute bit too.
  // eslint-disable-next-line no-bitwise
  return (bits & 0o2) !== 0 && (bits & 0o1) !== 0;
}

async function findBootstrapFailureLogLine(): Promise<string | undefined> {
  const { stdout } = await $$({
    reject: false,
  })`docker compose logs --tail 200 db`;

  return stdout
    .split('\n')
    .find(line =>
      BOOTSTRAP_FAILURE_PATTERNS.some(pattern => pattern.test(line)),
    )
    ?.trim();
}

/**
 * Checks whether the mssql data directory is (and would stay) usable by the
 * `db` container, flagging the most common cause of it failing to start:
 * a root-owned, non-world-writable directory. The container's own logs are
 * only consulted to corroborate - a stale bootstrap failure in old logs
 * after the directory's been fixed is not itself a problem.
 */
export async function getMssqlVolumeHealth(): Promise<MssqlVolumeHealth> {
  const now = Date.now();

  if (cachedHealth && now - cachedAt < HEALTH_TTL_MS) {
    return cachedHealth;
  }

  const problems: string[] = [];
  let requiresImport = false;

  let stat: fs.Stats | undefined;

  try {
    stat = await fsp.stat(MSSQL_DATA_DIR);
  } catch (err) {
    if ((err as NodeJS.ErrnoException).code !== 'ENOENT') {
      problems.push(`couldn't inspect ${MSSQL_DATA_DIR}: ${errorMessage(err)}`);
    }
  }

  if (stat) {
    if (!canWriteIn(stat.uid, stat.gid, stat.mode)) {
      problems.push(
        `${MSSQL_DATA_DIR} (uid ${stat.uid}:${stat.gid}, mode ${modeString(stat.mode)}) isn't writable by the mssql container user (uid ${MSSQL_UID})`,
      );
    }
  } else {
    requiresImport = true;
    problems.push(
      `${MSSQL_DATA_DIR} doesn't exist - the mssql data directory should only ever be created by importing a db test data zip`,
    );
  }

  if (problems.length > 0) {
    const bootstrapLine = await findBootstrapFailureLogLine();

    if (bootstrapLine) {
      problems.push(
        `the db container's logs show SQL Server failing to start ("${bootstrapLine}")`,
      );
    }
  }

  cachedHealth =
    problems.length === 0
      ? { status: 'ok' }
      : { status: 'error', requiresImport, message: `${problems.join('. ')}.` };

  cachedAt = now;
  return cachedHealth;
}

/**
 * Makes the mssql data directory usable by the `db` container by fixing its
 * ownership/permissions - directly when the host user owns it, otherwise
 * through a throwaway root container over the same bind mount. Only repairs
 * an existing directory: it must never create one, because the data directory
 * should only ever come into existence populated by a db test data zip import
 * (see testData.ts), and an empty dir would just make SQL Server bootstrap a
 * fresh instance with no logins/databases.
 */
export async function ensureMssqlVolumePermissions(): Promise<void> {
  let stat: fs.Stats;

  try {
    stat = await fsp.stat(MSSQL_DATA_DIR);
  } catch (err) {
    throw new Error(
      `${MSSQL_DATA_DIR} doesn't exist - import a db test data zip to create and populate it (${errorMessage(err)})`,
    );
  }

  const hostUid = process.getuid?.() ?? -1;

  if (stat.uid === hostUid) {
    await $({ reject: false })`chmod -R a+rwX ${MSSQL_DATA_DIR}`;
  }

  const afterStat = await fsp.stat(MSSQL_DATA_DIR);

  if (!canWriteIn(afterStat.uid, afterStat.gid, afterStat.mode)) {
    // The host user can't change ownership of a directory they don't own, so
    // do it from a throwaway root container over the same bind mount - the
    // Docker daemon runs as root, so this works whatever the host ownership
    // is. Uses `--entrypoint` (rather than a shell) so execa's argument
    // splitting can't mangle any embedded quoting.
    await $$`docker run --rm --user root --entrypoint chmod -v ${MSSQL_DATA_DIR}:/var/opt/mssql/data ${MSSQL_IMAGE} -R a+rwX /var/opt/mssql/data`;
    await $$`docker run --rm --user root --entrypoint chown -v ${MSSQL_DATA_DIR}:/var/opt/mssql/data ${MSSQL_IMAGE} -R 10001:0 /var/opt/mssql/data`;
  }

  // The directory has changed underneath us - don't keep serving the stale
  // (error) health that got us here.
  cachedHealth = undefined;
}
