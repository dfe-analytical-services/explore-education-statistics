const dockerServicesEl = document.getElementById('docker-services');
const appServicesEl = document.getElementById('app-services');
const backupPanelEl = document.getElementById('backup-panel');
const stopAllBtn = document.getElementById('stop-all-btn');
const issuesBannerEl = document.getElementById('issues-banner');
const projectRootBannerEl = document.getElementById('project-root-banner');

stopAllBtn.addEventListener('click', async () => {
  if (!window.confirm('Stop all app processes AND all Docker services?')) {
    return;
  }
  stopAllBtn.disabled = true;
  stopAllBtn.textContent = 'Stopping...';
  try {
    await api('/api/services/stop-all', { method: 'POST' });
  } catch (err) {
    window.alert(err.message);
  }
  stopAllBtn.disabled = false;
  stopAllBtn.textContent = 'Stop everything';
  refreshServices();
});

const mssqlImportInput = document.getElementById('mssql-import-input');
const mssqlImportBtn = document.getElementById('mssql-import-btn');

mssqlImportBtn.addEventListener('click', async () => {
  const file = mssqlImportInput.files[0];
  if (!file) {
    window.alert('Choose a .zip file first.');
    return;
  }
  if (
    !window.confirm(
      `Import '${file.name}'? This stops the db and any running app processes, overwrites matching files in data/ees-mssql, then restarts the db. This cannot be undone.`,
    )
  ) {
    return;
  }

  mssqlImportBtn.disabled = true;
  mssqlImportBtn.textContent = 'Importing...';
  try {
    const formData = new FormData();
    formData.append('file', file);
    const res = await fetch('/api/mssql-data/import', {
      method: 'POST',
      body: formData,
    });
    const body = await res.json().catch(() => ({}));
    if (!res.ok) {
      throw new Error(body.error || 'Import failed');
    }
    window.alert('Import complete.');
    mssqlImportInput.value = '';
  } catch (err) {
    window.alert(err.message);
  }
  mssqlImportBtn.disabled = false;
  mssqlImportBtn.textContent = 'Import';
  refreshServices();
});

const logPanel = document.getElementById('log-panel');
const logPanelBackdrop = document.getElementById('log-panel-backdrop');
const logPanelTitle = document.getElementById('log-panel-title');
const logPanelContent = document.getElementById('log-panel-content');
document
  .getElementById('log-panel-close')
  .addEventListener('click', closeLogPanel);
logPanelBackdrop.addEventListener('click', closeLogPanel);

let currentLogSource = null;
let currentLogService = null;
let pendingLogLines = [];
let logFlushHandle = null;

/**
 * Appends every line buffered since the last flush in one go.
 *
 * Opening a Docker service's logs replays up to 500 lines as an immediate
 * burst of SSE events, so writing each line as it arrives meant re-serialising
 * the whole panel (`textContent +=` is O(n^2) over the total text) and forcing
 * a synchronous reflow per line by reading `scrollHeight`. Batching keeps both
 * costs to once per tick no matter how fast lines arrive.
 *
 * Scheduled on a timer rather than `requestAnimationFrame` because rAF doesn't
 * fire while the tab is hidden - lines would queue up unrendered until you
 * switched back to it.
 */
function flushLogLines() {
  logFlushHandle = null;

  if (pendingLogLines.length === 0) {
    return;
  }

  // Measure before appending, and only stick to the bottom if we were already
  // there - otherwise a burst yanks the view away while you're reading back.
  const pinnedToBottom =
    logPanelContent.scrollHeight - logPanelContent.scrollTop <=
    logPanelContent.clientHeight + 20;

  logPanelContent.appendChild(
    document.createTextNode(`${pendingLogLines.join('\n')}\n`),
  );
  pendingLogLines = [];

  if (pinnedToBottom) {
    logPanelContent.scrollTop = logPanelContent.scrollHeight;
  }
}

function closeLogPanel() {
  if (currentLogSource) {
    currentLogSource.close();
    currentLogSource = null;
  }
  if (logFlushHandle !== null) {
    clearTimeout(logFlushHandle);
    logFlushHandle = null;
  }
  pendingLogLines = [];
  currentLogService = null;
  logPanel.classList.add('hidden');
  logPanelBackdrop.classList.add('hidden');
}

function openLogPanel(name) {
  if (currentLogService === name) {
    closeLogPanel();
    return;
  }

  closeLogPanel();
  currentLogService = name;
  logPanelTitle.textContent = `Logs: ${displayName(name)}`;
  logPanelContent.textContent = '';
  logPanel.classList.remove('hidden');
  logPanelBackdrop.classList.remove('hidden');

  currentLogSource = new EventSource(`/api/services/${name}/logs`);
  currentLogSource.onmessage = event => {
    pendingLogLines.push(JSON.parse(event.data));

    if (logFlushHandle === null) {
      logFlushHandle = setTimeout(flushLogLines, 16);
    }
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

function displayName(name) {
  return name.charAt(0).toUpperCase() + name.slice(1);
}

function needsText(service) {
  const needs = [
    ...(service.dependsOnServices ?? []),
    ...(service.dependsOn ?? []),
  ];
  return needs.length ? `Needs: ${needs.join(', ')}` : '';
}

function appendCardMeta(card, text, isError) {
  if (!text) {
    return;
  }
  const meta = document.createElement('div');
  meta.className = 'card-meta';
  if (isError) {
    meta.style.color = 'var(--red)';
  }
  meta.textContent = text;
  card.appendChild(meta);
}

function appendOpenLink(card, service) {
  if (service.status !== 'running' || !service.url) {
    return;
  }
  const link = document.createElement('a');
  link.className = 'open-link';
  link.href = service.url;
  link.target = '_blank';
  link.rel = 'noopener noreferrer';
  link.textContent = 'Open ↗';
  card.appendChild(link);
}

function renderServiceCard(service) {
  const card = document.createElement('div');
  card.className = 'card';
  card.id = `service-card-${service.name}`;

  const titleRow = document.createElement('div');
  titleRow.className = 'card-title-row';

  const name = document.createElement('span');
  name.className = 'card-name';
  name.textContent = displayName(service.name);
  titleRow.appendChild(name);

  const status = document.createElement('span');
  status.className = 'status-label';
  status.innerHTML = `<span class="status-dot status-${service.status}"></span>${statusLabel(service.status)}`;
  titleRow.appendChild(status);

  card.appendChild(titleRow);

  if (service.kind === 'process') {
    appendCardMeta(card, needsText(service));
  }
  appendCardMeta(card, service.error, true);
  appendOpenLink(card, service);

  const actions = document.createElement('div');
  actions.className = 'card-actions';

  const isRunning = service.status === 'running';
  const isBusy = service.status === 'starting' || service.status === 'stopping';

  if (service.name === 'admin') {
    const option = document.createElement('label');
    option.className = 'admin-option';

    const checkbox = document.createElement('input');
    checkbox.type = 'checkbox';
    // Prefer what admin was actually started with (survives page refreshes);
    // fall back to the in-session choice only when the server has no record.
    checkbox.checked = service.publicDataDbExists ?? startPublicDataWithAdmin;
    checkbox.disabled = isRunning || isBusy;
    option.classList.toggle('disabled', isRunning || isBusy);
    checkbox.addEventListener('change', () => {
      startPublicDataWithAdmin = checkbox.checked;
      renderApp();
    });
    option.appendChild(checkbox);
    option.appendChild(document.createTextNode('Start with PublicData'));

    card.appendChild(option);
  }

  const toggleBtn = document.createElement('button');
  toggleBtn.className = isRunning ? 'danger' : 'primary';
  toggleBtn.textContent = isRunning ? 'Stop' : 'Start';
  toggleBtn.disabled = isBusy;
  toggleBtn.addEventListener('click', async () => {
    toggleBtn.disabled = true;
    try {
      const startPublicData =
        service.name === 'admin' && !isRunning && startPublicDataWithAdmin;

      await api(
        `/api/services/${service.name}/${isRunning ? 'stop' : 'start'}`,
        {
          method: 'POST',
          ...(startPublicData
            ? { body: JSON.stringify({ startPublicData: true }) }
            : {}),
        },
      );
    } catch (err) {
      window.alert(err.message);
    }
    refreshServices();
  });
  actions.appendChild(toggleBtn);

  const logsBtn = document.createElement('button');
  logsBtn.textContent =
    currentLogService === service.name ? 'Hide logs' : 'Logs';
  logsBtn.addEventListener('click', () => {
    openLogPanel(service.name);
    refreshServices();
  });
  actions.appendChild(logsBtn);

  card.appendChild(actions);
  return card;
}

// Persists which mode is selected per group across re-renders, since
// nothing else remembers it while no member of the group is running.
const selectedModeByGroup = {};

// Services sharing a `group` (e.g. frontend/frontendProd, which can't run at
// the same time) are shown as one tile with a mode selector, rather than as
// separate cards.
function renderGroupedServiceCard(groupName, members) {
  const active = members.find(
    m =>
      m.status === 'running' ||
      m.status === 'starting' ||
      m.status === 'stopping',
  );
  const locked = Boolean(active);
  const selectedName =
    active?.name ?? selectedModeByGroup[groupName] ?? members[0].name;
  const selected = members.find(m => m.name === selectedName) ?? members[0];

  const card = document.createElement('div');
  card.className = 'card';

  const titleRow = document.createElement('div');
  titleRow.className = 'card-title-row';

  const name = document.createElement('span');
  name.className = 'card-name';
  name.textContent = displayName(groupName);
  titleRow.appendChild(name);

  const status = document.createElement('span');
  status.className = 'status-label';
  const displayStatus = active?.status ?? 'stopped';
  status.innerHTML = `<span class="status-dot status-${displayStatus}"></span>${statusLabel(displayStatus)}`;
  titleRow.appendChild(status);

  card.appendChild(titleRow);

  const modeSelector = document.createElement('div');
  modeSelector.className = 'mode-selector';

  members.forEach(member => {
    const label = document.createElement('label');
    const radio = document.createElement('input');
    radio.type = 'radio';
    radio.name = `mode-${groupName}`;
    radio.value = member.name;
    radio.checked = member.name === selectedName;
    radio.disabled = locked;
    radio.addEventListener('change', () => {
      selectedModeByGroup[groupName] = member.name;
      renderApp();
    });
    label.appendChild(radio);
    label.append(displayName(member.name));
    modeSelector.appendChild(label);
  });
  card.appendChild(modeSelector);

  appendCardMeta(card, needsText(selected));
  appendCardMeta(card, selected.error, true);
  appendOpenLink(card, active ?? selected);

  const actions = document.createElement('div');
  actions.className = 'card-actions';

  const isBusy = displayStatus === 'starting' || displayStatus === 'stopping';

  const toggleBtn = document.createElement('button');
  toggleBtn.className = active ? 'danger' : 'primary';
  toggleBtn.textContent = active ? 'Stop' : 'Start';
  toggleBtn.disabled = isBusy;
  toggleBtn.addEventListener('click', async () => {
    const target = active ? active.name : selectedName;
    toggleBtn.disabled = true;
    try {
      await api(`/api/services/${target}/${active ? 'stop' : 'start'}`, {
        method: 'POST',
      });
    } catch (err) {
      window.alert(err.message);
    }
    refreshServices();
  });
  actions.appendChild(toggleBtn);

  const logsBtn = document.createElement('button');
  const logsTarget = active?.name ?? selectedName;
  logsBtn.textContent = currentLogService === logsTarget ? 'Hide logs' : 'Logs';
  logsBtn.addEventListener('click', () => {
    openLogPanel(logsTarget);
    refreshServices();
  });
  actions.appendChild(logsBtn);

  card.appendChild(actions);
  return card;
}

function groupServices(services) {
  const groups = new Map();
  const ungrouped = [];

  services.forEach(service => {
    if (service.kind === 'process' && service.group) {
      const members = groups.get(service.group) ?? [];
      members.push(service);
      groups.set(service.group, members);
    } else {
      ungrouped.push(service);
    }
  });

  return { groups, ungrouped };
}

let lastServices = [];
let fixingIssueId = null;
// Whether starting admin should also start the public API (and force admin's
// PublicDataDbExists setting), chosen via a checkbox on the Admin card.
let startPublicDataWithAdmin = false;

// Issues are rendered in the banner at the top. Each one may name the service
// it's associated with (e.g. the db container), shown as a chip that scrolls
// to that service's card.
async function runIssueFix(issue) {
  if (issue.action === 'open-import') {
    mssqlImportInput.scrollIntoView({
      behavior: 'smooth',
      block: 'center',
    });
    mssqlImportInput.click();
    return;
  }

  fixingIssueId = issue.id;
  refreshServices();
  try {
    await api(issue.fixEndpoint, { method: 'POST' });
  } catch (err) {
    window.alert(err.message);
  }
  fixingIssueId = null;
  refreshServices();
}

function makeIssueFixButton(issue) {
  const fixBtn = document.createElement('button');
  const isFixing = fixingIssueId === issue.id;
  fixBtn.disabled = isFixing;
  fixBtn.textContent = isFixing ? 'Fixing...' : issue.fixLabel;
  fixBtn.addEventListener('click', () => runIssueFix(issue));
  return fixBtn;
}

function renderProjectRootBanner(projectRootOverride) {
  if (!projectRootOverride) {
    projectRootBannerEl.classList.add('hidden');
    return;
  }

  projectRootBannerEl.textContent = `Managing services from ${projectRootOverride} (EES_PROJECT_ROOT override)`;
  projectRootBannerEl.classList.remove('hidden');
}

function renderIssues(issues) {
  issuesBannerEl.replaceChildren();

  if (issues.length === 0) {
    issuesBannerEl.classList.add('hidden');
    return;
  }

  issuesBannerEl.classList.remove('hidden');

  issues.forEach(issue => {
    const text = document.createElement('span');
    text.className = 'issue-text';

    if (issue.serviceName) {
      const chip = document.createElement('button');
      chip.className = 'issue-service-chip';
      chip.title = `Scroll to the ${displayName(issue.serviceName)} service`;
      chip.textContent = displayName(issue.serviceName);
      chip.addEventListener('click', () => {
        document
          .getElementById(`service-card-${issue.serviceName}`)
          ?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      });
      text.appendChild(chip);
    }

    const message = document.createElement('span');
    message.textContent = issue.message;
    text.appendChild(message);

    issuesBannerEl.appendChild(text);

    if (issue.fixLabel) {
      issuesBannerEl.appendChild(makeIssueFixButton(issue));
    }
  });
}

function renderApp() {
  const { groups, ungrouped } = groupServices(lastServices);

  const dockerCards = ungrouped
    .filter(s => s.kind === 'docker')
    .sort((a, b) => a.name.localeCompare(b.name))
    .map(renderServiceCard);
  dockerServicesEl.replaceChildren(...dockerCards);

  const processItems = ungrouped
    .filter(s => s.kind === 'process')
    .map(service => ({
      sortKey: service.name,
      card: renderServiceCard(service),
    }));

  groups.forEach((members, groupName) => {
    processItems.push({
      sortKey: groupName,
      card: renderGroupedServiceCard(groupName, members),
    });
  });

  processItems.sort((a, b) => a.sortKey.localeCompare(b.sortKey));
  appServicesEl.replaceChildren(...processItems.map(item => item.card));
}

async function refreshServices() {
  const {
    services,
    issues = [],
    projectRootOverride,
  } = await api('/api/services');
  lastServices = services;
  renderApp();
  renderIssues(issues);
  renderProjectRootBanner(projectRootOverride);
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

const STORE_LABELS = {
  mssql: 'MSSQL',
  postgres: 'Postgres',
  azurite: 'Azurite',
};

function storesSummary(stores) {
  return Object.entries(STORE_LABELS)
    .filter(([store]) => stores[store])
    .map(([store, label]) => `${label} ${formatBytes(stores[store].sizeBytes)}`)
    .join(' · ');
}

function renderBackupItem(backup) {
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
  meta.textContent = `${formatTimestamp(backup.timestamp)} · ${storesSummary(backup.stores)}`;
  info.appendChild(meta);

  item.appendChild(info);

  const actions = document.createElement('div');
  actions.className = 'backup-item-actions';

  const restoreBtn = document.createElement('button');
  restoreBtn.textContent = 'Restore';
  restoreBtn.addEventListener('click', async () => {
    if (
      !window.confirm(
        `Restore '${backup.label}'? This overwrites the current MSSQL, Postgres, and Azurite local dev data, and cannot be undone.`,
      )
    ) {
      return;
    }
    restoreBtn.disabled = true;
    try {
      await api(`/api/backups/${backup.id}/restore`, { method: 'POST' });
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
      await api(`/api/backups/${backup.id}`, { method: 'DELETE' });
      refreshBackups();
    } catch (err) {
      window.alert(err.message);
    }
  });
  actions.appendChild(deleteBtn);

  item.appendChild(actions);
  return item;
}

function renderBackupPanel(backups) {
  const panel = document.createElement('div');
  panel.className = 'backup-store';

  const createRow = document.createElement('div');
  createRow.className = 'backup-create-row';

  const labelInput = document.createElement('input');
  labelInput.type = 'text';
  labelInput.placeholder = 'Label (optional)';
  createRow.appendChild(labelInput);

  const createBtn = document.createElement('button');
  createBtn.className = 'primary';
  createBtn.textContent = 'Backup';
  createBtn.addEventListener('click', async () => {
    createBtn.disabled = true;
    createBtn.textContent = 'Backing up...';
    try {
      await api('/api/backups', {
        method: 'POST',
        body: JSON.stringify({ label: labelInput.value }),
      });
      labelInput.value = '';
      refreshBackups();
    } catch (err) {
      window.alert(err.message);
    }
    createBtn.disabled = false;
    createBtn.textContent = 'Backup';
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
    backups.forEach(backup => list.appendChild(renderBackupItem(backup)));
  }

  panel.appendChild(list);
  return panel;
}

async function refreshBackups() {
  const { backups } = await api('/api/backups');
  backupPanelEl.replaceChildren(renderBackupPanel(backups));
}

refreshServices();
refreshBackups();
setInterval(refreshServices, 3000);
