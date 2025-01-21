import { createQueryKeys } from '@lukemorales/query-key-factory';
import feedbackService from '@admin/services/feedbackService';

const feedbackQueries = createQueryKeys('release', {
  getFeedback(showRead: string) {
    return {
      queryKey: [showRead],
      queryFn: () => feedbackService.listFeedback(showRead),
    };
  },
});

export default feedbackQueries;
