import { setHostUrl } from '@common/utils/url/hostUrl';

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
  readonly publicAppUrl: string;
  readonly permittedEmbedUrlDomains: string[];
  readonly oidc: OidcConfig;
  readonly url: string;
}

let config: Config;

export async function getConfig(): Promise<Config> {
  if (!config) {
    config = await fetch('/api/config').then(r => r.json());

    setHostUrl(config.url);
  }
  return config;
}
