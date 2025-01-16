import { contentApi } from '@common/services/api';
import { FeedbackRequest } from '@common/services/types/feedback';

const feedbackService = {
  sendFeedback(feedback: FeedbackRequest): Promise<void> {
    return contentApi.post('/feedback', feedback);
  },
};

export default feedbackService;
