/* eslint-disable no-console */
import globby from 'globby';
import path from 'path';
import { v4 } from 'uuid';
import fs from 'fs';
import FormData from 'form-data';
import { projectRoot } from '../config';
import { SubjectData } from '../types/SubjectData';
import sleep from '../utils/sleep';
import adminApi from '../utils/adminApi';
import { importStages } from '../modules/subject/uploadSubject';
import spinner from '../utils/spinner';

const cwd = projectRoot;

const { SUBJECT_POLL_TIME } = process.env;

const subjectService = {
  addSubject: async (releaseId: string): Promise<string> => {
    spinner.start('Uploading subject \n');
    const finalZipFileGlob = await globby(
      `${cwd}/zip-files/clean-test-zip-*.zip`,
      {
        concurrency: 32,
      },
    );
    const oldPath = finalZipFileGlob[0];
    const newPath = path.resolve(oldPath, '..', path.basename(oldPath));

    fs.rename(oldPath, newPath, (err: NodeJS.ErrnoException | null): void => {
      if (err) throw err;
    });

    const form = new FormData();
    form.append('zipFile', fs.createReadStream(newPath));
    const res = await adminApi.post(
      `/api/release/${releaseId}/zip-data?title=importer-subject-${v4()}`,
      form,
      {
        headers: {
          ...form.getHeaders(),
        },
        maxContentLength: 1024 ** 1000000,
        maxBodyLength: 1024 ** 1000000,
      },
    );
    spinner.succeed(`Subject uploaded: (status: ${res.data.status})`);
    return res.data.id;
  },

  getSubjectIdArr: async (
    releaseId: string,
  ): Promise<{ id: string; content: string }[]> => {
    const res = await adminApi.get(`/api/release/${releaseId}/data-guidance`);
    const subjects: SubjectData[] = res.data?.subjects;
    const subjArr: { id: string; content: string }[] = [];
    subjects.forEach(subject => {
      subjArr.push({ id: subject.id, content: `Hello ${v4()}` });
    });
    return subjArr;
  },
  // eslint-disable-next-line consistent-return
  getSubjectProgress: async (
    releaseId: string,
    subjectId: string,
  ): Promise<importStages> => {
    spinner.start();
    const res = await adminApi.get(
      `/api/release/${releaseId}/data/${subjectId}/import/status`,
    );

    if (!res.data.status) {
      spinner.info(
        'No import status available. Waiting 3 seconds before polling the API',
      );
      await sleep(parseInt(SUBJECT_POLL_TIME, 10));
    }

    spinner.stop();
    return res.data.status;
  },
};
export default subjectService;
