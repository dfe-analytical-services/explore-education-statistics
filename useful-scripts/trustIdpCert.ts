#!/usr/bin/env ts-node

/**
 * Exports the self-signed HTTPS certificate from the running Keycloak IdP container
 * (`ees-idp`) and installs it into the current user's trusted certificate store, so
 * that browsers accept https://ees.local:5031 without a certificate warning.
 *
 * This is particularly useful on managed devices where policy prevents clicking
 * through browser certificate warnings.
 *
 * The certificate is regenerated whenever the Keycloak image is rebuilt, so re-run
 * this script after rebuilding (e.g. with `pnpm start idp --rebuild-docker`).
 */

import { X509Certificate } from 'node:crypto';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import process from 'node:process';
import { $ } from 'execa';
import exitProcess from './utils/exitProcess';
import { logError, logInfo } from './utils/logging';

const containerName = 'ees-idp';
const certAlias = 'ees-idp';
const keystorePath = '/opt/keycloak/conf/server.keystore';

async function main(): Promise<void> {
  const running = await $({
    reject: false,
  })`docker inspect -f {{.State.Running}} ${containerName}`;

  if (running.exitCode !== 0 || running.stdout.trim() !== 'true') {
    logError(
      `The '${containerName}' container is not running. Start it first with: pnpm start idp`,
    );
    exitProcess({ code: 1 });
  }

  logInfo('Exporting certificate from the Keycloak container...');

  await $`docker exec ${containerName} keytool -exportcert -alias ${certAlias} -keystore ${keystorePath} -storepass password -rfc -file /tmp/ees-idp.crt`;

  const certPath = path.join(os.tmpdir(), 'ees-idp.crt');

  await $`docker cp ${containerName}:/tmp/ees-idp.crt ${certPath}`;

  const cert = new X509Certificate(fs.readFileSync(certPath));
  const thumbprint = cert.fingerprint.replaceAll(':', '');

  switch (process.platform) {
    case 'win32': {
      const existing = await $({
        reject: false,
      })`certutil -store -user Root ${thumbprint}`;

      if (existing.exitCode === 0) {
        logInfo('Certificate is already trusted. Nothing to do.');
        return;
      }

      logInfo(
        'Installing the certificate. Accept the security prompt that appears...',
      );

      await $({ stdio: 'inherit' })`certutil -addstore -user Root ${certPath}`;
      break;
    }
    case 'darwin': {
      const keychain = path.join(
        os.homedir(),
        'Library/Keychains/login.keychain-db',
      );

      const existing = await $({
        reject: false,
      })`security find-certificate -a -c ees.local -Z ${keychain}`;

      if (existing.stdout?.toUpperCase().includes(thumbprint.toUpperCase())) {
        logInfo('Certificate is already trusted. Nothing to do.');
        return;
      }

      logInfo(
        'Installing the certificate. You may be prompted for your password...',
      );

      await $({
        stdio: 'inherit',
      })`security add-trusted-cert -r trustRoot -k ${keychain} ${certPath}`;
      break;
    }
    default: {
      logInfo(
        `The certificate has been exported to: ${certPath}\n` +
          `Automatic installation isn't supported on this platform. To trust it manually:\n` +
          `  - System store: sudo cp ${certPath} /usr/local/share/ca-certificates/ && sudo update-ca-certificates\n` +
          `  - Chrome/Chromium (NSS): certutil -d sql:$HOME/.pki/nssdb -A -t "C,," -n ees-idp -i ${certPath}`,
      );
      return;
    }
  }

  logInfo(
    'Certificate trusted successfully. Fully reload the admin in your browser (Ctrl+Shift+R).',
  );
}

main().catch(error => {
  logError(String(error));
  exitProcess({ code: 1 });
});
