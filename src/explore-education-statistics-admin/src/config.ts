export interface OidcConfig {
  readonly clientId: string;
  readonly authority: string;
  readonly knownAuthorities: string[];
  readonly adminApiScope: string;
  readonly authorityMetadata?: {
    readonly authorizationEndpoint: string;
    readonly tokenEndpoint: string;
    readonly issuer: string;
    readonly userInfoEndpoint: string;
    readonly endSessionEndpoint: string;
  };
}

export interface Config {
  readonly appInsightsKey: string;
  readonly oidc: OidcConfig;
  readonly permittedEmbedUrlDomains: string[];
  /**
   * Normalised to never end with a slash, so paths
   * can safely be appended to it.
   */
  readonly publicAppUrl: string;
  readonly publicApiUrl: string;
  readonly publicApiDocsUrl: string;
}

let config: Config;

export async function getConfig(): Promise<Config> {
  if (!config) {
    const fetchedConfig: Config = await fetch('/api/config').then(r =>
      r.json(),
    );

    config = {
      ...fetchedConfig,
      // May be configured with or without a trailing slash, so normalise it to
      // never have one, allowing paths to be appended to it safely.
      publicAppUrl: fetchedConfig.publicAppUrl.replace(/\/$/, ''),
    };
  }
  return config;
}
