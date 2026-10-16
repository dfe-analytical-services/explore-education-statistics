import { contentApi } from '@common/services/api';
import { Organisation } from '@common/services/types/organisation';

const organisationService = {
  list(): Promise<Organisation[]> {
    return contentApi.get('/organisations');
  },
};

export default organisationService;
