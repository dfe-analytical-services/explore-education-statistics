const STORE_LABELS = {
  mssql: 'MSSQL (content + statistics)',
  postgres: 'Postgres (public_data)',
  azurite: 'Azurite (blob/queue/table)',
};

const dockerServicesEl = document.getElementById('docker-services');
const appServicesEl = document.getElementById('app-services');
const backupStoresEl = document.getElementById('backup-stores');

const logPanel = document.getElementById('log-panel');
const logPanelTitle = document.getElementById('log-panel-title');
const logPanelContent = document.getElementById('log-panel-content');
document
  .getElementById('log-panel-close')
  .addEventListener('click', closeLogPanel);

let currentLogSource = null;
let currentLogService = null;

function closeLogPanel() {
  if (currentLogSource) {
    currentLogSource.close();
    currentLogSource = null;
  }
  currentLogService = null;
  logPanel.classList.add('hidden');
}

function openLogPanel(name) {
  if (currentLogService === name) {
    closeLogPanel();
    return;
  }

  closeLogPanel();
  currentLogService = name;
  logPanelTitle.textContent = `Logs: ${name}`;
  logPanelContent.textContent = '';
  logPanel.classList.remove('hidden');

  currentLogSource = new EventSource(`/api/services/${name}/logs`);
  currentLogSource.onmessage = event => {
    const line = JSON.parse(event.data);
    logPanelContent.textContent += `${line}\n`;
    logPanelContent.scrollTop = logPanelContent.scrollHeight;
  };
}

async function api(path, options) {
  const res = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  const body = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(body.error || `Request to ${path} failed`);
  }
  return body;
}

function statusLabel(status) {
  return status.charAt(0).toUpperCase() + status.slice(1);
}

function renderServiceCard(service) {
  const card = document.createElement('div');
  card.className = 'card';

  const titleRow = document.createElement('div');
  titleRow.className = 'card-title-row';

  const name = document.createElement('span');
  name.className = 'card-name';
  name.textContent = service.name;
  titleRow.appendChild(name);

  const status = document.createElement('span');
  status.className = 'status-label';
  status.innerHTML = `<span class="status-dot status-${service.status}"></span>${statusLabel(service.status)}`;
  titleRow.appendChild(status);

  card.appendChild(titleRow);

  if (service.kind === 'process' && service.dependsOn?.length) {
    const meta = document.createElement('div');
    meta.className = 'card-meta';
    meta.textContent = `Needs: ${service.dependsOn.join(', ')}`;
    card.appendChild(meta);
  }

  if (service.error) {
    const err = document.createElement('div');
    err.className = 'card-meta';
    err.style.color = 'var(--red)';
    err.textContent = service.error;
    card.appendChild(err);
  }

  const actions = document.createElement('div');
  actions.className = 'card-actions';

  const isRunning = service.status === 'running';
  const isBusy = service.status === 'starting' || service.status === 'stopping';

  const toggleBtn = document.createElement('button');
  toggleBtn.className = isRunning ? 'danger' : 'primary';
  toggleBtn.textContent = isRunning ? 'Stop' : 'Start';
  toggleBtn.disabled = isBusy;
  toggleBtn.addEventListener('click', async () => {
    toggleBtn.disabled = true;
    try {
      await api(
        `/api/services/${service.name}/${isRunning ? 'stop' : 'start'}`,
        {
          method: 'POST',
        },
      );
    } catch (err) {
      window.alert(err.message);
    }
    refreshServices();
  });
  actions.appendChild(toggleBtn);

  if (service.kind === 'process') {
    const logsBtn = document.createElement('button');
    logsBtn.textContent =
      currentLogService === service.name ? 'Hide logs' : 'Logs';
    logsBtn.addEventListener('click', () => {
      openLogPanel(service.name);
      refreshServices();
    });
    actions.appendChild(logsBtn);
  }

  card.appendChild(actions);
  return card;
}

async function refreshServices() {
  const { services } = await api('/api/services');

  dockerServicesEl.replaceChildren(
    ...services.filter(s => s.kind === 'docker').map(renderServiceCard),
  );
  appServicesEl.replaceChildren(
    ...services.filter(s => s.kind === 'process').map(renderServiceCard),
  );
}

function formatBytes(bytes) {
  if (bytes < 1024) return `${bytes} B`;
  const units = ['KB', 'MB', 'GB'];
  let value = bytes;
  let unit = -1;
  do {
    value /= 1024;
    unit += 1;
  } while (value >= 1024 && unit < units.length - 1);
  return `${value.toFixed(1)} ${units[unit]}`;
}

function formatTimestamp(iso) {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return date.toLocaleString();
}

function renderBackupItem(store, backup) {
  const item = document.createElement('div');
  item.className = 'backup-item';

  const info = document.createElement('div');
  info.className = 'backup-item-info';

  const label = document.createElement('span');
  label.className = 'backup-item-label';
  label.textContent = backup.label;
  info.appendChild(label);

  const meta = document.createElement('span');
  meta.className = 'backup-item-meta';
  meta.textContent = `${formatTimestamp(backup.timestamp)} · ${formatBytes(backup.sizeBytes)}`;
  info.appendChild(meta);

  item.appendChild(info);

  const actions = document.createElement('div');
  actions.className = 'backup-item-actions';

  const restoreBtn = document.createElement('button');
  restoreBtn.textContent = 'Restore';
  restoreBtn.addEventListener('click', async () => {
    if (
      !window.confirm(
        `Restore '${backup.label}' into ${STORE_LABELS[store]}? This overwrites the current local data and cannot be undone.`,
      )
    ) {
      return;
    }
    restoreBtn.disabled = true;
    try {
      await api(`/api/backups/${store}/${backup.id}/restore`, {
        method: 'POST',
      });
      window.alert('Restore complete.');
    } catch (err) {
      window.alert(err.message);
    }
    restoreBtn.disabled = false;
  });
  actions.appendChild(restoreBtn);

  const deleteBtn = document.createElement('button');
  deleteBtn.className = 'danger';
  deleteBtn.textContent = 'Delete';
  deleteBtn.addEventListener('click', async () => {
    if (
      !window.confirm(`Delete backup '${backup.label}'? This cannot be undone.`)
    ) {
      return;
    }
    try {
      await api(`/api/backups/${store}/${backup.id}`, { method: 'DELETE' });
      refreshBackups();
    } catch (err) {
      window.alert(err.message);
    }
  });
  actions.appendChild(deleteBtn);

  item.appendChild(actions);
  return item;
}

function renderBackupStore(store, backups) {
  const panel = document.createElement('div');
  panel.className = 'backup-store';

  const heading = document.createElement('h3');
  heading.textContent = STORE_LABELS[store];
  panel.appendChild(heading);

  const createRow = document.createElement('div');
  createRow.className = 'backup-create-row';

  const labelInput = document.createElement('input');
  labelInput.type = 'text';
  labelInput.placeholder = 'Label (optional)';
  createRow.appendChild(labelInput);

  const createBtn = document.createElement('button');
  createBtn.className = 'primary';
  createBtn.textContent = 'Back up';
  createBtn.addEventListener('click', async () => {
    createBtn.disabled = true;
    createBtn.textContent = 'Backing up...';
    try {
      await api('/api/backups', {
        method: 'POST',
        body: JSON.stringify({ store, label: labelInput.value }),
      });
      labelInput.value = '';
      refreshBackups();
    } catch (err) {
      window.alert(err.message);
    }
    createBtn.disabled = false;
    createBtn.textContent = 'Back up';
  });
  createRow.appendChild(createBtn);

  panel.appendChild(createRow);

  const list = document.createElement('div');
  list.className = 'backup-list';

  if (backups.length === 0) {
    const empty = document.createElement('div');
    empty.className = 'empty-note';
    empty.textContent = 'No backups yet.';
    list.appendChild(empty);
  } else {
    backups.forEach(backup =>
      list.appendChild(renderBackupItem(store, backup)),
    );
  }

  panel.appendChild(list);
  return panel;
}

async function refreshBackups() {
  const { backups } = await api('/api/backups');
  const byStore = { mssql: [], postgres: [], azurite: [] };
  backups.forEach(backup => byStore[backup.store]?.push(backup));

  backupStoresEl.replaceChildren(
    ...Object.keys(STORE_LABELS).map(store =>
      renderBackupStore(store, byStore[store]),
    ),
  );
}

refreshServices();
refreshBackups();
setInterval(refreshServices, 3000);
