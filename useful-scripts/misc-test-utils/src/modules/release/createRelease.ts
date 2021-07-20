/* eslint-disable no-console */
import axios from 'axios';
import { v4 } from 'uuid';
import errorHandler from '../../utils/errorHandler';

const { JWT_TOKEN, ADMIN_URL, TOPIC_ID } = process.env;

const createPublication = async () => {
  console.time('CreatePublication');
  try {
    const res = await axios({
      method: 'post',
      url: `${ADMIN_URL}/api/publications`,
      data: {
        title: `importer-testing-${v4()}`,
        topicId: TOPIC_ID,
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

    return res.data.id;
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
const createPublicationAndRelease = async () => {
  const publicationId: string = await createPublication();
  await createRelease(publicationId);
};
export default createPublicationAndRelease;
