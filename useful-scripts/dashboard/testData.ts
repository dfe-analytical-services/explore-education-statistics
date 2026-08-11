import { $ } from 'execa';
import fsp from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { projectRoot } from '../services';
import { startDockerServices, stopDockerServices } from './dockerManager';
import {
  startProcess,
  stopAllStartedProcesses,
  waitUntilSettled,
} from './processManager';

const MSSQL_DATA_DIR = path.join(projectRoot, 'data/ees-mssql');

// Our own on-disk backups (see backups.ts) live alongside the MSSQL data
// files in this same bind-mounted directory, so a full data-directory swap
// needs to leave this one entry alone.
const MSSQL_BACKUPS_DIR_NAME = 'backups';

async function extractToStagingDir(zipFilePath: string): Promise<string> {
  const stagingDir = await fsp.mkdtemp(
    path.join(os.tmpdir(), 'ees-mssql-import-'),
  );

  await $({ cwd: projectRoot })`unzip -o ${zipFilePath} -d ${stagingDir}`;

  return stagingDir;
}

/**
 * The team's zips wrap their contents in a top-level `ees-mssql/` folder
 * rather than extracting flat, so unzipping straight into MSSQL_DATA_DIR
 * nested a copy inside itself instead of replacing it. If extraction
 * produced exactly one top-level directory, treat its contents as the
 * real payload.
 */
async function resolveExtractedDataDir(stagingDir: string): Promise<string> {
  const entries = await fsp.readdir(stagingDir, { withFileTypes: true });

  if (entries.length === 1 && entries[0].isDirectory()) {
    return path.join(stagingDir, entries[0].name);
  }

  return stagingDir;
}

/**
 * Imports a `ees-mssql-data-xx.zip` snapshot (shared via the team's Google
 * Drive folder) by replacing the live MSSQL data directory's contents with
 * the zip's, while the container is stopped - the same "cold copy" approach
 * as manually swapping in pre-made .mdf/.ldf files.
 */
export default async function importMssqlDataZip(
  zipFilePath: string,
): Promise<void> {
  const stoppedServices = await stopAllStartedProcesses();
  await stopDockerServices(['db']);

  const stagingDir = await extractToStagingDir(zipFilePath);

  try {
    const extractedDataDir = await resolveExtractedDataDir(stagingDir);

    const existingEntries = await fsp.readdir(MSSQL_DATA_DIR);
    await Promise.all(
      existingEntries
        .filter(entry => entry !== MSSQL_BACKUPS_DIR_NAME)
        .map(entry =>
          fsp.rm(path.join(MSSQL_DATA_DIR, entry), {
            recursive: true,
            force: true,
          }),
        ),
    );

    const newEntries = await fsp.readdir(extractedDataDir);
    await Promise.all(
      newEntries.map(entry =>
        fsp.cp(
          path.join(extractedDataDir, entry),
          path.join(MSSQL_DATA_DIR, entry),
          { recursive: true },
        ),
      ),
    );
  } finally {
    await fsp.rm(stagingDir, { recursive: true, force: true });
  }

  // The mssql container runs as a non-root user unrelated to the host user
  // that owns these newly-extracted files (see the matching fix for the
  // backups directory), so they need to be readable/writable by everyone.
  // `+X` (capital) only adds execute where already executable or on
  // directories, so data files don't spuriously become executable.
  await $({ reject: false })`chmod -R a+rwX ${MSSQL_DATA_DIR}`;

  await startDockerServices(['db']);

  // Wait for each restarted service to actually reach a terminal outcome
  // (running or errored), not just for it to have been spawned, so the
  // import doesn't report as complete while services are still starting.
  await Promise.all(
    stoppedServices.map(async service => {
      await startProcess(service);
      await waitUntilSettled(service);
    }),
  );
}
