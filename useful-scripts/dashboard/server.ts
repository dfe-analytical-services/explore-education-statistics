import express from 'express';
import multer from 'multer';
import os from 'node:os';
import path from 'node:path';
import { createReadStream } from 'node:fs';
import fsp from 'node:fs/promises';
import { pipeline } from 'node:stream/promises';
import process from 'node:process';
import {
  allowedServiceNames,
  projectRootOverride,
  resolveDockerServices,
  resolveServiceDependencies,
  ServiceName,
  serviceSchemas,
  serviceUsesPublicDataDb,
  StartOptions,
} from '../services';
import errorMessage from '../utils/errorMessage';
import { getDirname } from '../utils/nodeGlobals';
import onExitSignal from '../utils/onExitSignal';
import {
  createUnifiedBackup,
  deleteUnifiedBackup,
  listUnifiedBackups,
  restoreUnifiedBackup,
} from './backups';
import {
  dockerServiceLogs,
  getDockerStatuses,
  startDockerServices,
  stopAllDockerServices,
  stopDockerServices,
  subscribeDockerLogs,
} from './dockerManager';
import { logFilePaths } from './logFiles';
import {
  ensureMssqlVolumePermissions,
  findMissingDatabaseLoginLine,
  getMssqlVolumeHealth,
} from './mssqlVolume';
import {
  getError,
  getLogs,
  getPublicDataDbOverride,
  getStatus,
  startProcess,
  stopAllProcesses,
  stopAllStartedProcesses,
  stopProcess,
  subscribeLogs,
} from './processManager';
import importMssqlDataZip from './testData';

const __dirname = getDirname(import.meta.url);
const PORT = Number(process.env.DASHBOARD_PORT ?? 4300);

// Node treats an unhandled rejection as fatal by default, which would
// otherwise take down this long-running server (and any dev processes it's
// supervising) over a single missed .catch() somewhere. Log and keep going.
process.on('unhandledRejection', err => {
  console.error('Unhandled rejection:', err);
});
process.on('uncaughtException', err => {
  console.error('Uncaught exception:', err);
});

function isServiceName(value: string): value is ServiceName {
  return (allowedServiceNames as readonly string[]).includes(value);
}

/**
 * Whether a service is (or, if it isn't started, would be) using the public
 * data database. While it's up, the env override it was actually started with
 * wins - that's the dashboard's "Start with PublicData" checkbox forcing
 * `PublicDataDbExists` - and otherwise we're back to its own appsettings.
 *
 * This is the single source of truth for the checkbox and for deciding
 * whether admin needs restarting to pick the public API up, so that neither
 * can disagree with what admin is really running with.
 */
function usesPublicDataDb(service: ServiceName): boolean {
  const status = getStatus(service);
  const isStarted = status === 'running' || status === 'starting';

  return (
    (isStarted ? getPublicDataDbOverride(service) : undefined) ??
    serviceUsesPublicDataDb(service)
  );
}

/** An error carrying the status code it should be reported with. */
class HttpError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

/**
 * Express 4 doesn't catch rejected promises from async handlers, so an
 * uncaught error here would otherwise crash the whole dashboard process
 * (Node treats an unhandled rejection as fatal by default).
 */
function asyncHandler(
  handler: (req: express.Request, res: express.Response) => Promise<void>,
) {
  return (req: express.Request, res: express.Response) => {
    handler(req, res).catch(err => {
      // Some responses stream, and are therefore already committed by the
      // time anything can fail. Trying to set a status on one of those throws
      // ERR_HTTP_HEADERS_SENT from inside the error path, replacing whatever
      // actually went wrong with a less useful error about reporting it.
      if (res.headersSent) {
        res.end();
        return;
      }

      res
        .status(err instanceof HttpError ? err.status : 500)
        .json({ error: errorMessage(err) });
    });
  };
}

/**
 * The destructive operation currently running, if any.
 *
 * Backup, restore and import all stop every service, take containers down and
 * bring them back up, and each ends by restarting whatever it stopped. Two of
 * them overlapping means one's cleanup fighting the other's setup - a restore
 * emptying the Azurite volume while a backup is reading it, or restarting
 * services the other deliberately stopped.
 */
let runningOperation: string | undefined;

/**
 * Runs a destructive operation, refusing rather than queueing if one is
 * already in progress. Refusing is the friendlier of the two: queueing a
 * second restore behind a ten-minute backup, with nothing on screen to say
 * so, is how you end up wondering why your data changed later.
 */
async function withExclusiveOperation<T>(
  name: string,
  run: () => Promise<T>,
): Promise<T> {
  if (runningOperation) {
    throw new HttpError(
      409,
      `Can't ${name} while ${runningOperation} is in progress - wait for it to finish.`,
    );
  }

  runningOperation = name;

  try {
    return await run();
  } finally {
    runningOperation = undefined;
  }
}

const app = express();
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

const uploadZip = multer({
  storage: multer.diskStorage({
    destination: os.tmpdir(),
    filename: (_req, _file, cb) =>
      cb(null, `ees-mssql-import-${Date.now()}.zip`),
  }),
  limits: { fileSize: 5 * 1024 * 1024 * 1024 },
});

interface ServiceIssue {
  id: string;
  message: string;
  fixLabel: string;
  fixEndpoint?: string;
  /**
   * Optional client-side behaviour for the fix button, for cases where the
   * "fix" is a user action rather than a server call (e.g. picking a zip to
   * import). When set, the button performs this instead of POSTing to
   * `fixEndpoint`.
   */
  action?: string;
  /**
   * The service the issue is about, so the dashboard can label the banner
   * with it (and scroll to its card).
   */
  serviceName?: string;
}

app.get(
  '/api/services',
  asyncHandler(async (_req, res) => {
    const dockerStatuses = await getDockerStatuses();

    // The mssql data directory issues are associated with the `db` service,
    // so the dashboard labels the banner with it and can scroll to its card.
    const issues: ServiceIssue[] = [];

    const mssqlHealth = await getMssqlVolumeHealth();

    if (mssqlHealth.status === 'error') {
      if (mssqlHealth.requiresImport) {
        issues.push({
          id: 'mssql-missing-data-dir',
          message: mssqlHealth.message,
          fixLabel: 'Go to import',
          action: 'open-import',
          serviceName: 'db',
        });
      } else {
        issues.push({
          id: 'mssql-volume-permissions',
          message: mssqlHealth.message,
          fixLabel: 'Fix permissions',
          fixEndpoint: '/api/mssql/fix-permissions',
          serviceName: 'db',
        });
      }
    }

    const missingLoginLine = findMissingDatabaseLoginLine(getLogs('admin'));

    if (missingLoginLine) {
      issues.push({
        id: 'mssql-missing-logins',
        message: `SQL Server is running but the required databases/logins aren't set up (${missingLoginLine}) - import a db test data zip from the 'Import DB test zip' section to populate the mssql data directory.`,
        fixLabel: 'Go to import',
        action: 'open-import',
        serviceName: 'db',
      });
    }

    const services = allowedServiceNames.map(name => {
      const schema = serviceSchemas[name];

      if (schema.type === 'docker') {
        return {
          name,
          kind: 'docker' as const,
          dockerService: schema.service,
          status: dockerStatuses[schema.service] ?? 'stopped',
          url: schema.url,
        };
      }

      const publicDataDbExists = usesPublicDataDb(name);

      // Resolved with the same PublicDataDbExists this service would actually
      // start with, rather than with no options at all - otherwise admin's
      // "Needs:" line omits publicProcessor/publicData/public-api-db until
      // it's running, which is exactly when you're reading it to find out
      // what starting it will bring up.
      const options = {
        env: { PublicDataDbExists: String(publicDataDbExists) },
      };

      return {
        name,
        kind: 'process' as const,
        processType: schema.type,
        status: getStatus(name),
        error: getError(name),
        publicDataDbExists,
        dependsOnServices: resolveServiceDependencies(name, options),
        dependsOn: resolveDockerServices(name, options),
        url: schema.url,
        group: schema.group,
        conflictsWith: schema.conflictsWith,
      };
    });

    res.json({
      services,
      issues,
      projectRootOverride,
      runningOperation: runningOperation ?? null,
    });
  }),
);

app.post(
  '/api/services/:name/start',
  asyncHandler(async (req, res) => {
    const { name } = req.params;

    if (!isServiceName(name)) {
      res.status(404).json({ error: `Unknown service '${name}'` });
      return;
    }

    const schema = serviceSchemas[name];

    if (schema.type === 'docker') {
      await startDockerServices([schema.service]);
      res.json({ ok: true });
      return;
    }

    // Mirrors the CLI's --skip-build, for the frontend tile: `frontendProd`
    // otherwise runs a full production build every time it starts.
    const options: StartOptions =
      req.body?.skipBuild === true ? { skipBuild: true } : {};

    // The dashboard's "Start with PublicData" checkbox sends this either way
    // round, so that an explicitly unticked box beats an appsettings default
    // of `PublicDataDbExists: true` - whatever the checkbox showed is what
    // admin ends up running with. .NET treats an env var as higher-priority
    // than appsettings, so the override is all it takes.
    if (name === 'admin' && typeof req.body?.startPublicData === 'boolean') {
      const { startPublicData } = req.body;

      await startProcess('admin', {
        ...options,
        env: { PublicDataDbExists: String(startPublicData) },
      });

      if (startPublicData) {
        await startProcess('publicData', options);
      }

      res.json({ ok: true });
      return;
    }

    if (name === 'publicData') {
      // Admin talks to the public API via the public data database, so if
      // it's running but was started without it (PublicDataDbExists: false),
      // restart it with the override so the two actually work together.
      const adminStatus = getStatus('admin');

      if (
        (adminStatus === 'running' || adminStatus === 'starting') &&
        !usesPublicDataDb('admin')
      ) {
        await stopProcess('admin');
        await startProcess('admin', {
          ...options,
          env: { PublicDataDbExists: 'true' },
        });
      }

      await startProcess('publicData', options);
      res.json({ ok: true });
      return;
    }

    await startProcess(name, options);
    res.json({ ok: true });
  }),
);

app.post(
  '/api/services/:name/stop',
  asyncHandler(async (req, res) => {
    const { name } = req.params;

    if (!isServiceName(name)) {
      res.status(404).json({ error: `Unknown service '${name}'` });
      return;
    }

    const schema = serviceSchemas[name];

    if (schema.type === 'docker') {
      await stopDockerServices([schema.service]);
    } else {
      await stopProcess(name);
    }

    res.json({ ok: true });
  }),
);

app.post(
  '/api/services/stop-all',
  asyncHandler(async (_req, res) => {
    const { forced } = await stopAllStartedProcesses();
    await stopAllDockerServices();

    // Reported back rather than swallowed: a service that had to be killed
    // didn't get to shut down cleanly, which is worth knowing before you take
    // a backup of what it left behind.
    res.json({ ok: true, forced });
  }),
);

app.post(
  '/api/mssql/fix-permissions',
  asyncHandler(async (_req, res) => {
    // The db container is only stopped so a fresh start re-triggers SQL
    // Server's bootstrap once the directory's usable again.
    await stopDockerServices(['db']);

    try {
      await ensureMssqlVolumePermissions();
    } catch (err) {
      await startDockerServices(['db']);
      throw err;
    }

    await startDockerServices(['db']);

    res.json({ ok: true });
  }),
);

app.get('/api/services/:name/logs', (req, res) => {
  const { name } = req.params;

  if (!isServiceName(name)) {
    res.status(404).json({ error: `Unknown service '${name}'` });
    return;
  }

  res.setHeader('Content-Type', 'text/event-stream');
  res.setHeader('Cache-Control', 'no-cache');
  res.setHeader('Connection', 'keep-alive');
  res.flushHeaders();

  if (serviceSchemas[name].type === 'docker') {
    const unsubscribe = subscribeDockerLogs(name, line => {
      res.write(`data: ${JSON.stringify(line)}\n\n`);
    });
    req.on('close', unsubscribe);
    return;
  }

  getLogs(name).forEach(line => {
    res.write(`data: ${JSON.stringify(line)}\n\n`);
  });

  const unsubscribe = subscribeLogs(name, line => {
    res.write(`data: ${JSON.stringify(line)}\n\n`);
  });

  req.on('close', () => unsubscribe());
});

/**
 * The whole of a service's log, as a download - the panel only holds the most
 * recent lines, and a startup failure usually scrolls out of that.
 *
 * For app processes this is what the dashboard teed to disk as it ran (the
 * rotated-out file first, so it reads chronologically). Docker services keep
 * their own logs in the container, so those come straight from Compose.
 */
app.get(
  '/api/services/:name/log-file',
  asyncHandler(async (req, res) => {
    const { name } = req.params;

    if (!isServiceName(name)) {
      res.status(404).json({ error: `Unknown service '${name}'` });
      return;
    }

    res.setHeader('Content-Type', 'text/plain; charset=utf-8');
    res.setHeader('Content-Disposition', `attachment; filename="${name}.log"`);

    if (serviceSchemas[name].type === 'docker') {
      const { stdout } = await dockerServiceLogs(name);
      res.send(stdout);
      return;
    }

    const files = logFilePaths(name);

    if (files.length === 0) {
      res.send(
        `No log file for '${name}' yet - it hasn't been started from the dashboard.\n`,
      );
      return;
    }

    // eslint-disable-next-line no-restricted-syntax
    for await (const file of files) {
      await pipeline(createReadStream(file), res, { end: false });
    }

    res.end();
  }),
);

app.get(
  '/api/backups',
  asyncHandler(async (_req, res) => {
    const backups = await listUnifiedBackups();
    res.json({ backups });
  }),
);

app.post(
  '/api/backups',
  asyncHandler(async (req, res) => {
    const { label } = req.body ?? {};

    const backup = await withExclusiveOperation('take a backup', () =>
      createUnifiedBackup(typeof label === 'string' ? label : ''),
    );
    res.json({ backup });
  }),
);

app.post(
  '/api/backups/:id/restore',
  asyncHandler(async (req, res) => {
    const { id } = req.params;

    await withExclusiveOperation('restore a backup', () =>
      restoreUnifiedBackup(id),
    );
    res.json({ ok: true });
  }),
);

app.delete(
  '/api/backups/:id',
  asyncHandler(async (req, res) => {
    const { id } = req.params;

    // Exclusive too, despite only deleting files: the backup being deleted
    // could be the one a restore is midway through reading.
    await withExclusiveOperation('delete a backup', () =>
      deleteUnifiedBackup(id),
    );
    res.json({ ok: true });
  }),
);

app.post(
  '/api/mssql-data/import',
  uploadZip.single('file'),
  asyncHandler(async (req, res) => {
    if (!req.file) {
      res.status(400).json({ error: 'No file uploaded' });
      return;
    }

    const { path: uploadPath } = req.file;

    try {
      await withExclusiveOperation('import a test data zip', () =>
        importMssqlDataZip(uploadPath),
      );
      res.json({ ok: true });
    } finally {
      await fsp.unlink(uploadPath).catch(() => {});
    }
  }),
);

// Must be defined last, and with 4 args, for Express to treat it as an
// error handler - catches errors from middleware (e.g. multer) that run
// before asyncHandler gets a chance to wrap the route.
// eslint-disable-next-line no-unused-vars
app.use(
  (
    err: unknown,
    _req: express.Request,
    res: express.Response,
    _: express.NextFunction,
  ) => {
    console.error('Unhandled request error:', err);

    // Same reasoning as asyncHandler: a streamed response is already
    // committed, and setting a status on one throws from inside the handler
    // that exists to report the original problem.
    if (res.headersSent) {
      res.end();
      return;
    }

    res
      .status(err instanceof HttpError ? err.status : 500)
      .json({ error: errorMessage(err) });
  },
);

const server = app.listen(PORT, '127.0.0.1', () => {
  console.info(`Dashboard running at http://localhost:${PORT}`);
});

onExitSignal(() => {
  stopAllProcesses();
  server.close();
});
