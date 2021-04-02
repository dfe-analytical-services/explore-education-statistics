/* eslint-disable */

import archiver from 'archiver';
import fs from 'fs';

/**
 *
 * @param {String} source
 * @param {String} out
 * @returns {Promise}
 */

const ZipDirectory = async (source: string, out: string) => {
  const archive = archiver('zip');
  const stream = fs.createWriteStream(out);

  return new Promise((resolve, reject) => {
    archive
      .directory(source, false)
      .on('error', err => reject(err))
      .pipe(stream);
    stream.on('close', () => resolve('Zipped file successfully'));
    archive.finalize();
  });
};

export default ZipDirectory;
