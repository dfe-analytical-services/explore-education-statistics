import { createQueryKeys } from '@lukemorales/query-key-factory';
import pageFeedbackService from '@admin/services/pageFeedbackService';

const pageFeedbackQueries = createQueryKeys('release', {
  getFeedback(showRead: string) {
    return {
      queryKey: [showRead],
      queryFn: () => pageFeedbackService.listFeedback(showRead),
    };
  },
});

export default pageFeedbackQueries;
