import { contentApi } from './api';

export interface Redirects {
  methodologies: Redirect[];
  publications: Redirect[];
}

export type RedirectType = keyof Redirects;

interface Redirect {
  fromSlug: string;
  toSlug: string;
}

export const redirectPathStarts = {
  methodologies: '/methodology',
  publications: '/find-statistics',
};

const redirectService = {
  async list(): Promise<Redirects> {
    return contentApi.get(`/redirects`);
  },
};

export default redirectService;
