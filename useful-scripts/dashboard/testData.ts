import fsp from 'node:fs/promises';
import path from 'node:path';
import $$ from './projectExec';
import { startDockerServices, stopDockerServices } from './dockerManager';
import { ensureMssqlVolumePermissions, MSSQL_DATA_DIR } from './mssqlVolume';
import {
  getRestartOptions,
  startProcess,
  stopAllStartedProcesses,
  waitUntilSettled,
} from './processManager';

// Our own on-disk backups (see backups.ts) live alongside the MSSQL data
// files in this same bind-mounted directory, so a full data-directory swap
// needs to leave this one entry alone.
const MSSQL_BACKUPS_DIR_NAME = 'backups';

/**
 * Fails early, and legibly, if `unzip` isn't installed - otherwise the first
 * sign of it is a bare `spawn unzip ENOENT` after the databases have already
 * been stopped.
 */
async function assertUnzipAvailable(): Promise<void> {
  const { exitCode } = await $$({ reject: false })`unzip -v`;

  if (exitCode !== 0) {
    throw new Error(
      "`unzip` isn't available on this machine - install it (e.g. `sudo apt install unzip`) and try the import again",
    );
  }
}

/**
 * Extracts alongside the MSSQL data directory rather than in the system temp
 * directory, so that moving the payload into place afterwards is a rename on
 * one filesystem instead of a multi-gigabyte copy across two. That matters
 * for more than speed: the copy happens *after* the old data directory has
 * been emptied, so running out of space part-way through used to leave
 * neither the old data nor the new.
 */
async function extractToStagingDir(zipFilePath: string): Promise<string> {
  const stagingDir = await fsp.mkdtemp(
    path.join(path.dirname(MSSQL_DATA_DIR), 'ees-mssql-import-'),
  );

  await $$`unzip -o ${zipFilePath} -d ${stagingDir}`;

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
  // Before anything is stopped: there's no point taking the databases down to
  // discover the tool that does the work is missing.
  await assertUnzipAvailable();

  const { restartable: stoppedServices } = await stopAllStartedProcesses();
  await stopDockerServices(['db']);

  // The data directory is the one place it's fine to create: an import is the
  // only sanctioned way the directory comes into existence, and it's populated
  // from the zip immediately below. If it already exists (possibly root-owned
  // from a Docker-created bind mount), sort its permissions out so the
  // extraction can write into it.
  await fsp.mkdir(MSSQL_DATA_DIR, { recursive: true });
  await ensureMssqlVolumePermissions();

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

    // Renamed rather than copied: staging sits on the same filesystem as the
    // data directory, so this is a metadata operation rather than a
    // multi-gigabyte copy - and it can't run out of space half way through,
    // which at this point would be after the old data has already gone.
    const newEntries = await fsp.readdir(extractedDataDir);
    await Promise.all(
      newEntries.map(entry =>
        fsp.rename(
          path.join(extractedDataDir, entry),
          path.join(MSSQL_DATA_DIR, entry),
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
  await ensureMssqlVolumePermissions();

  await startDockerServices(['db']);

  // Wait for each restarted service to actually reach a terminal outcome
  // (running or errored), not just for it to have been spawned, so the
  // import doesn't report as complete while services are still starting.
  await Promise.all(
    stoppedServices.map(async service => {
      await startProcess(service, getRestartOptions(service));
      await waitUntilSettled(service);
    }),
  );
}
