import client from '@admin/services/utils/service';
import produce from 'immer';

export interface Config {
  readonly AppInsightsKey: string;
  readonly PublicAppUrl: string;
  readonly PermittedEmbedUrlDomains: string[];
}

let config: Config;

export async function getConfig(): Promise<Config> {
  if (config) {
    return config;
  }

  const configResponse = await client.get<Config>('/config');

  config = produce<Config>(
    (undefined as unknown) as Config,
    () => configResponse,
  );

  return config;
}
