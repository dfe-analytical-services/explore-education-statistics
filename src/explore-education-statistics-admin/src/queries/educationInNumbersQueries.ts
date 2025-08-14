import { createQueryKeys } from '@lukemorales/query-key-factory';
import educationInNumbersService from '@admin/services/educationInNumbersService';

const educationInNumbersQueries = createQueryKeys('education-in-numbers', {
  getEducationInNumbersPage(id: string) {
    return {
      queryKey: [id],
      queryFn: () => educationInNumbersService.getEducationInNumbersPage(id),
    };
  },
  listLatestPages: {
    queryKey: null,
    queryFn: () => educationInNumbersService.listLatestPages(),
  },
});

export default educationInNumbersQueries;
