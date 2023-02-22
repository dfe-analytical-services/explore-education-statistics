import { RefinedResponse } from 'k6/http';
import { AdminService } from './adminService';

type DataFileImportHandler = (
  adminService: AdminService,
  releaseId: string,
) => {
  response: RefinedResponse<'text'>;
  id: string;
};
const utils = {
  getDataFileUploadStrategy({
    filename,
  }: {
    filename: string;
  }): {
    filename: string;
    isZip: boolean;
    subjectName: string;
    getOrImportSubject: DataFileImportHandler;
  } {
    const isZip = filename.endsWith('.zip');
    const subjectName = filename;

    // eslint-disable-next-line no-restricted-globals
    const zipFile = isZip ? open(`admin/import/assets/${filename}`, 'b') : null;
    const subjectFile = !isZip
      ? // eslint-disable-next-line no-restricted-globals
        open(`admin/import/assets/${filename}`, 'b')
      : null;
    const subjectMetaFile = !isZip
      ? // eslint-disable-next-line no-restricted-globals
        open(
          `admin/import/assets/${filename.replace('.csv', '.meta.csv')}`,
          'b',
        )
      : null;

    return {
      isZip,
      filename,
      subjectName: filename,
      getOrImportSubject: (adminService, releaseId) =>
        isZip
          ? adminService.uploadDataZipFile({
              title: subjectName,
              releaseId,
              zipFile: {
                file: zipFile as ArrayBuffer,
                filename: `${subjectName}.zip`,
              },
            })
          : adminService.uploadDataFile({
              title: subjectName,
              releaseId,
              dataFile: {
                file: subjectFile as ArrayBuffer,
                filename: `${subjectName}.csv`,
              },
              metaFile: {
                file: subjectMetaFile as ArrayBuffer,
                filename: `${subjectName}.meta.csv`,
              },
            }),
    };
  },
};

export default utils;
