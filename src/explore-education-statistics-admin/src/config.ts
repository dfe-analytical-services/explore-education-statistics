import client from '@admin/services/util/service';
import produce from 'immer';

export interface Config {
  readonly AppInsightsKey: string;
  readonly PublicAppUrl: string;
}

let config: Config;

export async function getConfig(): Promise<Config> {
  if (config) {
    return config;
  }

  const configResponse = await client.get<Config>('/config');

  config = produce(undefined, () => configResponse);

  return config;
}
