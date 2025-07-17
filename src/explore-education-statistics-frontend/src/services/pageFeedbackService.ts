import { contentApi } from '@common/services/api';
import { PageFeedbackRequest } from '@common/services/types/pageFeedback';

const pageFeedbackService = {
  sendFeedback(feedback: PageFeedbackRequest): Promise<void> {
    return contentApi.post('/feedback/page', feedback);
  },
};

export default pageFeedbackService;
