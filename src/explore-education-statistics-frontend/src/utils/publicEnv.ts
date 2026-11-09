/**
 * Environment variables that the browser needs.
 *
 * The app is built once and deployed to every environment, so these can't be
 * inlined at build time (e.g. with `NEXT_PUBLIC_` variables). Instead,
 * `_document` renders them into each page as JSON (see
 * {@link getPublicEnvPayload}) from the server's runtime environment,
 * and {@link getPublicEnv} reads them back in the browser.
 *
 * Only add variables here that are safe to expose publicly.
 */
export interface PublicEnv {
  APP_ENV: 'Production' | 'Pre-Production' | 'Test' | 'Development' | 'Local';
  APPINSIGHTS_INSTRUMENTATIONKEY: string;
  CONTENT_API_BASE_URL: string;
  DATA_API_BASE_URL: string;
  GA_TRACKING_ID: string;
  NOTIFICATION_API_BASE_URL: string;
  PUBLIC_API_BASE_URL: string;
  PUBLIC_API_DOCS_URL: string;
  PUBLIC_URL: string;
}

export type PublicEnvKey = keyof PublicEnv;

export const publicEnvKeys = [
  'APP_ENV',
  'APPINSIGHTS_INSTRUMENTATIONKEY',
  'CONTENT_API_BASE_URL',
  'DATA_API_BASE_URL',
  'GA_TRACKING_ID',
  'NOTIFICATION_API_BASE_URL',
  'PUBLIC_API_BASE_URL',
  'PUBLIC_API_DOCS_URL',
  'PUBLIC_URL',
] as const satisfies readonly PublicEnvKey[];

export const publicEnvElementId = '__PUBLIC_ENV__';

let clientEnv: Partial<PublicEnv> | undefined;

function getClientEnv(): Partial<PublicEnv> | undefined {
  if (clientEnv) {
    return clientEnv;
  }

  if (typeof document === 'undefined') {
    return undefined;
  }

  const element = document.getElementById(publicEnvElementId);

  if (!element?.textContent) {
    return undefined;
  }

  clientEnv = JSON.parse(element.textContent) as Partial<PublicEnv>;

  return clientEnv;
}

function normalise(key: PublicEnvKey, value?: string): string | undefined {
  if (key === 'PUBLIC_URL' && value) {
    return value.replace(/\/+$/, '');
  }

  return value;
}

/**
 * Get a public environment variable.
 *
 * In the browser, this reads the JSON payload rendered by `_document`.
 * On the server (and in tests), it reads `process.env` directly.
 */
export default function getPublicEnv<K extends PublicEnvKey>(
  key: K,
): PublicEnv[K] {
  const env = getClientEnv();
  const value = env ? env[key] : process.env[key];

  return normalise(key, value) as PublicEnv[K];
}

/**
 * Get the public environment variables from the server's `process.env`,
 * for `_document` to render into the page.
 */
export function getPublicEnvPayload(): Partial<PublicEnv> {
  return publicEnvKeys.reduce<Partial<Record<PublicEnvKey, string>>>(
    (acc, key) => {
      const value = normalise(key, process.env[key]);

      if (value !== undefined) {
        acc[key] = value;
      }

      return acc;
    },
    {},
  ) as Partial<PublicEnv>;
}

/**
 * Serialise the payload so it's safe to embed inside a `<script>` element.
 */
export function serialisePublicEnvPayload(payload: Partial<PublicEnv>): string {
  return JSON.stringify(payload).replace(/</g, '\\u003c');
}
