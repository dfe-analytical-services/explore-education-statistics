/* eslint-disable no-console */
import axios from 'axios';
import chalk from 'chalk';
import FormData from 'form-data';
import fs from 'fs';
import globby from 'globby';
import StreamZip from 'node-stream-zip';
import path from 'path';
import rimraf from 'rimraf';
import { v4 } from 'uuid';
import { projectRoot } from '../../config';
import { ReleaseData } from '../../types/ReleaseData';
import { SubjectData } from '../../types/SubjectData';
import errorHandler from '../../utils/errorHandler';
import sleep from '../../utils/sleep';
import ZipDirectory from '../../utils/zipDirectory';

// disable insecure warnings
const { ADMIN_URL, TOPIC_ID, JWT_TOKEN } = process.env;
const cwd = projectRoot;

const createPublication = async () => {
  console.time('createPublication');

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

    console.timeEnd('createPublication');
    console.log(chalk.green(`Created publication. Status code ${res.status}`));

    return res.data.id;
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
  const finalZipFileGlob = await globby(
    `${cwd}/zip-files/clean-test-zip-*.zip`,
  );

  const oldPath = finalZipFileGlob[0];

  const newPath = path.resolve(oldPath, '..', path.basename(oldPath));
  fs.renameSync(oldPath, newPath);

  const form = new FormData();
  form.append('zipFile', fs.createReadStream(newPath));

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

    return res.data.id;
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
      await sleep(3000);
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
    const subjects: SubjectData[] = res.data?.subjects;
    const subjArr: { id: string; content: string }[] = [];
    subjects.forEach(sub => {
      subjArr.push({ id: sub.id, content: `Hello ${v4()}` });
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
): Promise<boolean> => {
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
    return true;
  } catch (e) {
    errorHandler(e);
  }
  return false;
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

    await sleep(1000);

    return {
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
      preReleaseAccessList: '',
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
      approvalstatus: 'Approved',
      amendment: 'false',
      latestInternalReleaseNote: 'Approved by publisher testing',
      publishMethod: 'Immediate',
    };
  } catch (e) {
    errorHandler(e);
    return undefined;
  }
};

const publishRelease = async (obj: ReleaseData, releaseId: string) => {
  try {
    await axios({
      method: 'POST',
      url: `${ADMIN_URL}/api/releases/${releaseId}/status`,
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
      url: `${ADMIN_URL}/api/releases/${releaseId}/stage-status`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'content-Type': 'application/json',
      },
    });
    // eslint-disable-next-line no-constant-condition
    while (res.data.overallStage !== 'Complete') {
      console.log(
        chalk.blue('overall stage'),
        chalk.green(res.data.overallStage),
      );
      console.log(chalk.blue('data stage'), chalk.green(res.data.dataStage));
      console.log(
        chalk.blue('content stage'),
        chalk.green(res.data.contentStage),
      );
      console.log(chalk.blue('files stage'), chalk.green(res.data.filesStage));
      console.log(
        chalk.blue('publishing stage'),
        chalk.green(res.data.publishingStage),
      );
      // eslint-disable-next-line no-await-in-loop
      await sleep(3000);
      // eslint-disable-next-line no-await-in-loop
      await getPublicationProgress(releaseId);
    }
    return res.data;
  } catch (e) {
    return errorHandler(e);
  }
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
    console.log(
      'No importStatus just yet, waiting 4 seconds before polling again',
    );
    await sleep(4000);
  }

  while (importStatus !== 'COMPLETE') {
    console.log(chalk.blue('importStatus', chalk.green(importStatus)));
    // eslint-disable-next-line no-await-in-loop
    await sleep(1000);

    // eslint-disable-next-line no-await-in-loop
    importStatus = await getSubjectProgress(releaseId, subjectId as string);
  }
  console.timeEnd('import subject upload');

  const subjectArray = await getSubjectIdArr(releaseId);

  await addMetaGuidance(
    subjectArray as { id: string; content: string }[],
    releaseId,
  );
  const finalReleaseObject = await getFinalReleaseDetails(releaseId);

  await publishRelease(finalReleaseObject as never, releaseId);
  console.time('publication elapsed time');
  console.log(
    chalk.green(
      `Started publication of release: ${ADMIN_URL}/publication/${publicationId}/release/${releaseId}/status`,
    ),
  );

  await getPublicationProgress(releaseId);
};
export default createReleaseAndPublish;
