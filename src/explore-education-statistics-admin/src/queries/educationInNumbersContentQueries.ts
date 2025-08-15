import educationInNumbersContentService from '@admin/services/educationInNumbersContentService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const educationInNumbersContentQueries = createQueryKeys(
  'educationInNumbersContent',
  {
    get(educationInNumbersPageId: string) {
      return {
        queryKey: [educationInNumbersPageId],
        queryFn: () =>
          educationInNumbersContentService.getEducationInNumbersPageContent(
            educationInNumbersPageId,
          ),
      };
    },
  },
);

export default educationInNumbersContentQueries;
