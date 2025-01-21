import client from '@admin/services/utils/service';
import { FeedbackViewModel } from '@common/services/types/feedback';

const feedbackService = {
  listFeedback(showRead: string): Promise<FeedbackViewModel[]> {
    return showRead
      ? client.get('/feedback?showRead=true')
      : client.get('/feedback');
  },
  toggleReadStatus(id: string): Promise<void> {
    return client.patch(`/feedback/${id}`);
  },
};

export default feedbackService;
