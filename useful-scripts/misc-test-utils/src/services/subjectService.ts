/* eslint-disable no-console */
import globby from 'globby';
import path from 'path';
import { v4 } from 'uuid';
import fs from 'fs';
import FormData from 'form-data';
import chalk from 'chalk';
import { projectRoot } from '../config';
import errorHandler from '../utils/errorHandler';
import { SubjectData } from '../types/SubjectData';
import sleep from '../utils/sleep';
import adminApi from '../utils/adminApi';

const cwd = projectRoot;

const subjectService = {
  addSubject: async (releaseId: string): Promise<string | null> => {
    const finalZipFileGlob = await globby(
      `${cwd}/zip-files/clean-test-zip-*.zip`,
    );
    const oldPath = finalZipFileGlob[0];
    const newPath = path.resolve(oldPath, '..', path.basename(oldPath));
    fs.renameSync(oldPath, newPath);
    const form = new FormData();
    form.append('zipFile', fs.createReadStream(newPath));
    try {
      const res = await adminApi.post(
        `/api/release/${releaseId}/zip-data?title=importer-subject-${v4()}`,
        form,
        {
          headers: {
            ...form.getHeaders(),
          },
        },
      );
      console.log(res.data);
      return res.data.id;
    } catch (e) {
      errorHandler(e);
    }
    return null;
  },

  getSubjectIdArr: async (
    releaseId: string,
  ): Promise<{ id: string; content: string }[] | null> => {
    try {
      const res = await adminApi.get(`/api/release/${releaseId}/meta-guidance`);
      const subjects: SubjectData[] = res.data?.subjects;
      const subjArr: { id: string; content: string }[] = [];
      subjects.forEach(subject => {
        subjArr.push({ id: subject.id, content: `Hello ${v4()}` });
      });
      return subjArr;
    } catch (e) {
      errorHandler(e);
    }
    return null;
  },
  // eslint-disable-next-line consistent-return
  getSubjectProgress: async (releaseId: string, subjectId: string) => {
    try {
      const res = await adminApi.get(
        `/api/release/${releaseId}/data/${subjectId}/import/status`,
      );
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
  },
};
export default subjectService;
