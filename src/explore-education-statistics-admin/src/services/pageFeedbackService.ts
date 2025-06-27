import client from '@admin/services/utils/service';
import { PageFeedbackViewModel } from '@common/services/types/pageFeedback';

const pageFeedbackService = {
  listFeedback(showRead: string): Promise<PageFeedbackViewModel[]> {
    return showRead
      ? client.get('/feedback/page?showRead=true')
      : client.get('/feedback/page');
  },
  toggleReadStatus(id: string): Promise<void> {
    return client.patch(`/feedback/page/${id}`);
  },
};

export default pageFeedbackService;
