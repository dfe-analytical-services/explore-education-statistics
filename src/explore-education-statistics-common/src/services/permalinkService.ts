import { dataApi } from './api';

export interface Permalink {
  id: string;
  title: string;
  created: string;
  data: string
}

export default {
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
};
