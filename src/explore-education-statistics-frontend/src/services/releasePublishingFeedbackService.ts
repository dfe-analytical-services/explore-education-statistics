import { contentApi } from '@common/services/api';
import { ReleasePublishingFeedbackRequest } from '@common/services/types/releasePublishingFeedback';

const releasePublishingFeedbackService = {
  sendFeedback(feedback: ReleasePublishingFeedbackRequest): Promise<void> {
    return contentApi.post('/feedback/release-publishing', feedback);
  },
};

export default releasePublishingFeedbackService;
