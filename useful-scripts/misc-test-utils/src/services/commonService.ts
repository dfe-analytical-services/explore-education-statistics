/* eslint-disable no-console */
import chalk from 'chalk';
import StreamZip from 'node-stream-zip';
import globby from 'globby';
import fs from 'fs';
import rimraf from 'rimraf';
import { projectRoot } from '../config';

const cwd = projectRoot;

const commonService = {
  validateArchives: async () => {
    const zipGlob = await globby(`${cwd}/*.zip`);
    if (fs.existsSync(zipGlob[0]) && zipGlob[0].includes('archive.zip')) {
      console.log(chalk.green('found zip file, continuing'));
    } else {
      throw new Error(
        'No zip file named archive.zip in root dir! Exiting test with failures',
      );
    }
    if (zipGlob[1]) {
      rimraf(zipGlob[1], () => {
        console.log(chalk.green('cleaned stale zip files'));
      });
    }
  },

  prepareDirectories: async () => {
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
  },
  extractZip: async () => {
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
  },
  renameFiles: async () => {
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
  },
};
export default commonService;
