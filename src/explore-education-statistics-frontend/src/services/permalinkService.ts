import { dataApi } from '@common/services/api';
import FullTable from '@frontend/modules/table-tool/components/types/FullTable';

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
}

export default {
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
};
