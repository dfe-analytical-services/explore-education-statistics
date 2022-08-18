/* eslint-disable no-console */
import { randomInt } from 'crypto';
import chalk from 'chalk';
import spinner from '../utils/spinner';
import adminApi from '../utils/adminApi';

const id = randomInt(1, 30);

const topicService = {
  renameTopic: async (topicId: string, themeId: string): Promise<string> => {
    spinner.start(`Renaming topic with ID ${chalk.blue(topicId)}`);
    const res = await adminApi.put(`/api/topics/${topicId}`, {
      title: `UI test topic-${id}`,
      summary: 'test',
      slug: `ui-test-topic-${id}`,
      themeId,
    });
    spinner.succeed(`Renamed topic with ID ${chalk.blue(topicId)}`);
    return res.data.id;
  },
  deleteTopic: async (topicId: string): Promise<void> => {
    spinner.start(`Deleting topic with ID ${chalk.blue(topicId)}`);
    await adminApi.delete(`/api/topics/${topicId}`);
    spinner.succeed(`Deleted topic with ID ${chalk.blue(topicId)}`);
  },
};
export default topicService;
