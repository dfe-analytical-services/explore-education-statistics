/* eslint-disable no-console */
import chalk from 'chalk';
import { v4 } from 'uuid';
import spinner from '../../utils/spinner';
import publicationService from '../../services/publicationService';
import { projectRoot } from '../../config';
import releaseService from '../../services/releaseService';
import commonService from '../../services/commonService';
import ZipDirectory from '../../utils/zipDirectory';
import sleep from '../../utils/sleep';
import subjectService from '../../services/subjectService';

const cwd = projectRoot;

const { ADMIN_URL } = process.env;

const createReleaseAndPublish = async () => {
  spinner.start();
  await commonService.validateArchives();
  await commonService.prepareDirectories();

  type importStages = 'STARTED' | 'QUEUED' | 'COMPLETE' | '';
  let importStatus: importStages = '';

  const publicationId = await publicationService.createPublication();
  const releaseId = await releaseService.createRelease(publicationId);

  await commonService.extractZip();
  await commonService.renameFiles();
  await ZipDirectory(
    `${cwd}/test-files`,
    `${cwd}/zip-files/clean-test-zip-${v4()}.zip`,
  );
  if (!releaseId) {
    throw new Error(
      chalk.red(
        'No release ID returned from "createRelease" function! Exiting test with errors',
      ),
    );
  }
  const subjectId = await subjectService.addSubject(releaseId);
  console.time('import subject upload');
  if (!importStatus) {
    spinner.warn(
      'No importStatus just yet, waiting 4 seconds before polling again',
    );
    await sleep(4000);
  }

  while (importStatus !== 'COMPLETE') {
    spinner.info(`${chalk.blue('importStatus', chalk.green(importStatus))}`);
    // eslint-disable-next-line no-await-in-loop
    await sleep(1000);

    // eslint-disable-next-line no-await-in-loop
    importStatus = await subjectService.getSubjectProgress(
      releaseId,
      subjectId as string,
    );
  }
  console.timeEnd('import subject upload');

  const subjectArray = await subjectService.getSubjectIdArr(releaseId);

  await releaseService.addDataGuidance(
    subjectArray as { id: string; content: string }[],
    releaseId,
  );

  const finalReleaseObject = await releaseService.getRelease(releaseId);

  await releaseService.publishRelease(finalReleaseObject as never, releaseId);
  console.time('publication elapsed time');
  const url = `${ADMIN_URL}/publication/${publicationId}/release/${releaseId}/status`;

  await releaseService.getReleaseProgress(releaseId, url);
};
export default createReleaseAndPublish;
