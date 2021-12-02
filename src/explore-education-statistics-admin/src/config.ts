import client from '@admin/services/utils/service';
import produce from 'immer';

export interface Config {
  readonly AppInsightsKey: string;
  readonly PublicAppUrl: string;
}

let config: Config | undefined;

export async function getConfig(): Promise<Config | undefined> {
  if (config) {
    return config;
  }

  const configResponse = await client.get<Config>('/config');

  config = produce(undefined, () => configResponse);

  return config;
}
