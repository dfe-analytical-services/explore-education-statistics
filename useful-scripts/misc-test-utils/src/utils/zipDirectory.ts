import archiver from 'archiver';
import fs from 'fs';

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
