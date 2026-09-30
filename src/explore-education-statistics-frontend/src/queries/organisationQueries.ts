import { Organisation } from '@common/services/types/organisation';
import organisationService from '@frontend/services/organisationService';
import { UseQueryOptions } from '@tanstack/react-query';

const organisationQueries = {
  list(): UseQueryOptions<Organisation[]> {
    return {
      queryKey: ['listOrganisations'],
      queryFn: () => organisationService.list(),
    };
  },
} as const;

export default organisationQueries;
