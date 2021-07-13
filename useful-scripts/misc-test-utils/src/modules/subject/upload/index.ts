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
import errorHandler from '../../../utils/errorHandler';

// disable insecure warnings
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const { ADMIN_URL, JWT_TOKEN } = process.env;
const cwd = process.cwd();

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

const addSubject = async (releaseId: string) => {
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

const getSubjectProgress = async (releaseId: string, subjectId: string) => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${ADMIN_URL}/api/release/${releaseId}/data/${subjectId}/import/status`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    return res.data.status;
  } catch (e) {
    errorHandler(e);
  }
  return null;
};

const getSubjectIdArr = async (releaseId: string) => {
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

const UploadSingleSubject = async (releaseId: string) => {
  if (releaseId === '') {
    throw new Error(chalk.red('Release ID is required!'));
  }
  const zipGlob = await globby(`${cwd}/*.zip`);
  if (fs.existsSync(zipGlob[0]) && zipGlob[0].includes('archive.zip')) {
    console.log(chalk.green('found zip file, continuing'));
  } else {
    throw new Error(
      chalk.red(
        'No zip file named archive.zip in root dir! Exiting test with failures',
      ),
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

  await extractZip();
  await renameFiles();
  await ZipDirectory(
    `${cwd}/test-files`,
    `${cwd}/zip-files/clean-test-zip-${v4()}.zip`,
  );
  const subjectId = await addSubject(releaseId);
  console.time('import subject upload');
  while (importStatus !== 'COMPLETE') {
    console.log(chalk.blue('importStatus', importStatus));
    // eslint-disable-next-line no-await-in-loop
    await Sleep(300);
    // eslint-disable-next-line no-await-in-loop
    importStatus = await getSubjectProgress(releaseId, subjectId);
  }
  console.timeEnd(chalk.green('import subject upload'));

  try {
    const subjArr = await getSubjectIdArr(releaseId);
    if (!subjArr) {
      throw new Error(
        chalk.red(
          'No subject array returned from `getSubjectIdArr` function!, existing test with failures',
        ),
      );
    }
    await addMetaGuidance(subjArr, releaseId);
    console.timeEnd(chalk.green('publication elapsed time'));
  } catch (e) {
    errorHandler(e);
  }
};
export default UploadSingleSubject;
