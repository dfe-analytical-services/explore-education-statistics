import chalk, { ChalkInstance } from 'chalk';
import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import { getDirname } from './utils/nodeGlobals';

const __dirname = getDirname(import.meta.url);

export const projectRoot = path.resolve(__dirname, '..');

/**
 * Shared lock file used to serialize `dotnet build`s across every tool that
 * can start services (the `start` CLI and the dashboard), since concurrent
 * builds risk https://github.com/dotnet/sdk/issues/9487.
 *
 * `cross-process-lock` requires its target file to already exist, so it's
 * created here (once, on module load) rather than relying on some source
 * file happening to exist at this path, the way `start.ts` used to rely on
 * its own `__filename`.
 */
export const dotnetBuildLockFile = path.join(__dirname, '.dotnet-build');

if (!fs.existsSync(dotnetBuildLockFile)) {
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
}

export type ServiceSchemaDockerServices =
  DockerService[] | ((options: StartOptions) => DockerService[]);

export type ServiceSchema = {
  colour: ChalkInstance;
  type: 'dotnet' | 'func' | 'docker' | 'command';
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

export const serviceSchemas: Record<ServiceName, ServiceSchema> = {
  admin: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Admin',
    colour: chalk.green,
    type: 'dotnet',
    dockerServices() {
      return fs.existsSync(
        path.join(projectRoot, this.root, 'appsettings.Idp.json'),
      )
        ? ['db', 'data-storage', 'public-api-db', 'data-screener']
        : ['db', 'data-storage', 'public-api-db', 'idp', 'data-screener'];
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
  },
  frontendProd: {
    root: 'src/explore-education-statistics-frontend',
    command(options) {
      return options.skipBuild ? 'pnpm start' : 'pnpm build && pnpm start';
    },
    colour: chalk.greenBright,
    checkReady: line => line.startsWith('Server started on '),
    type: 'command',
  },
  publicData: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Public.Data.Api',
    colour: chalk.magentaBright,
    type: 'dotnet',
    dockerServices: ['public-api-db'],
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
    dockerServices: ['db', 'data-storage', 'public-api-db'],
  },
  notifier: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Notifier',
    colour: chalk.blue,
    port: 7073,
    type: 'func',
    dockerServices: ['data-storage'],
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
  const env: NodeJS.ProcessEnv = { ...baseEnv };

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

export function resolveDockerServices(
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
