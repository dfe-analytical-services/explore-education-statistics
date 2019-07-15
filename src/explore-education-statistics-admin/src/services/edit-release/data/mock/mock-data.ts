import { Dictionary } from '@common/types';
import { DataFileView } from '../types';

const dataFilesByReleaseId: Dictionary<DataFileView> = {
  'my-publication-1-release-1': {
    publicationTitle: 'asdf',
    dataFiles: [
      {
        title: 'Geographical absence',
        file: {
          id: 'file-1',
          fileName: 'absence_geoglevels.csv',
        },
        fileSize: {
          size: 61,
          unit: 'Mb',
        },
        numberOfRows: 212000,
        metadataFile: {
          id: 'metadata-file-1',
          fileName: 'meta_absence_geoglevels.csv',
        },
      },
      {
        title: 'Local authority',
        file: {
          id: 'file-2',
          fileName: 'absence_lacharacteristics.csv',
        },
        fileSize: {
          size: 66,
          unit: 'Mb',
        },
        numberOfRows: 240000,
        metadataFile: {
          id: 'metadata-file-2',
          fileName: 'meta_absence_lacharacteristics.csv',
        },
      },
      {
        title: 'National characteristics',
        file: {
          id: 'file-3',
          fileName: 'bsence_natcharacteristics.csv',
        },
        fileSize: {
          size: 71,
          unit: 'Mb',
        },
        numberOfRows: 320000,
        metadataFile: {
          id: 'metadata-file-3',
          fileName: 'meta_absence_natcharacteristics.csv',
        },
      },
    ],
  },
};

export default {
  getDataFilesForRelease: (releaseId: string) =>
    dataFilesByReleaseId[releaseId],
};
