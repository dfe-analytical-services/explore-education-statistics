import client from '@admin/services/utils/service';

export interface Topic {
  id: string;
  title: string;
  slug: string;
  description: string;
  themeId: string;
}

const topicService = {
  getTopic(topicId: string): Promise<Topic> {
    return client.get(`/topics/${topicId}`);
  },
};

export default topicService;
