#!/usr/bin/env ts-node

import { Argument, Command, Option } from '@commander-js/extra-typings';
import chalk, { ChalkInstance } from 'chalk';
import { $, ExecaChildProcess } from 'execa';
import fs from 'node:fs';
import { EOL } from 'node:os';
import path from 'node:path';
import process from 'node:process';
import splitLines from 'split2';
import kill from 'tree-kill';
import delay from './utils/delay';
import exitProcess from './utils/exitProcess';
import { logColours, logError, logInfo } from './utils/logging';
import { getDirname, getFilename } from './utils/nodeGlobals';
import onExitSignal from './utils/onExitSignal';
import patchSigInt from './utils/patchSigInt';
import createFileLock from './utils/createFileLock';
import { ExecaChildProcessWithoutNullStreams } from './utils/types';

patchSigInt();

const __dirname = getDirname(import.meta.url);
const __filename = getFilename(import.meta.url);
const projectRoot = path.resolve(__dirname, '..');

const allowedDockerServices = ['db', 'data-storage', 'idp'] as const;

type DockerService = (typeof allowedDockerServices)[number];

type ServiceSchema = {
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
      command: string | ((options: ProgramOptions) => string);
      checkReady?: (line: string) => boolean;
      dockerServices?: ServiceSchemaDockerServices;
    }
);

type ServiceSchemaDockerServices =
  | DockerService[]
  | ((options: ProgramOptions) => DockerService[]);

// Annoyingly, need to define these separately from schemas,
// or we run into various circular reference issues in the types.
const allowedServiceNames = [
  'admin',
  'content',
  'data',
  'frontend',
  'frontendProd',
  'processor',
  'publisher',
  'notifier',
  'idp',
  'db',
  'dataStorage',
] as const;

type ServiceName = (typeof allowedServiceNames)[number];

const serviceSchemas: Record<ServiceName, ServiceSchema> = {
  admin: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Admin',
    colour: chalk.green,
    type: 'dotnet',
    dockerServices() {
      return fs.existsSync(
        path.join(projectRoot, this.root, 'appsettings.Idp.json'),
      )
        ? ['db', 'data-storage']
        : ['db', 'data-storage', 'idp'];
    },
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
    dockerServices: ['db', 'data-storage'],
  },
  notifier: {
    root: 'src/GovUk.Education.ExploreEducationStatistics.Notifier',
    colour: chalk.blue,
    port: 7073,
    type: 'func',
    dockerServices: ['data-storage'],
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
};

type ProgramOptions = ReturnType<(typeof program)['opts']>;

const program = new Command()
  .description(
    `Start one or more project services in parallel.

This script will also run prerequisite tasks to ensure that services will be able to startup:

- Starting Docker services that are required for any services to start correctly (e.g. the database)
- Run a .NET clean and build will be executed for any .NET services (e.g. admin)
`,
  )
  .addHelpText(
    'after',
    `
Examples:

Start frontend:
  $ start frontend

Start frontend in production mode:
  $ start frontendProd

Start frontend in production mode without build step:
  $ start frontendProd --skip-build

Start content and data APIs:
  $ start data content

Start admin, processor and publisher:
  $ start admin processor publisher

Start .NET services without a clean step:
  $ start data content --skip-clean

Start services without first starting any Docker services:
  $ start data content --skip-docker

Start Docker services directly:
  $ start db dataStorage

Restart Docker services:
  $ start db dataStorage --restart-docker  
`,
  )
  .addArgument(
    new Argument('<services...>', 'The services to start').choices(
      Object.keys(serviceSchemas) as ServiceName[],
    ),
  )
  .addOption(new Option('--restart-docker', 'Restart any Docker containers'))
  .addOption(new Option('--rebuild-docker', 'Rebuild any Docker containers'))
  .addOption(new Option('--skip-clean', 'Skip clean steps where possible'))
  .addOption(new Option('--skip-build', 'Skip build steps where possible'))
  .addOption(
    new Option(
      '--skip-docker',
      'Skip running Docker services that are dependencies of other services',
    ),
  );

program.parse();

const programOpts = program.opts();

const [servicesToStart] = program.processedArgs;

await startDockerServices();

const serviceProcesses = new Set<ExecaChildProcess>();

onExitSignal(() => {
  kill(process.pid);
});

// eslint-disable-next-line no-restricted-syntax
for await (const service of servicesToStart) {
  await startService(service);
  await delay(2000);
}

// Call shutdown to try and avoid residual processes hanging around after every build.
// These processes aren't cleaned up properly, resulting in excessive memory usage.
// See: https://github.com/dotnet/sdk/issues/9487
await $({
  cwd: path.join(projectRoot, 'src'),
  reject: false,
})`dotnet build-server shutdown`;

async function startDockerServices() {
  if (programOpts.skipDocker) {
    return;
  }

  const dockerServicesToStart = servicesToStart.reduce<Set<DockerService>>(
    (acc, service) => {
      const serviceSchema = serviceSchemas[service];

      if (serviceSchema.type === 'docker') {
        acc.add(serviceSchema.service);
      } else if ('dockerServices' in serviceSchema) {
        const { dockerServices } = serviceSchema;

        const services =
          typeof dockerServices === 'function'
            ? dockerServices.call(serviceSchema, programOpts)
            : dockerServices;

        services?.forEach(dockerService => acc.add(dockerService));
      }

      return acc;
    },
    new Set(),
  );

  if (dockerServicesToStart.size > 0) {
    const $$ = $({
      cwd: projectRoot,
      stdio: 'inherit',
    });

    if (programOpts.restartDocker) {
      logInfo('Stopping Docker services...');

      await $$`docker-compose stop ${[...dockerServicesToStart]}`;
    }

    logInfo('Starting Docker services...');

    const args = ['-d'];

    if (programOpts.rebuildDocker) {
      args.push('--build', '--force-recreate');
    }

    await $$`docker-compose up ${[...args, ...dockerServicesToStart]}`;

    await delay(1000);
  }
}

async function runDotnetClean(service: ServiceName) {
  const schema = serviceSchemas[service];

  if (schema.type !== 'dotnet' && schema.type !== 'func') {
    return;
  }

  if (!programOpts.skipClean) {
    logService(service, logColours.info('Running clean...'));

    await $({
      cwd: path.join(projectRoot, schema.root),
    })`dotnet clean`;
  }
}

async function startService(service: ServiceName): Promise<void> {
  const schema = serviceSchemas[service];

  let command = '';
  let args: string[] = [];

  let lockUntilReady = false;
  let beforeTask: (() => void | Promise<void>) | undefined;
  let checkReady: ((line: string) => boolean) | undefined;

  const env = {
    ...process.env,
  };

  switch (schema.type) {
    case 'dotnet': {
      command = 'dotnet run';

      env.ASPNETCORE_ENVIRONMENT ??= 'Development';
      env.MSBUILDDISABLENODEREUSE = '1';

      lockUntilReady = true;
      beforeTask = () => runDotnetClean(service);
      checkReady = line => line.startsWith('Server listening on address:');

      break;
    }
    case 'func': {
      command = 'func host start';
      args = ['--port', `${schema.port}`, '--pause-on-error'];

      env.ASPNETCORE_ENVIRONMENT ??= 'Development';
      env.MSBUILDDISABLENODEREUSE = '1';

      lockUntilReady = true;
      beforeTask = () => runDotnetClean(service);
      checkReady = line => line.startsWith('Function Runtime Version:');

      break;
    }
    case 'command': {
      command =
        typeof schema.command === 'function'
          ? schema.command.call(schema, programOpts)
          : schema.command;

      checkReady = schema.checkReady;

      break;
    }
    case 'docker': {
      command = 'docker-compose logs';
      args = ['-f', '--no-log-prefix'];

      if (programOpts.rebuildDocker) {
        args.push('--build', '--force-recreate');
      }

      args.push(schema.service);

      break;
    }
    default:
      throw new Error('Unknown service definition type');
  }

  const lockFile = __filename;
  const message = logColours.info(
    `Waiting for another process to finish with lock on '${path.basename(
      lockFile,
    )}'...`,
  );

  const unlock = lockUntilReady
    ? await createFileLock({
        lockFile,
        lockTimeout: 300_000,
        waitTimeout: 300_000,
        onExistingLock: () => logService(service, message),
      })
    : undefined;

  try {
    await beforeTask?.();
  } catch (err) {
    if (err instanceof Error) {
      logError(err.message);
      unlock?.();
    }

    return undefined;
  }

  logService(service, logColours.info('Starting service...'));

  const serviceProcess = $({
    cwd: path.join(projectRoot, schema.type === 'docker' ? '' : schema.root),
    env,
    shell: true,
    cleanup: false,
  })`${command} ${args}` as ExecaChildProcessWithoutNullStreams;

  serviceProcesses.add(serviceProcess);

  return new Promise<void>(resolve => {
    let isReady = false;

    const startNextService = async () => {
      isReady = true;
      await unlock?.();
      resolve();
    };

    if (!lockUntilReady) {
      startNextService();
    }

    serviceProcess.stdout
      .pipe(
        tagServiceStream(service, line => {
          if (!isReady && checkReady?.(line)) {
            // Don't need to await this
            startNextService();
          }

          return line;
        }),
      )
      .pipe(process.stdout);

    serviceProcess.stderr
      .pipe(tagServiceStream(service, logColours.error))
      .pipe(process.stderr);

    serviceProcess.on('exit', async (code, signal) => {
      serviceProcesses.delete(serviceProcess);

      if (!isReady) {
        // Let the next service run
        await startNextService();
        return;
      }

      if (serviceProcesses.size > 0) {
        return;
      }

      exitProcess({ signal, code });
    });
  });
}

function tagServiceStream(
  service: ServiceName,
  transform?: (line: string) => string,
) {
  return splitLines(line => {
    return `${serviceSchemas[service].colour(`[${service}]`)} ${
      typeof transform === 'function' ? transform(line) : line
    }${EOL}`;
  });
}

function logService(service: ServiceName, message: string): void {
  const { colour } = serviceSchemas[service];

  console.info(`${colour(`[${service}]`)} ${message}`);
}
