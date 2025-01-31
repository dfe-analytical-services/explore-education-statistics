/* eslint-disable no-await-in-loop */
/* eslint-disable no-console */
import { v4 } from 'uuid';
import chalk from 'chalk';
import { projectRoot } from '../../config';
import commonService from '../../services/commonService';
import ZipDirectory from '../../utils/zipDirectory';
import subjectService from '../../services/subjectService';
import releaseService from '../../services/releaseService';
import sleep from '../../utils/sleep';
import spinner from '../../utils/spinner';
import logger from '../../utils/logger';

const cwd = projectRoot;
const { SUBJECT_POLL_TIME } = process.env;

export type importStages = 'STARTED' | 'QUEUED' | 'COMPLETE' | '';

const uploadSingleSubject = async (releaseId: string, fast: boolean) => {
  spinner.start();
  if (!releaseId) {
    throw new Error(chalk.red('Release ID is required!'));
  }
  await commonService.validateArchives();
  await commonService.prepareDirectories();

  let importStatus: importStages = '';

  await commonService.extractZip();
  await commonService.renameFiles();
  await ZipDirectory(
    `${cwd}/test-files`,
    `${cwd}/zip-files/clean-test-zip-${v4()}.zip`,
  );

  const subjectId = await subjectService.addSubject(releaseId);

  console.time('import subject upload');

  const subjectArray = await subjectService.getSubjectIdArr(releaseId);

  await releaseService.addDataGuidance(
    subjectArray as { id: string; content: string }[],
    releaseId,
  );

  while (importStatus !== 'QUEUED') {
    importStatus = await subjectService.getSubjectProgress(
      releaseId,
      subjectId as string,
    );
  }

  if (!fast) {
    while (importStatus !== 'COMPLETE') {
      logger.info(chalk.blue('importStatus', chalk.green(importStatus)));
      importStatus = await subjectService.getSubjectProgress(
        releaseId,
        subjectId as string,
      );
      await sleep(parseInt(SUBJECT_POLL_TIME as string, 10));
    }
  }
  console.timeEnd('import subject upload');
  spinner.succeed('Subject uploaded');
};
export default uploadSingleSubject;
