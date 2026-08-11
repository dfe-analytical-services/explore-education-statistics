import express from 'express';
import multer from 'multer';
import os from 'node:os';
import path from 'node:path';
import fsp from 'node:fs/promises';
import process from 'node:process';
import {
  allowedServiceNames,
  resolveDockerServices,
  resolveServiceDependencies,
  ServiceName,
  serviceSchemas,
} from '../services';
import { getDirname } from '../utils/nodeGlobals';
import onExitSignal from '../utils/onExitSignal';
import {
  createUnifiedBackup,
  deleteUnifiedBackup,
  listUnifiedBackups,
  restoreUnifiedBackup,
} from './backups';
import {
  getDockerStatuses,
  startDockerServices,
  stopAllDockerServices,
  stopDockerServices,
} from './dockerManager';
import {
  getError,
  getLogs,
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

function errorMessage(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
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
      res.status(500).json({ error: errorMessage(err) });
    });
  };
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

app.get(
  '/api/services',
  asyncHandler(async (_req, res) => {
    const dockerStatuses = await getDockerStatuses();

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

      return {
        name,
        kind: 'process' as const,
        processType: schema.type,
        status: getStatus(name),
        error: getError(name),
        dependsOnServices: resolveServiceDependencies(name),
        dependsOn: resolveDockerServices(name, {}),
        url: schema.url,
        group: schema.group,
        conflictsWith: schema.conflictsWith,
      };
    });

    res.json({ services });
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
    } else {
      await startProcess(name);
    }

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
    await stopAllStartedProcesses();
    await stopAllDockerServices();
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

  getLogs(name).forEach(line => {
    res.write(`data: ${JSON.stringify(line)}\n\n`);
  });

  const unsubscribe = subscribeLogs(name, line => {
    res.write(`data: ${JSON.stringify(line)}\n\n`);
  });

  req.on('close', () => unsubscribe());
});

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

    const backup = await createUnifiedBackup(
      typeof label === 'string' ? label : '',
    );
    res.json({ backup });
  }),
);

app.post(
  '/api/backups/:id/restore',
  asyncHandler(async (req, res) => {
    const { id } = req.params;

    await restoreUnifiedBackup(id);
    res.json({ ok: true });
  }),
);

app.delete(
  '/api/backups/:id',
  asyncHandler(async (req, res) => {
    const { id } = req.params;

    await deleteUnifiedBackup(id);
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

    try {
      await importMssqlDataZip(req.file.path);
      res.json({ ok: true });
    } finally {
      await fsp.unlink(req.file.path).catch(() => {});
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
    res.status(500).json({ error: errorMessage(err) });
  },
);

const server = app.listen(PORT, '127.0.0.1', () => {
  console.info(`Dashboard running at http://localhost:${PORT}`);
});

onExitSignal(() => {
  stopAllProcesses();
  server.close();
});
