/* eslint-disable */
import axios from 'axios';
import 'dotenv-safe/config';
import { v4 } from 'uuid';
import fs from 'fs';
import StreamZip from 'node-stream-zip';
import globby from 'globby';
import rimraf from 'rimraf';
import FormData from 'form-data';
import { ReleaseDataProps, SubjectArrayT } from 'types';
import ZipDirectory from './utils/ZipDirectory';
import Sleep from './utils/Sleep';

// disable insecure warnings
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const { JWT_TOKEN, API_URL, TOPIC_ID } = process.env;

const createPublication = async () => {
  console.time('CreatePublication');
  try {
    const res = await axios({
      method: 'post',
      url: `${API_URL}/api/publications`,
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
    const publicationId = res.data.id;
    return publicationId;
  } catch (e) {
    console.error(e);
  }
};

const createRelease = async (publicationId: string) => {
  console.time('createRelease');
  try {
    const res = await axios({
      method: 'POST',
      url: `${API_URL}/api/publications/${publicationId}/releases`,
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
      `Release URL: ${API_URL}/publication/${publicationId}/release/${releaseId}/data`,
    );
    return releaseId;
  } catch (e) {
    console.error(e);
  }
};

const extractZip = async () => {
  const zippedFile = await globby('*.zip');
  const cleanZipFile = zippedFile[0];
  const archive = fs.existsSync(zippedFile[0]);
  console.log('archive', archive);
  if (archive) {
    const zip = new StreamZip.async({ file: cleanZipFile });
    const count = await zip.extract(null, './test-files');
    console.log(`Extracted ${count} entries to test-files`);
    await zip.close();
  }
};

const renameFiles = async () => {
  // define a random id to assign to the renamed data & meta file...
  const randomId = Math.floor(Math.random() * 1000000);

  // rename csv file..
  const csvGlob1 = await globby('./test-files/*.csv');
  const csvGlob2 = csvGlob1[0];

  fs.rename(`${csvGlob2}`, `./test-files/testfile-${randomId}.csv`, e =>
    e ? console.error(e) : '',
  );

  // rename the meta file...
  const metaGlob = await globby('./test-files/*.meta.csv');
  const metaGlob2 = metaGlob[0];

  fs.rename(`${metaGlob2}`, `./test-files/testfile-${randomId}.meta.csv`, e =>
    e ? console.error(e) : '',
  );
};

const addSubject = async (releaseId: string) => {
  const glob = await globby('./zip-files/*.zip');
  if (glob !== undefined) {
    const oldPath =
      __dirname + '\\zip-files\\' + fs.readdirSync(__dirname + '/zip-files')[0];
    const newPath =
      __dirname + '\\' + fs.readdirSync(__dirname + '/zip-files')[0];
    fs.renameSync(oldPath, newPath);
  }
  const rootGlob = await globby('clean-test-zip-*.zip');
  const form = new FormData();
  form.append('zipFile', fs.createReadStream(rootGlob[0]));

  try {
    const res = await axios({
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
      method: 'POST',
      url: `${API_URL}/api/release/${releaseId}/zip-data?name=importer-subject-${v4()}`,
      data: form,
      headers: {
        ...form.getHeaders(),
        Authorization: `Bearer ${JWT_TOKEN}`,
      },
    });
    console.log(res.data);
    const subjectId = res.data.id;
    return subjectId;
  } catch (e) {
    console.error(e);
  }
};

const getSubjectProgress = async (releaseId: string, subjectId: string) => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${API_URL}/api/release/${releaseId}/data/${subjectId}/import/status`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    return res.data.status;
  } catch (e) {
    console.error(e);
  }
};

const getSubjectIdArr = async (releaseId: string) => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${API_URL}/api/release/${releaseId}/meta-guidance`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    const subjects: SubjectArrayT[] = res.data?.subjects;
    let subjArr: { id: string; content: string }[] = [];
    subjects.forEach(sub => {
      subjArr.push({ id: `${sub.id}`, content: `Hello ${v4()}` });
    });
    return subjArr;
  } catch (e) {
    console.error(e);
  }
  return;
};

const addMetaGuidance = async (
  subjArr: { id: string; content: string }[],
  releaseId: string,
) => {
  try {
    await axios({
      method: 'PATCH',
      url: `${API_URL}/api/release/${releaseId}/meta-guidance`,
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
    console.error(e);
  }
};

const getFinalReleaseDetails = async (releaseId: string) => {
  try {
    const res = await axios({
      method: 'GET',
      url: `${API_URL}/api/releases/${releaseId}`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
    });
    const releaseData: ReleaseDataProps = res.data;
    await Sleep(1000);
    const obj = {
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
      internalReleaseNote: 'Approved by publisher testing',
      publishMethod: 'Immediate',
    };
    return obj;
  } catch (e) {
    console.error(e);
  }
  return;
};

const publishRelease = async (obj: any, releaseId: string) => {
  try {
    await axios({
      method: 'PUT',
      url: `${API_URL}/api/releases/${releaseId}`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'Content-Type': 'application/json',
      },
      data: obj,
    });
  } catch (e) {
    console.error(e);
  }
};

const getPublicationProgress = async (releaseId: string) => {
  try {
    const res = await axios({
      maxContentLength: Infinity,
      maxBodyLength: Infinity,
      method: 'GET',
      url: `${API_URL}/api/releases/${releaseId}/status`,
      headers: {
        Authorization: `Bearer ${JWT_TOKEN}`,
        'content-Type': 'application/json',
      },
    });
    return res.data?.overallStage;
  } catch (e) {
    console.error(e);
  }
};

(async () => {
  const zipGlob = await globby('*.zip');
  if (fs.existsSync(zipGlob[0]) && zipGlob[0] === 'archive.zip') {
    console.log('found zip file, continuing');
  } else {
    throw new Error(
      'No zip file named archive.zip in root dir! Exiting test with failures',
    );
  }

  rimraf(`./test-files/*`, () => {
    console.log('cleaned test directory');
  });

  rimraf(`./zip-files/*`, () => {
    console.log('cleaned zip file directory');
  });

  if (!fs.existsSync('./test-results')) {
    fs.mkdirSync('./test-results');
  }
  if (!fs.existsSync('./test-files')) {
    fs.mkdirSync('./test-files');
  }

  if (!fs.existsSync('./zip-files')) {
    fs.mkdirSync('./zip-files');
  }

  if (zipGlob[1]) {
    console.log(zipGlob[1]);

    rimraf(zipGlob[1], () => {
      console.log('removed stale zip files');
    });
  }

  type importStages = 'STARTED' | 'QUEUED' | 'COMPLETE' | '';
  let publicationId: string;
  let releaseId: string;
  let subjectId: string;
  let importStatus: importStages = '';

  publicationId = await createPublication();
  releaseId = await createRelease(publicationId);
  await extractZip();
  await renameFiles();
  await ZipDirectory('./test-files', `./zip-files/clean-test-zip-${v4()}.zip`);
  subjectId = await addSubject(releaseId);
  console.time('import subject upload');
  while (importStatus !== 'COMPLETE') {
    console.log('importStatus', importStatus);
    await Sleep(300);
    importStatus = await getSubjectProgress(releaseId, subjectId);
  }
  console.timeEnd('import subject upload');

  if (process.argv[2] !== undefined && process.argv[2].includes('publisher')) {
    try {
      let subjArr;
      let obj;
      type publicationStages = 'notStarted' | 'Started' | 'Complete' | '';
      let publicationStatus: publicationStages = '';
      subjArr = await getSubjectIdArr(releaseId);
      await addMetaGuidance(subjArr!, releaseId);
      obj = await getFinalReleaseDetails(releaseId);
      await publishRelease(obj, releaseId);
      console.time('publication elapsed time');
      while (publicationStatus !== 'Complete') {
        console.log('publicationStatus', publicationStatus);
        publicationStatus = await getPublicationProgress(releaseId);
        await Sleep(1500);
      }
      console.timeEnd('publication elapsed time');
    } catch (e) {
      console.error(e);
    }
  }
})();
