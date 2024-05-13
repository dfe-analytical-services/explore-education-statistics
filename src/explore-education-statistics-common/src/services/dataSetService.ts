import { contentApi } from './api';

export interface DataSetSitemapSummary {
  id: string;
  lastModified: Date | undefined;
}

const dataSetService = {
  getSitemapSummaries(): Promise<DataSetSitemapSummary[]> {
    return contentApi.get('/data-set-files/sitemap-summaries');
  },
};

export default dataSetService;
