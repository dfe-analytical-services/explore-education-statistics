/* eslint-disable no-console */
import axios from 'axios';
import 'dotenv-safe/config';
import { v4 } from 'uuid';
import errorHandler from '../../../utils/errorHandler';

// disable insecure warnings
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const { JWT_TOKEN, ADMIN_URL } = process.env;

const createPublication = async () => {
  console.time('CreatePublication');
  try {
    const res = await axios({
      method: 'post',
      url: `${ADMIN_URL}/api/publications`,
      data: {
        title: `importer-testing-${v4()}`,
        topicId: '565c9fa0-08c6-4a9d-a193-08d8f9086d62',
        contact: {
          teamName: 'testing',
          teamEmail: 'johndoe@gmail.com',
          contactName: 'John Doe',
          contactTelNo: '123456789',
        },
      },
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${JWT_TOKEN}`,
      },
    });
    console.timeEnd('CreatePublication');
    console.log(`Created publication. Status code ${res.status}`);
    const publicationId = res.data.id;
    return publicationId;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const createRelease = async (publicationId: string) => {
  console.time('createRelease');
  try {
    const res = await axios({
      method: 'POST',
      url: `${ADMIN_URL}/api/publications/${publicationId}/releases`,
      data: {
        timePeriodCoverage: { value: 'AY' },
        releaseName: 2222,
        typeId: '1821abb8-68b0-431b-9770-0bea65d02ff0',
        publicationId: 'c76c1bc4-0a01-4116-74cd-08d8eebbf0c1',
        templateReleaseId: '',
      },
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${JWT_TOKEN}`,
      },
    });
    console.timeEnd('createRelease');
    const releaseId = res.data.id;
    console.log(
      `Release URL: ${ADMIN_URL}/publication/${publicationId}/release/${releaseId}/data`,
    );
    return releaseId;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};
const createSingleRelease = async () => {
  const publicationId: string = await createPublication();
  await createRelease(publicationId);
};
export default createSingleRelease;
