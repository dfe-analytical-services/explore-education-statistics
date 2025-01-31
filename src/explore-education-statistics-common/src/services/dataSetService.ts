import { contentApi } from './api';

export interface DataSetSitemapItem {
  id: string;
  lastModified?: string;
}

const dataSetService = {
  listSitemapItems(): Promise<DataSetSitemapItem[]> {
    return contentApi.get('/data-set-files/sitemap-items');
  },
};

export default dataSetService;
