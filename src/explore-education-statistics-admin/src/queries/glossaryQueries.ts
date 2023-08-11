import glossaryService from '@admin/services/glossaryService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const glossaryQueries = createQueryKeys('glossary', {
  list: {
    queryKey: null,
    queryFn: () => glossaryService.listEntries(),
  },
});
export default glossaryQueries;
