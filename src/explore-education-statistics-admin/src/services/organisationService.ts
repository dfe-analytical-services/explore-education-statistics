import client from '@admin/services/utils/service';
import { Organisation } from '@common/services/types/organisation';

const releaseService = {
  listOrganisations(): Promise<Organisation[]> {
    return client.get('/organisations');
  },
};

export default releaseService;
