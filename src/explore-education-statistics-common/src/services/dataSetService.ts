import { contentApi } from './api';

export interface DataSetSitemapItem {
  id: string;
  lastModified?: string;
}

const dataSetService = {
  getSitemapItems(): Promise<DataSetSitemapItem[]> {
    return contentApi.get('/data-set-files/sitemap-summaries');
  },
};

export default dataSetService;
