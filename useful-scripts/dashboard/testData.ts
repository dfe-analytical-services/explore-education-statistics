import { $ } from 'execa';
import path from 'node:path';
import { projectRoot } from '../services';
import { startDockerServices, stopDockerServices } from './dockerManager';
import { stopAllStartedProcesses } from './processManager';

const MSSQL_DATA_DIR = path.join(projectRoot, 'data/ees-mssql');

/**
 * Imports a `ees-mssql-data-xx.zip` snapshot (shared via the team's Google
 * Drive folder) by extracting it directly over the live MSSQL data
 * directory while the container is stopped - the same "cold copy" approach
 * as manually swapping in pre-made .mdf/.ldf files. Only files present in
 * the zip are overwritten; anything else already in the directory (e.g.
 * system databases) is left untouched, since these zips typically contain
 * just the content/statistics databases, not the whole SQL Server instance.
 */
export default async function importMssqlDataZip(
  zipFilePath: string,
): Promise<void> {
  await stopAllStartedProcesses();
  await stopDockerServices(['db']);

  await $({
    cwd: projectRoot,
  })`unzip -o ${zipFilePath} -d ${MSSQL_DATA_DIR}`;

  // The mssql container runs as a non-root user unrelated to the host user
  // that owns these newly-extracted files (see the matching fix for the
  // backups directory), so they need to be readable/writable by everyone.
  // `+X` (capital) only adds execute where already executable or on
  // directories, so data files don't spuriously become executable.
  await $({ reject: false })`chmod -R a+rwX ${MSSQL_DATA_DIR}`;

  await startDockerServices(['db']);
}
