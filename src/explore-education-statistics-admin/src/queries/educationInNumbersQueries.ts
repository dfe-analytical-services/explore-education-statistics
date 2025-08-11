import { createQueryKeys } from '@lukemorales/query-key-factory';
import educationInNumbersService, {
  CreateEducationInNumbersPageRequest,
  UpdateEducationInNumbersPageRequest,
} from '@admin/services/educationInNumbersService';

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
  createEducationInNumbersPage(page: CreateEducationInNumbersPageRequest) {
    return {
      queryKey: [page.title], // @MarkFix yeah?
      queryFn: () =>
        educationInNumbersService.createEducationInNumbersPage(page),
    };
  },
  updateEducationInNumbersPage(
    educationInNumbersPageId: string,
    page: UpdateEducationInNumbersPageRequest,
  ) {
    return {
      queryKey: [educationInNumbersPageId], // @MarkFix yeah?
      queryFn: () =>
        educationInNumbersService.updateEducationInNumbersPage(
          educationInNumbersPageId,
          page,
        ),
    };
  },
});

export default educationInNumbersQueries;
