import { createQueryKeys } from '@lukemorales/query-key-factory';
import educationInNumbersService from '@admin/services/educationInNumbersService';

const educationInNumbersQueries = createQueryKeys('education-in-numbers', {
  listLatestPages: {
    queryKey: null,
    queryFn: () => educationInNumbersService.listLatestPages(),
  },
});

export default educationInNumbersQueries;
