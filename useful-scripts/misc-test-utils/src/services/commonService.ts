/* eslint-disable no-console */
import chalk from 'chalk';
import StreamZip from 'node-stream-zip';
import globby from 'globby';
import fs from 'fs';
import rimraf from 'rimraf';
import { projectRoot } from '../config';
import spinner from '../utils/spinner';
import logger from '../utils/logger';

const cwd = projectRoot;

const commonService = {
  validateArchives: async () => {
    spinner.start('Validating archives');
    const zipGlob = await globby(`${cwd}/*.zip`);
    if (fs.existsSync(zipGlob[0]) && zipGlob[0].includes('archive.zip')) {
      logger.info(chalk.green('found zip file, continuing'));
    } else {
      throw new Error(
        'No zip file named archive.zip in root dir! Exiting test with failures',
      );
    }
    if (zipGlob[1]) {
      rimraf(zipGlob[1], () => {
        spinner.info('cleaned stale zip files');
      });
    }
    spinner.succeed('Validated archives');
  },

  prepareDirectories: async () => {
    spinner.start('Preparing directories \n');
    if (fs.existsSync(`${cwd}/test-files`)) {
      rimraf(`${cwd}/test-files/*`, () => {
        spinner.info('cleaned test-files directory');
      });
    }

    if (fs.existsSync(`${cwd}/zip-files`)) {
      rimraf(`${cwd}/zip-files/*`, () => {
        spinner.info('cleaned zip-files directory');
      });
    }

    if (!fs.existsSync(`${cwd}/test-files`)) {
      fs.mkdirSync(`${cwd}/test-files`);
    }

    if (!fs.existsSync(`${cwd}/zip-files`)) {
      fs.mkdirSync(`${cwd}/zip-files`);
    }
    spinner.succeed('Prepared directories');
  },
  extractZip: async () => {
    spinner.start('Extracting zip');
    const zippedFile = await globby(`${cwd}/*.zip`);
    const cleanZipFile = zippedFile[0];
    const archive = fs.existsSync(zippedFile[0]);
    if (archive) {
      // eslint-disable-next-line new-cap
      const zip = new StreamZip.async({ file: cleanZipFile });
      const count = await zip.extract(null, `${cwd}/test-files`);
      spinner.info(`Extracted ${count} files to test-files`);
      await zip.close();
    }
    spinner.succeed('Extracted zip');
  },
  renameFiles: async () => {
    spinner.start('Renaming files');
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
    spinner.succeed('Renamed files');
  },
};
export default commonService;
