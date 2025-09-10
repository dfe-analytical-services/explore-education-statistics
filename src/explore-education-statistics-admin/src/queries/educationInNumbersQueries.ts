import { createQueryKeys } from '@lukemorales/query-key-factory';
import educationInNumbersService from '@admin/services/educationInNumbersService';

const educationInNumbersQueries = createQueryKeys('education-in-numbers', {
  getEducationInNumbersPage(id: string) {
    return {
      queryKey: ['ein', id],
      queryFn: () => educationInNumbersService.getEducationInNumbersPage(id),
    };
  },
  listLatestPages: {
    queryKey: ['ein'],
    queryFn: () => educationInNumbersService.listLatestPages(),
  },
});

export default educationInNumbersQueries;
