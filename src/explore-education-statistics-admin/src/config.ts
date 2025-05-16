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
  readonly publicAppUrl: string;
  readonly publicApiUrl: string;
  readonly publicApiDocsUrl: string;
  readonly enableReplacementOfPublicApiDataSets?: boolean;
}

let config: Config;

export async function getConfig(): Promise<Config> {
  if (!config) {
    config = await fetch('/api/config').then(r => r.json());
  }
  return config;
}
