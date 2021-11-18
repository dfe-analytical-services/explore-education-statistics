/* eslint-disable no-console */
import { random } from 'faker';
import adminApi from '../utils/adminApi';

const id = random.alphaNumeric(10);

const topicService = {
  renameTopic: async (topicId: string, themeId: string): Promise<string> => {
    const res = await adminApi.put(`/api/topics/${topicId}`, {
      title: `UI test topic-${id}`,
      summary: 'test',
      slug: `ui-test-topic-${id}`,
      themeId,
    });
    return res.data.id;
  },
  deleteTopic: async (topicId: string): Promise<void> => {
    await adminApi.delete(`/api/topics/${topicId}`);
  },
};
export default topicService;
