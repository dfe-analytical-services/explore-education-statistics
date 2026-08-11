import { $ } from 'execa';
import fs from 'node:fs';
import fsp from 'node:fs/promises';
import path from 'node:path';
import { pipeline } from 'node:stream/promises';
import { projectRoot } from '../services';
import { ExecaChildProcessWithoutNullStreams } from '../utils/types';
import { startDockerServices, stopDockerServices } from './dockerManager';

export type BackupStore = 'mssql' | 'postgres' | 'azurite';

export interface BackupInfo {
  id: string;
  store: BackupStore;
  label: string;
  /** ISO timestamp the backup was taken at. */
  timestamp: string;
  sizeBytes: number;
  files: string[];
}

const MSSQL_DATABASES = ['content', 'statistics'] as const;
const MSSQL_SA_PASSWORD = 'Your_Password123';
const MSSQL_HOST_BACKUP_DIR = path.join(projectRoot, 'data/ees-mssql/backups');
const MSSQL_CONTAINER_BACKUP_DIR = '/var/opt/mssql/data/backups';

const POSTGRES_DB = 'public_data';
const POSTGRES_PASSWORD = 'password';
const POSTGRES_BACKUP_DIR = path.join(projectRoot, 'data/backups/postgres');

const AZURITE_VOLUME = 'explore-education-statistics_data-storage-data';
const AZURITE_BACKUP_DIR = path.join(projectRoot, 'data/backups/azurite');

const $$ = $({ cwd: projectRoot });

function slugifyLabel(label: string): string {
  const slug = label
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

  return slug || 'unlabelled';
}

function timestampSlug(date: Date): string {
  return date.toISOString().replace(/[:.]/g, '-');
}

function parseTimestampSlug(slug: string): string {
  const match = slug.match(
    /^(\d{4}-\d{2}-\d{2}T\d{2})-(\d{2})-(\d{2})-(\d{3})Z$/,
  );

  if (!match) {
    return slug;
  }

  const [, datePart, minutes, seconds, ms] = match;
  return `${datePart}:${minutes}:${seconds}.${ms}Z`;
}

function buildId(label: string, date: Date = new Date()): string {
  return `${timestampSlug(date)}__${slugifyLabel(label)}`;
}

function parseId(id: string): { timestamp: string; label: string } {
  const [ts, ...labelParts] = id.split('__');
  return { timestamp: parseTimestampSlug(ts), label: labelParts.join('__') };
}

async function sumFileSizes(files: string[]): Promise<number> {
  const stats = await Promise.all(files.map(file => fsp.stat(file)));
  return stats.reduce((total, stat) => total + stat.size, 0);
}

async function execSqlcmd(query: string): Promise<void> {
  await startDockerServices(['db']);

  // -b: exit with a non-zero code on SQL errors, so failures (e.g. permission
  // errors writing the backup file) surface as thrown errors instead of
  // being silently swallowed by sqlcmd's default success exit code.
  await $$`docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd -C -b -S localhost -U sa -P ${MSSQL_SA_PASSWORD} -Q ${query}`;
}

async function listSingleFileBackups(
  store: Extract<BackupStore, 'postgres' | 'azurite'>,
  dir: string,
  extension: string,
): Promise<BackupInfo[]> {
  if (!fs.existsSync(dir)) {
    return [];
  }

  const entries = await fsp.readdir(dir);

  return Promise.all(
    entries
      .filter(entry => entry.endsWith(extension))
      .map(async entry => {
        const id = entry.slice(0, -extension.length);
        const { timestamp, label } = parseId(id);
        const filePath = path.join(dir, entry);

        return {
          id,
          store,
          label,
          timestamp,
          sizeBytes: await sumFileSizes([filePath]),
          files: [filePath],
        };
      }),
  );
}

async function listMssqlBackups(): Promise<BackupInfo[]> {
  if (!fs.existsSync(MSSQL_HOST_BACKUP_DIR)) {
    return [];
  }

  const entries = await fsp.readdir(MSSQL_HOST_BACKUP_DIR);
  const groups = new Map<string, string[]>();

  entries.forEach(entry => {
    if (!entry.endsWith('.bak')) {
      return;
    }

    const [ts, labelSlug, dbNameWithExt] = entry.split('__');

    if (!ts || !labelSlug || !dbNameWithExt) {
      return;
    }

    const dbName = dbNameWithExt.replace(/\.bak$/, '');

    if (!MSSQL_DATABASES.includes(dbName as (typeof MSSQL_DATABASES)[number])) {
      return;
    }

    const id = `${ts}__${labelSlug}`;
    const files = groups.get(id) ?? [];
    files.push(path.join(MSSQL_HOST_BACKUP_DIR, entry));
    groups.set(id, files);
  });

  return Promise.all(
    Array.from(groups.entries()).map(async ([id, files]) => {
      const { timestamp, label } = parseId(id);

      return {
        id,
        store: 'mssql' as const,
        label,
        timestamp,
        sizeBytes: await sumFileSizes(files),
        files,
      };
    }),
  );
}

export async function listBackups(store?: BackupStore): Promise<BackupInfo[]> {
  const stores: BackupStore[] = store
    ? [store]
    : ['mssql', 'postgres', 'azurite'];

  const results = await Promise.all(
    stores.map(async s => {
      switch (s) {
        case 'mssql':
          return listMssqlBackups();
        case 'postgres':
          return listSingleFileBackups(
            'postgres',
            POSTGRES_BACKUP_DIR,
            '.dump',
          );
        case 'azurite':
          return listSingleFileBackups(
            'azurite',
            AZURITE_BACKUP_DIR,
            '.tar.gz',
          );
        default:
          return [];
      }
    }),
  );

  return results.flat().sort((a, b) => b.timestamp.localeCompare(a.timestamp));
}

async function ensureMssqlBackupDir(): Promise<void> {
  await fsp.mkdir(MSSQL_HOST_BACKUP_DIR, { recursive: true });

  // The mssql container runs as a non-root user unrelated to the host user
  // that owns this bind-mounted directory, so it needs to be world-writable
  // for BACKUP/RESTORE DATABASE to be able to read and write files here.
  await fsp.chmod(MSSQL_HOST_BACKUP_DIR, 0o777);
}

async function createMssqlBackup(id: string): Promise<BackupInfo> {
  await ensureMssqlBackupDir();
  const { label, timestamp } = parseId(id);

  const files = await Promise.all(
    MSSQL_DATABASES.map(async db => {
      const fileName = `${id}__${db}.bak`;
      await execSqlcmd(
        `BACKUP DATABASE [${db}] TO DISK = N'${MSSQL_CONTAINER_BACKUP_DIR}/${fileName}' WITH INIT`,
      );
      return path.join(MSSQL_HOST_BACKUP_DIR, fileName);
    }),
  );

  return {
    id,
    store: 'mssql',
    label,
    timestamp,
    sizeBytes: await sumFileSizes(files),
    files,
  };
}

async function createPostgresBackup(id: string): Promise<BackupInfo> {
  await fsp.mkdir(POSTGRES_BACKUP_DIR, { recursive: true });
  await startDockerServices(['public-api-db']);

  const { label, timestamp } = parseId(id);
  const filePath = path.join(POSTGRES_BACKUP_DIR, `${id}.dump`);

  const child = $$({
    env: { PGPASSWORD: POSTGRES_PASSWORD },
  })`docker compose exec -T public-api-db pg_dump -U postgres -Fc ${POSTGRES_DB}` as ExecaChildProcessWithoutNullStreams;

  await Promise.all([
    pipeline(child.stdout, fs.createWriteStream(filePath)),
    child,
  ]);

  return {
    id,
    store: 'postgres',
    label,
    timestamp,
    sizeBytes: await sumFileSizes([filePath]),
    files: [filePath],
  };
}

async function createAzuriteBackup(id: string): Promise<BackupInfo> {
  await fsp.mkdir(AZURITE_BACKUP_DIR, { recursive: true });
  const { label, timestamp } = parseId(id);
  const fileName = `${id}.tar.gz`;
  const filePath = path.join(AZURITE_BACKUP_DIR, fileName);

  await stopDockerServices(['data-storage']);

  try {
    await $$`docker run --rm -v ${AZURITE_VOLUME}:/source:ro -v ${AZURITE_BACKUP_DIR}:/backup alpine tar czf /backup/${fileName} -C /source .`;
  } finally {
    await startDockerServices(['data-storage']);
  }

  return {
    id,
    store: 'azurite',
    label,
    timestamp,
    sizeBytes: await sumFileSizes([filePath]),
    files: [filePath],
  };
}

export async function createBackup(
  store: BackupStore,
  label: string,
): Promise<BackupInfo> {
  const id = buildId(label);

  switch (store) {
    case 'mssql':
      return createMssqlBackup(id);
    case 'postgres':
      return createPostgresBackup(id);
    case 'azurite':
      return createAzuriteBackup(id);
    default:
      throw new Error(`Unknown backup store '${store}'`);
  }
}

async function restoreMssqlBackup(id: string): Promise<void> {
  const backups = await listMssqlBackups();
  const backup = backups.find(b => b.id === id);

  if (!backup) {
    throw new Error(`MSSQL backup '${id}' not found`);
  }

  await Promise.all(
    backup.files.map(file => {
      const fileName = path.basename(file);
      const dbName = fileName.split('__')[2]?.replace(/\.bak$/, '');

      return execSqlcmd(
        `ALTER DATABASE [${dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ` +
          `RESTORE DATABASE [${dbName}] FROM DISK = N'${MSSQL_CONTAINER_BACKUP_DIR}/${fileName}' WITH REPLACE; ` +
          `ALTER DATABASE [${dbName}] SET MULTI_USER;`,
      );
    }),
  );
}

async function restorePostgresBackup(id: string): Promise<void> {
  const backups = await listSingleFileBackups(
    'postgres',
    POSTGRES_BACKUP_DIR,
    '.dump',
  );
  const backup = backups.find(b => b.id === id);

  if (!backup) {
    throw new Error(`Postgres backup '${id}' not found`);
  }

  await startDockerServices(['public-api-db']);

  const child = $$({
    env: { PGPASSWORD: POSTGRES_PASSWORD },
    stdin: 'pipe',
    reject: false,
  })`docker compose exec -T public-api-db pg_restore -U postgres -d ${POSTGRES_DB} --clean --if-exists --no-owner` as ExecaChildProcessWithoutNullStreams;

  await Promise.all([
    pipeline(fs.createReadStream(backup.files[0]), child.stdin),
    child,
  ]);
}

async function restoreAzuriteBackup(id: string): Promise<void> {
  const backups = await listSingleFileBackups(
    'azurite',
    AZURITE_BACKUP_DIR,
    '.tar.gz',
  );
  const backup = backups.find(b => b.id === id);

  if (!backup) {
    throw new Error(`Azurite backup '${id}' not found`);
  }

  const fileName = path.basename(backup.files[0]);

  await stopDockerServices(['data-storage']);

  try {
    await $$`docker run --rm -v ${AZURITE_VOLUME}:/target -v ${AZURITE_BACKUP_DIR}:/backup alpine sh -c ${`find /target -mindepth 1 -delete && tar xzf /backup/${fileName} -C /target`}`;
  } finally {
    await startDockerServices(['data-storage']);
  }
}

export async function restoreBackup(
  store: BackupStore,
  id: string,
): Promise<void> {
  switch (store) {
    case 'mssql':
      return restoreMssqlBackup(id);
    case 'postgres':
      return restorePostgresBackup(id);
    case 'azurite':
      return restoreAzuriteBackup(id);
    default:
      throw new Error(`Unknown backup store '${store}'`);
  }
}

export async function deleteBackup(
  store: BackupStore,
  id: string,
): Promise<void> {
  const backups = await listBackups(store);
  const backup = backups.find(b => b.id === id);

  if (!backup) {
    throw new Error(`Backup '${id}' not found in ${store}`);
  }

  await Promise.all(backup.files.map(file => fsp.unlink(file)));
}
