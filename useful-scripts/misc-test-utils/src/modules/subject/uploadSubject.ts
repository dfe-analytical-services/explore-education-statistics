/* eslint-disable no-console */
import chalk from 'chalk';
import { v4 } from 'uuid';
import { projectRoot } from '../../config';
import commonService from '../../services/commonService';
import ZipDirectory from '../../utils/zipDirectory';
import sleep from '../../utils/sleep';
import subjectService from '../../services/subjectService';
import releaseService from '../../services/releaseService';

const cwd = projectRoot;
export type importStages = 'STARTED' | 'QUEUED' | 'COMPLETE' | '';

const uploadSingleSubject = async (releaseId: string) => {
  if (releaseId === '') {
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
  await sleep(3000);

  console.time('import subject upload');
  await sleep(900);

  const subjectArray = await subjectService.getSubjectIdArr(releaseId);

  await releaseService.addDataGuidance(
    subjectArray as { id: string; content: string }[],
    releaseId,
  );

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
    importStatus = await subjectService.getSubjectProgress(
      releaseId,
      subjectId as string,
    );
  }
  console.timeEnd('import subject upload');
};
export default uploadSingleSubject;
