/* eslint-disable no-console */
import axios from 'axios';
import 'dotenv-safe/config';
import { v4 } from 'uuid';
import fs from 'fs';
import StreamZip from 'node-stream-zip';
import globby from 'globby';
import rimraf from 'rimraf';
import FormData from 'form-data';
import chalk from 'chalk';
import ZipDirectory from '../../../utils/zipDirectory';
import Sleep from '../../../utils/Sleep';
import { SubjectArray } from '../../../interfaces/SubjectArray';
import { ReleaseData } from '../../../interfaces/ReleaseData';
import errorHandler from '../../../utils/errorHandler';

// disable insecure warnings
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const { ADMIN_URL, TOPIC_ID, JWT_TOKEN } = process.env;
const cwd = process.cwd();

type finalReleaseData = {
  id: string;
  title: string;
  slug: string;
  publicationId: string;
  publicationTitle: string;
  publicationSlug: string;
  releaseName: string;
  yearTitle: string;
  typeId: string;
  live: string;
  timePeriodCoverage: {
    value: string;
    label: string;
  };
  latestRelease: string;
  type: {
    id: string;
    title: string;
  };
  contact: {
    id: string;
    teamName: string;
    teamEmail: string;
    contactName: string;
    contactTelNo: string;
  };
  status: string;
  amendment: string;
  latestInternalReleaseNote: string;
  publishMethod: string;
};

const createPublication = async () => {
  console.time('CreatePublication');
  try {
    const res = await axios({
      method: 'POST',
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
    console.log(chalk.green(`Created publication. Status code ${res.status}`));
    const publicationId = res.data.id;
    return publicationId;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const createRelease = async (publicationId: string): Promise<string | null> => {
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
      chalk.green(
        `Release URL: ${ADMIN_URL}/publication/${publicationId}/release/${releaseId}/data`,
      ),
    );
    return releaseId;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const extractZip = async () => {
  const zippedFile = await globby(`${cwd}/*.zip`);
  const cleanZipFile = zippedFile[0];
  const archive = fs.existsSync(zippedFile[0]);
  if (archive) {
    // eslint-disable-next-line new-cap
    const zip = new StreamZip.async({ file: cleanZipFile });
    const count = await zip.extract(null, `${cwd}/test-files`);
    console.log(chalk.green(`Extracted ${count} entries to test-files`));
    await zip.close();
  }
};

const renameFiles = async () => {
  // define a random id to assign to the renamed data & meta file...
  const randomId = Math.floor(Math.random() * 1000000);

  // rename csv file..
  const csvGlob1 = await globby(`${cwd}/test-files/*.csv`);

  const csvGlob2 = csvGlob1[0];

  fs.rename(`${csvGlob2}`, `${cwd}/test-files/testfile-${randomId}.csv`, e =>
    e ? console.error(chalk.red(e)) : '',
  );

  // rename the meta file...
  const metaGlob = await globby(`${cwd}/test-files/*.meta.csv`);
  const metaGlob2 = metaGlob[0];

  fs.rename(
    `${metaGlob2}`,
    `${cwd}/test-files/testfile-${randomId}.meta.csv`,
    e => (e ? console.error(chalk.red(e)) : ''),
  );
};

const addSubject = async (releaseId: string): Promise<string | null> => {
  const glob = await globby(`${cwd}/zip-files/*.zip`);

  // potential issue here with how windows paths are formatted in git bash
  if (glob !== undefined) {
    const oldPath = `${process.cwd()}/zip-files/${
      fs.readdirSync(`${process.cwd()}/zip-files`)[0]
    }`;

    const newPath = `${process.cwd()}/${
      fs.readdirSync(`${process.cwd()}/zip-files`)[0]
    }`;
    fs.renameSync(oldPath, newPath);
  }

  const rootGlob = await globby(`${cwd}/clean-test-zip-*.zip`);
  const form = new FormData();
  form.append('zipFile', fs.createReadStream(rootGlob[0]));

  try {
    const res = await axios({
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
      method: 'POST',
      url: `${ADMIN_URL}/api/release/${releaseId}/zip-data?name=importer-subject-${v4()}`,
      data: form,
      headers: {
        ...form.getHeaders(),
        Authorization: `Bearer ${JWT_TOKEN}`,
      },
    });
    const subjectId = res.data.id;
    return subjectId;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const getSubjectProgress = async (
  releaseId: string,
  subjectId: string,
  // eslint-disable-next-line consistent-return
) => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${ADMIN_URL}/api/release/${releaseId}/data/${subjectId}/import/status`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    if (!res.data.status) {
      console.info(
        chalk.cyan(
          'No import status available. Waiting 3 seconds before polling the API',
        ),
      );
      await Sleep(3000);
    }
    return res.data.status;
  } catch (e) {
    errorHandler(e);
  }
};

const getSubjectIdArr = async (
  releaseId: string,
): Promise<{ id: string; content: string }[] | null> => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${ADMIN_URL}/api/release/${releaseId}/meta-guidance`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    const subjects: SubjectArray[] = res.data?.subjects;
    const subjArr: { id: string; content: string }[] = [];
    subjects.forEach(sub => {
      subjArr.push({ id: `${sub.id}`, content: `Hello ${v4()}` });
    });
    return subjArr;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const addMetaGuidance = async (
  subjArr: { id: string; content: string }[],
  releaseId: string,
) => {
  try {
    await axios({
      method: 'PATCH',
      url: `${ADMIN_URL}/api/release/${releaseId}/meta-guidance`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
      data: {
        content: '<p>testing</p>',
        subjects: subjArr,
      },
    });
  } catch (e) {
    errorHandler(e);
  }
};

const getFinalReleaseDetails = async (releaseId: string) => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${ADMIN_URL}/api/releases/${releaseId}`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    const releaseData: ReleaseData = res.data;
    await Sleep(1000);

    const obj: finalReleaseData = {
      id: releaseData.id,
      title: releaseData.title,
      slug: releaseData.slug,
      publicationId: releaseData.publicationId,
      publicationTitle: releaseData.publicationTitle,
      publicationSlug: releaseData.publicationSlug,
      releaseName: releaseData.releaseName,
      yearTitle: releaseData.yearTitle,
      typeId: releaseData.typeId,
      live: 'false',
      timePeriodCoverage: {
        value: releaseData.timePeriodCoverage.value,
        label: releaseData.timePeriodCoverage.label,
      },
      latestRelease: 'false',
      type: {
        id: releaseData.type.id,
        title: releaseData.type.title,
      },
      contact: {
        id: releaseData.contact.id,
        teamName: releaseData.contact.teamName,
        teamEmail: releaseData.contact.teamEmail,
        contactName: releaseData.contact.contactName,
        contactTelNo: releaseData.contact.contactTelNo,
      },
      status: 'Approved',
      amendment: 'false',
      latestInternalReleaseNote: 'Approved by publisher testing',
      publishMethod: 'Immediate',
    };
    return obj;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const publishRelease = async (obj: finalReleaseData, releaseId: string) => {
  try {
    await axios({
      method: 'PUT',
      url: `${ADMIN_URL}/api/releases/${releaseId}`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
      data: obj,
    });
  } catch (e) {
    errorHandler(e);
  }
};

const getPublicationProgress = async (releaseId: string) => {
  try {
    const res = await axios({
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
      method: 'GET',
      url: `${ADMIN_URL}/api/releases/${releaseId}/status`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'content-Type': 'application/json',
      },
    });
    return res.data?.overallStage;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const createReleaseAndPublish = async () => {
  const zipGlob = await globby(`${cwd}/*.zip`);
  if (fs.existsSync(zipGlob[0]) && zipGlob[0].includes('archive.zip')) {
    console.log(chalk.green('found zip file, continuing'));
  } else {
    throw new Error(
      'No zip file named archive.zip in root dir! Exiting test with failures',
    );
  }

  if (fs.existsSync(`${cwd}/test-files`)) {
    rimraf(`${cwd}/test-files/*`, () => {
      console.log(chalk.green('cleaned test-files folder'));
    });
  }

  if (fs.existsSync(`${cwd}/zip-files`)) {
    rimraf(`${cwd}/zip-files/*`, () => {
      console.log(chalk.green('cleaned zip-files folder'));
    });
  }
  if (!fs.existsSync(`${cwd}/test-results`)) {
    fs.mkdirSync(`${cwd}/test-results`);
  }
  if (!fs.existsSync(`${cwd}/test-files`)) {
    fs.mkdirSync(`${cwd}/test-files`);
  }

  if (!fs.existsSync(`${cwd}/zip-files`)) {
    fs.mkdirSync(`${cwd}/zip-files`);
  }

  if (zipGlob[1]) {
    rimraf(zipGlob[1], () => {
      console.log(chalk.green('cleaned stale zip files'));
    });
  }

  type importStages = 'STARTED' | 'QUEUED' | 'COMPLETE' | '';
  let importStatus: importStages = '';

  const publicationId = await createPublication();
  const releaseId = await createRelease(publicationId);
  await extractZip();
  await renameFiles();
  await ZipDirectory(
    `${cwd}/test-files`,
    `${cwd}/zip-files/clean-test-zip-${v4()}.zip`,
  );
  if (!releaseId) {
    throw new Error(
      chalk.red(
        'No release ID returned from `createRelease` function! Exiting test with errors',
      ),
    );
  }
  const subjectId = await addSubject(releaseId);
  console.time('import subject upload');
  if (!importStatus) {
    console.info(
      'No importStatus just yet, waiting 3 seconds before polling again',
    );
    await Sleep(3000);
  }

  while (importStatus !== 'COMPLETE') {
    console.log(chalk.blue('importStatus', importStatus));
    // eslint-disable-next-line no-await-in-loop
    await Sleep(300);

    // eslint-disable-next-line no-await-in-loop
    importStatus = await getSubjectProgress(releaseId, subjectId as string);
  }
  console.timeEnd('import subject upload');

  try {
    type publicationStages = 'notStarted' | 'Started' | 'Complete' | '';
    let publicationStatus: publicationStages = '';
    const subjectArray = await getSubjectIdArr(releaseId);
    if (!subjectArray) {
      throw new Error(
        chalk.red(
          'No subjectArray object returned from `getSubjectArr` function! Exiting test with failures',
        ),
      );
    }
    await addMetaGuidance(subjectArray, releaseId);
    const finalReleaseObject = await getFinalReleaseDetails(releaseId);
    await publishRelease(finalReleaseObject, releaseId);
    console.time('publication elapsed time');
    while (publicationStatus !== 'Complete') {
      console.log('publicationStatus', publicationStatus);
      // eslint-disable-next-line no-await-in-loop
      if (publicationStatus !== undefined) {
        // eslint-disable-next-line no-await-in-loop
        publicationStatus = await getPublicationProgress(releaseId);
      }
      // eslint-disable-next-line no-await-in-loop
      await Sleep(1500);
    }
    console.timeEnd('publication elapsed time');
  } catch (e) {
    errorHandler(e);
  }
};
export default createReleaseAndPublish;
