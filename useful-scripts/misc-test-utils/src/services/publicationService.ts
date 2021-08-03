/* eslint-disable no-console */
import chalk from 'chalk';
import { v4 } from 'uuid';
import errorHandler from '../utils/errorHandler';
import adminApi from '../utils/adminApi';

const { TOPIC_ID } = process.env;

const publicationService = {
  // eslint-disable-next-line consistent-return
  createPublication: async () => {
    console.time('createPublication');
    try {
      const res = await adminApi.post('/api/publications', {
        title: `importer-testing-${v4()}`,
        topicId: TOPIC_ID,
        contact: {
          teamName: 'testing',
          teamEmail: 'johndoe@gmail.com',
          contactName: 'John Doe',
          contactTelNo: '123456789',
        },
      });
      console.timeEnd('createPublication');
      console.log(
        chalk.green(`Created publication. Status code ${res.status}`),
      );
      return res.data.id;
    } catch (e) {
      errorHandler(e);
    }
    return null;
  },
};
export default publicationService;
