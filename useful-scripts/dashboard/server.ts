import express from 'express';
import path from 'node:path';
import process from 'node:process';
import {
  allowedServiceNames,
  resolveDockerServices,
  ServiceName,
  serviceSchemas,
} from '../services';
import { getDirname } from '../utils/nodeGlobals';
import onExitSignal from '../utils/onExitSignal';
import {
  BackupStore,
  createBackup,
  deleteBackup,
  listBackups,
  restoreBackup,
} from './backups';
import {
  getDockerStatuses,
  startDockerServices,
  stopDockerServices,
} from './dockerManager';
import {
  getError,
  getLogs,
  getStatus,
  startProcess,
  stopAllProcesses,
  stopProcess,
  subscribeLogs,
} from './processManager';

const __dirname = getDirname(import.meta.url);
const PORT = Number(process.env.DASHBOARD_PORT ?? 4300);
const BACKUP_STORES: BackupStore[] = ['mssql', 'postgres', 'azurite'];

function isServiceName(value: string): value is ServiceName {
  return (allowedServiceNames as readonly string[]).includes(value);
}

function isBackupStore(value: string): value is BackupStore {
  return (BACKUP_STORES as string[]).includes(value);
}

function errorMessage(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
}

const app = express();
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

app.get('/api/services', async (_req, res) => {
  const dockerStatuses = await getDockerStatuses();

  const services = allowedServiceNames.map(name => {
    const schema = serviceSchemas[name];

    if (schema.type === 'docker') {
      return {
        name,
        kind: 'docker' as const,
        dockerService: schema.service,
        status: dockerStatuses[schema.service] ?? 'stopped',
      };
    }

    return {
      name,
      kind: 'process' as const,
      processType: schema.type,
      status: getStatus(name),
      error: getError(name),
      dependsOn: resolveDockerServices(name, {}),
    };
  });

  res.json({ services });
});

app.post('/api/services/:name/start', async (req, res) => {
  const { name } = req.params;

  if (!isServiceName(name)) {
    res.status(404).json({ error: `Unknown service '${name}'` });
    return;
  }

  try {
    const schema = serviceSchemas[name];

    if (schema.type === 'docker') {
      await startDockerServices([schema.service]);
    } else {
      await startProcess(name);
    }

    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ error: errorMessage(err) });
  }
});

app.post('/api/services/:name/stop', async (req, res) => {
  const { name } = req.params;

  if (!isServiceName(name)) {
    res.status(404).json({ error: `Unknown service '${name}'` });
    return;
  }

  try {
    const schema = serviceSchemas[name];

    if (schema.type === 'docker') {
      await stopDockerServices([schema.service]);
    } else {
      await stopProcess(name);
    }

    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ error: errorMessage(err) });
  }
});

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

app.get('/api/backups', async (req, res) => {
  const { store } = req.query;

  if (typeof store === 'string' && !isBackupStore(store)) {
    res.status(400).json({ error: `Unknown backup store '${store}'` });
    return;
  }

  const backups = await listBackups(
    typeof store === 'string' ? store : undefined,
  );

  res.json({ backups });
});

app.post('/api/backups', async (req, res) => {
  const { store, label } = req.body ?? {};

  if (typeof store !== 'string' || !isBackupStore(store)) {
    res.status(400).json({ error: `Unknown backup store '${store}'` });
    return;
  }

  try {
    const backup = await createBackup(
      store,
      typeof label === 'string' ? label : '',
    );
    res.json({ backup });
  } catch (err) {
    res.status(500).json({ error: errorMessage(err) });
  }
});

app.post('/api/backups/:store/:id/restore', async (req, res) => {
  const { store, id } = req.params;

  if (!isBackupStore(store)) {
    res.status(400).json({ error: `Unknown backup store '${store}'` });
    return;
  }

  try {
    await restoreBackup(store, id);
    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ error: errorMessage(err) });
  }
});

app.delete('/api/backups/:store/:id', async (req, res) => {
  const { store, id } = req.params;

  if (!isBackupStore(store)) {
    res.status(400).json({ error: `Unknown backup store '${store}'` });
    return;
  }

  try {
    await deleteBackup(store, id);
    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ error: errorMessage(err) });
  }
});

const server = app.listen(PORT, '127.0.0.1', () => {
  console.info(`Dashboard running at http://localhost:${PORT}`);
});

onExitSignal(() => {
  stopAllProcesses();
  server.close();
});
