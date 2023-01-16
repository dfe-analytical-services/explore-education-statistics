/* eslint-disable no-console */
import { v4 } from 'uuid';
import spinner from '../utils/spinner';
import adminApi from '../utils/adminApi';

const { TOPIC_ID } = process.env;

const publicationService = {
  createPublication: async (): Promise<string> => {
    spinner.start();
    console.time('createPublication');
    const res = await adminApi.post('/api/publications', {
      title: `importer-testing-${v4()}`,
      topicId: TOPIC_ID,
      contact: {
        teamName: 'testing',
        teamEmail: 'johndoe@gmail.com',
        contactName: 'John Doe',
        contactTelNo: '123456789',
      },
      summary: 'testing',
    });
    console.timeEnd('createPublication');
    spinner.succeed(`Created publication ${res.data.id}`);
    return res.data.id;
  },
};
export default publicationService;
