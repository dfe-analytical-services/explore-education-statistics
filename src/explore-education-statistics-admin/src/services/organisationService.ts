// import client from '@admin/services/utils/service';
import { Organisation } from '@common/services/types/organisation';

const releaseService = {
  listOrganisations(): Promise<Organisation[]> {
    return new Promise(resolve => {
      return resolve([
        {
          id: '466a14bf-4c77-4fb4-beb0-a09065d9ced8',
          title: 'Department for Education',
          url: 'https://www.gov.uk/government/organisations/department-for-education',
        },
        {
          id: '8d26bfaa-44b8-461e-9260-2b0eed9631e0',
          title: 'Other Organisation name',
          url: 'https://example.com',
        },
      ]);
    });
    // return client.get('/organisations');
  },
};

export default releaseService;
