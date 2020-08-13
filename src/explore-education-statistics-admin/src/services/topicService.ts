import client from '@admin/services/utils/service';

export interface Topic {
  id: string;
  title: string;
  slug: string;
  themeId: string;
}

export interface SaveTopicRequest {
  title: string;
  themeId: string;
}

export type CreateTopicRequest = SaveTopicRequest;
export type UpdateTopicRequest = SaveTopicRequest;

const topicService = {
  getTopic(topicId: string): Promise<Topic> {
    return client.get(`/topics/${topicId}`);
  },
  createTopic(topic: CreateTopicRequest): Promise<Topic> {
    return client.post('/topics', topic);
  },
  updateTopic(topicId: string, topic: UpdateTopicRequest): Promise<Topic> {
    return client.put(`/topics/${topicId}`, topic);
  },
};

export default topicService;
