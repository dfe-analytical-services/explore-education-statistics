import chalk, { ChalkInstance } from 'chalk';
import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import { getDirname } from './utils/nodeGlobals';

const __dirname = getDirname(import.meta.url);

// EES_PROJECT_ROOT lets this checkout spawn services from a *different*
// checkout on disk - useful when running the dashboard from one git worktree
// (e.g. a branch you're iterating on) while it manages services in another
// (e.g. a feature branch you're actually developing against). Exposed
// separately from `projectRoot` so callers (e.g. the dashboard UI) can tell
// whether it's actually set, rather than just resolving to the default.
export const projectRootOverride = process.env.EES_PROJECT_ROOT
  ? path.resolve(process.env.EES_PROJECT_ROOT)
  : undefined;

export const projectRoot = projectRootOverride ?? path.resolve(__dirname, '..');

/**
 * Shared lock file used to serialize `dotnet build`s across every tool that
 * can start services (the `start` CLI and the dashboard), since concurrent
 * builds risk https://github.com/dotnet/sdk/issues/9487.
 *
 * Anchored to `projectRoot` rather than this file's own directory: what the
 * lock protects is the `src/artifacts` output tree, and with EES_PROJECT_ROOT
 * set that tree belongs to a *different* checkout from the one running this
 * code. Keying it to the checkout instead would give a dashboard running out
 * of one worktree and a `start` CLI run from the managed one two separate
 * lock files, so they'd build into the same artifacts tree uncoordinated.
 *
 * `cross-process-lock` requires its target file to already exist, so it's
 * created here (once, on module load) rather than relying on some source
 * file happening to exist at this path, the way `start.ts` used to rely on
 * its own `__filename`.
 */
export const dotnetBuildLockFile = path.join(
  projectRoot,
  'src/artifacts/.dotnet-build',
);

if (!fs.existsSync(dotnetBuildLockFile)) {
  // The artifacts directory won't exist yet in a checkout that's never been
  // built, and the lock is needed before the build that would create it.
  fs.mkdirSync(path.dirname(dotnetBuildLockFile), { recursive: true });
  fs.writeFileSync(dotnetBuildLockFile, '');
}

export const allowedDockerServices = [
  'db',
  'data-storage',
  'idp',
  'public-api-db',
  'data-screener',
] as const;

export type DockerService = (typeof allowedDockerServices)[number];

/**
 * Subset of `start.ts`'s CLI options that service schemas need to know about.
 * Kept as a plain interface (rather than importing commander's inferred type)
 * so this module has no dependency on the CLI itself.
 */
export interface StartOptions {
  skipBuild?: boolean;
  skipDocker?: boolean;
  restartDocker?: boolean;
  rebuildDocker?: boolean;
  /**
   * Extra environment variables for the spawned process, taking precedence
   * over the process's inherited environment (and, for .NET services, over
   * appsettings - mirroring .NET's own configuration precedence).
   */
  env?: NodeJS.ProcessEnv;
}

export type ServiceSchemaDockerServices =
  DockerService[] | ((options: StartOptions) => DockerService[]);

export type ServiceSchemaDependsOnServices =
  ServiceName[] | ((options: StartOptions) => ServiceName[]);

export type ServiceSchema = {
  colour: ChalkInstance;
  type: 'dotnet' | 'func' | 'docker' | 'command';
  /**
   * Other app-process services (as opposed to Docker services) that this
   * service talks to and so must also be started alongside it - e.g. the
   * frontend calling the content/data APIs directly over HTTP.
   */
  dependsOnServices?: ServiceSchemaDependsOnServices;
  /** Where to open this service in a browser once it's running. */
  url?: string;
  /**
   * Services that can't run at the same time as this one (e.g. `frontend`
   * and `frontendProd` bind the same port from the same project root).
   * Starting this service is rejected while any of these are running.
   */
  conflictsWith?: ServiceName[];
  /**
   * Services sharing this value are presented as one tile with a mode
   * selector in the dashboard, rather than as separate cards.
   */
  group?: string;
} & (
  | {
      type: 'dotnet';
      root: string;
      dockerServices?: ServiceSchemaDockerServices;
    }
  | {
      type: 'func';
      root: string;
      port: number;
      dockerServices?: ServiceSchemaDockerServices;
    }
  | {
      type: 'docker';
      service: DockerService;
    }
  | {
      type: 'command';
      root: string;
      command: string | ((options: StartOptions) => string);
      checkReady?: (line: string) => boolean;
      dockerServices?: ServiceSchemaDockerServices;
    }
);

// Annoyingly, need to define these separately from schemas,
// or we run into various circular reference issues in the types.
export const allowedServiceNames = [
  'admin',
  'analytics',
  'content',
  'data',
  'frontend',
  'frontendProd',
  'processor',
  'publicApiDb',
  'publicData',
  'publicProcessor',
  'publisher',
  'notifier',
  'idp',
  'db',
  'dataStorage',
  'searchFunctionApp',
  'dataScreener',
] as const;

export type ServiceName = (typeof allowedServiceNames)[number];

/**
 * Reads a config value the same way .NET's configuration layering would:
 * appsettings.json, then appsettings.{env}.json, then appsettings.Local.json
 * (a gitignored per-developer override), each overriding the last.
 */
function readLayeredAppSetting<T>(
  root: string,
  key: string,
  env = 'Development',
): T | undefined {
  const files = [
    'appsettings.json',
    `appsettings.${env}.json`,
    'appsettings.Local.json',
  ];

  let value: T | undefined;

  files.forEach(file => {
    const filePath = path.join(projectRoot, root, file);

    if (!fs.existsSync(filePath)) {
      return;
    }

    try {
      const settings = JSON.parse(fs.readFileSync(filePath, 'utf-8'));

      if (key in settings) {
        value = settings[key];
      }
    } catch {
      // Ignore malformed/unparseable appsettings files.
    }
  });

  return value;
}

/**
 * Whether the given service is configured (via its own layered
 * `PublicDataDbExists` appsetting) to use the public data database, ignoring
 * any env override passed at start time. Mirrors that service's own
 * `PublicDataDbExists` check (see Startup.cs for admin,
 * PublisherHostBuilderExtensions.cs for publisher).
 */
export function serviceUsesPublicDataDb(service: ServiceName): boolean {
  const schema = serviceSchemas[service];

  if (schema.type === 'docker') {
    return false;
  }

  return (
    readLayeredAppSetting<boolean>(schema.root, 'PublicDataDbExists') ?? false
  );
}

/**
 * Whether a service is starting configured to use the public data database,
 * preferring an env override (e.g. the dashboard's "Start with PublicData"
 * checkbox, or the CLI's own cross-service resolution below) over the
 * layered appsetting, exactly as .NET's own configuration precedence would.
 */
function resolveServiceUsesPublicDataDb(
  service: ServiceName,
  options: StartOptions,
): boolean {
  const envValue = options.env?.PublicDataDbExists;
  return envValue !== undefined
    ? envValue === 'true'
    : serviceUsesPublicDataDb(service);
}

interface DotnetLaunchProfile {
  commandName?: string;
  applicationUrl?: string;
}

interface DotnetLaunchSettings {
  profiles?: Record<string, DotnetLaunchProfile>;
}

function getDotnetLaunchPort(root: string): number | undefined {
  const filePath = path.join(
    projectRoot,
    root,
    'Properties/launchSettings.json',
  );

  if (!fs.existsSync(filePath)) {
    return undefined;
  }

  try {
    // launchSettings.json files in this repo are saved with a UTF-8 BOM.
    const raw = fs.readFileSync(filePath, 'utf-8').replace(/^\uFEFF/, '');
    const settings = JSON.parse(raw) as DotnetLaunchSettings;
    const profiles = Object.values(settings.profiles ?? {});
    // `dotnet run` uses the "Project" profile, not "IIS Express".
    const projectProfile = profiles.find(
      profile => profile.commandName === 'Project',
    );
    const urls = (projectProfile?.applicationUrl ?? '').split(';');
    const httpUrl = urls.find(url => url.startsWith('http://'));
    const match = httpUrl?.match(/:(\d+)$/);

    return match ? Number(match[1]) : undefined;
  } catch {
    return undefined;
  }
}

/**
 * The port a service will try to bind to, where known - used to proactively
 * free up the port from a stale/orphaned process before starting.
 */
export function getServicePort(service: ServiceName): number | undefined {
  const schema = serviceSchemas[service];

  if (schema.type === 'func') {
    return schema.port;
  }

  if (schema.type === 'dotnet') {
    return getDotnetLaunchPort(schema.root);
  }

  if (schema.type === 'command' && schema.url) {
    const match = schema.url.match(/:(\d+)$/);
    return match ? Number(match[1]) : undefined;
  }

  return undefined;
}

export const serviceSchemas: Record<ServiceName, ServiceSchema> = {
  admin: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Admin',
    colour: chalk.green,
    type: 'dotnet',
    url: 'https://localhost:5021',
    dependsOnServices(options) {
      // `processor`/`publisher` are talked to over Azure Storage Queues
      // (fire-and-forget), not HTTP, so admin works fine without them
      // running - but they're included anyway since without them nothing
      // ever picks the queued work up. `publicProcessor`/`publicData` are
      // real synchronous HTTP dependencies (see `IProcessorClient`/
      // `IPublicDataApiClient` in Startup.cs), only wired up at all when
      // admin is configured to use the public data database.
      const services: ServiceName[] = ['processor', 'publisher'];

      if (resolveServiceUsesPublicDataDb('admin', options)) {
        services.push('publicProcessor', 'publicData');
      }

      return services;
    },
    dockerServices(options) {
      const usesIdpContainer = !fs.existsSync(
        path.join(projectRoot, this.root, 'appsettings.Idp.json'),
      );
      // Mirrors Startup.cs's own `PublicDataDbExists` check, so the
      // dashboard/CLI only spin up `public-api-db` when admin is actually
      // configured to use it (set `PublicDataDbExists: false` in
      // appsettings.Local.json to run admin without the public API). An env
      // override wins where present (e.g. the dashboard starting admin
      // alongside the public API), exactly as it would for .NET.
      const services: DockerService[] = ['db', 'data-storage', 'data-screener'];

      if (resolveServiceUsesPublicDataDb('admin', options)) {
        services.push('public-api-db');
      }

      if (usesIdpContainer) {
        services.push('idp');
      }

      return services;
    },
  },
  analytics: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Analytics.Consumer',
    colour: chalk.rgb(165, 158, 255),
    port: 7076,
    type: 'func',
    dockerServices: ['data-storage'],
  },
  content: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Content.Api',
    colour: chalk.cyan,
    type: 'dotnet',
    dockerServices: ['db', 'data-storage'],
  },
  data: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Data.Api',
    colour: chalk.magenta,
    type: 'dotnet',
    dockerServices: ['db', 'data-storage'],
  },
  frontend: {
    root: 'src/explore-education-statistics-frontend',
    command: 'pnpm dev',
    colour: chalk.greenBright,
    checkReady: line => line.startsWith('Server started on '),
    type: 'command',
    url: 'http://localhost:3000',
    // Calls these directly over HTTP (see CONTENT_API_BASE_URL/
    // DATA_API_BASE_URL in .env) rather than through Docker.
    dependsOnServices: ['content', 'data'],
    // Same port, same project root as frontendProd - can't run both.
    conflictsWith: ['frontendProd'],
    group: 'frontend',
  },
  frontendProd: {
    root: 'src/explore-education-statistics-frontend',
    command(options) {
      return options.skipBuild ? 'pnpm start' : 'pnpm build && pnpm start';
    },
    colour: chalk.greenBright,
    checkReady: line => line.startsWith('Server started on '),
    type: 'command',
    url: 'http://localhost:3000',
    dependsOnServices: ['content', 'data'],
    conflictsWith: ['frontend'],
    group: 'frontend',
  },
  publicData: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Public.Data.Api',
    colour: chalk.magentaBright,
    type: 'dotnet',
    dockerServices: ['public-api-db'],
    // Calls this directly over HTTP (see `IContentApiClient` in Startup.cs).
    dependsOnServices: ['content'],
  },
  processor: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Data.Processor',
    colour: chalk.rgb(255, 158, 165),
    port: 7071,
    type: 'func',
    dockerServices: ['db', 'data-storage'],
  },
  publisher: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Publisher',
    colour: chalk.yellow,
    port: 7072,
    type: 'func',
    // Only bring up the public data database when publisher is actually
    // configured to use it - mirrors admin's own `PublicDataDbExists` check
    // (see PublisherHostBuilderExtensions.cs), falling back to publisher's
    // own layered appsetting when no env override says otherwise.
    dockerServices(options) {
      const services: DockerService[] = ['db', 'data-storage'];

      if (resolveServiceUsesPublicDataDb('publisher', options)) {
        services.push('public-api-db');
      }

      return services;
    },
  },
  notifier: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Notifier',
    colour: chalk.blue,
    port: 7073,
    type: 'func',
    dockerServices: ['db', 'data-storage'],
  },
  publicProcessor: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Public.Data.Processor',
    colour: chalk.blue,
    port: 7074,
    type: 'func',
    dockerServices: ['db', 'public-api-db', 'data-storage'],
  },
  searchFunctionApp: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp',
    colour: chalk.rgb(255, 102, 0),
    port: 7075,
    type: 'func',
    dockerServices: ['data-storage'],
    // Calls this directly over HTTP (see `IContentApiClient` in
    // HostBuilderExtension.cs).
    dependsOnServices: ['content'],
  },
  dataScreener: {
    colour: chalk.rgb(0, 255, 221),
    service: 'data-screener',
    type: 'docker',
  },
  idp: {
    service: 'idp',
    colour: chalk.gray,
    type: 'docker',
    url: 'http://localhost:5030',
  },
  db: {
    service: 'db',
    colour: chalk.blue,
    type: 'docker',
  },
  dataStorage: {
    service: 'data-storage',
    colour: chalk.green,
    type: 'docker',
  },
  publicApiDb: {
    service: 'public-api-db',
    colour: chalk.blue,
    type: 'docker',
  },
};

export interface RunCommand {
  command: string;
  args: string[];
  env: NodeJS.ProcessEnv;
  checkReady?: (line: string) => boolean;
  /**
   * Whether only one instance of this kind of process should run at a time
   * across the whole machine (used to serialize concurrent `dotnet build`s,
   * which otherwise risk https://github.com/dotnet/sdk/issues/9487).
   */
  lockUntilReady: boolean;
}

/**
 * Build the command needed to run a `dotnet`, `func`, or `command`-type
 * service. Docker-type services are handled separately by callers, since
 * how they're run (compose up/start vs. tailing logs) is caller-specific.
 */
export function buildRunCommand(
  service: ServiceName,
  options: StartOptions,
  baseEnv: NodeJS.ProcessEnv = process.env,
): RunCommand {
  const schema = serviceSchemas[service];
  const env: NodeJS.ProcessEnv = { ...baseEnv, ...options.env };

  switch (schema.type) {
    case 'dotnet': {
      env.ASPNETCORE_ENVIRONMENT ??= 'Development';
      env.MSBUILDDISABLENODEREUSE = '1';

      return {
        command: 'dotnet run',
        args: [],
        env,
        checkReady: line => line.startsWith('Server listening on address:'),
        lockUntilReady: true,
      };
    }
    case 'func': {
      env.ASPNETCORE_ENVIRONMENT ??= 'Development';
      env.MSBUILDDISABLENODEREUSE = '1';

      return {
        command: 'dotnet run',
        args: ['--port', `${schema.port}`, '--pause-on-error'],
        env,
        checkReady: line => line.startsWith('Function Runtime Version:'),
        lockUntilReady: true,
      };
    }
    case 'command': {
      const command =
        typeof schema.command === 'function'
          ? schema.command.call(schema, options)
          : schema.command;

      return {
        command,
        args: [],
        env,
        checkReady: schema.checkReady,
        lockUntilReady: false,
      };
    }
    default:
      throw new Error(
        `Service '${service}' is a Docker service, not a process`,
      );
  }
}

function resolveOwnDockerServices(
  service: ServiceName,
  options: StartOptions,
): DockerService[] {
  const schema = serviceSchemas[service];

  if (schema.type === 'docker') {
    return [schema.service];
  }

  if (!('dockerServices' in schema) || !schema.dockerServices) {
    return [];
  }

  const { dockerServices } = schema;

  return typeof dockerServices === 'function'
    ? dockerServices.call(schema, options)
    : dockerServices;
}

function resolveOwnServiceDependencies(
  service: ServiceName,
  options: StartOptions,
): ServiceName[] {
  const { dependsOnServices } = serviceSchemas[service];

  if (!dependsOnServices) {
    return [];
  }

  return typeof dependsOnServices === 'function'
    ? dependsOnServices(options)
    : dependsOnServices;
}

/**
 * The transitive closure of `dependsOnServices` for a service, i.e. every
 * other app-process service that must also be started alongside it - not
 * including the service itself.
 */
export function resolveServiceDependencies(
  service: ServiceName,
  options: StartOptions = {},
  seen: Set<ServiceName> = new Set(),
): ServiceName[] {
  // Seeding `seen` with the service itself is what keeps the "not including
  // the service itself" part of the contract true if a cycle is ever
  // introduced: without it, A -> B -> A resolves A as its own dependency,
  // which reads as a schema bug rather than the cycle it actually is.
  seen.add(service);

  const dependencies = resolveOwnServiceDependencies(service, options);
  const result: ServiceName[] = [];

  dependencies.forEach(dependency => {
    if (seen.has(dependency)) {
      return;
    }

    seen.add(dependency);
    result.push(
      ...resolveServiceDependencies(dependency, options, seen),
      dependency,
    );
  });

  return result;
}

/**
 * All Docker services a service needs, including those needed by any
 * app-process services it transitively depends on (e.g. the frontend
 * doesn't declare any Docker services itself, but needs `db`/`data-storage`
 * because it depends on `content`/`data`, which do).
 */
export function resolveDockerServices(
  service: ServiceName,
  options: StartOptions,
): DockerService[] {
  const services = [service, ...resolveServiceDependencies(service, options)];
  const dockerServices = new Set<DockerService>();

  services.forEach(s => {
    resolveOwnDockerServices(s, options).forEach(d => dockerServices.add(d));
  });

  return Array.from(dockerServices);
}

/**
 * Whether `public-api-db` will actually be available once every one of the
 * given services is started - not just one of them considered in isolation.
 * An explicit `PublicDataDbExists` env override always wins; otherwise it's
 * true if ANY of the given services would pull `public-api-db` in on its
 * own (asking for it directly, or via its own schema/appsettings).
 *
 * Callers that start several services in one go (e.g. the CLI) should
 * resolve this once up front and thread the result through as a shared env
 * override, rather than letting each service's schema work it out
 * independently - otherwise whether e.g. `publisher` gets `public-api-db`
 * could depend on whether `admin` happened to be listed before or after it.
 */
export function resolvePublicDataDbAvailability(
  services: readonly ServiceName[],
  options: StartOptions = {},
): boolean {
  const envValue = options.env?.PublicDataDbExists;

  if (envValue !== undefined) {
    return envValue === 'true';
  }

  return services.some(service => {
    const schema = serviceSchemas[service];

    if (schema.type === 'docker') {
      return schema.service === 'public-api-db';
    }

    return resolveOwnDockerServices(service, {}).includes('public-api-db');
  });
}
