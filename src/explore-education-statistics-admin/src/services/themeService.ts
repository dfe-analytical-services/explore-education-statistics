import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';

export interface Theme {
  id: string;
  title: string;
  summary: string;
  slug: string;
  topics: Topic[];
}

export type Topic = IdTitlePair;

const themeService = {
  getThemes(): Promise<Theme[]> {
    return client.get('/themes');
  },
};

export default themeService;
