import { AdhocFile, DataFile } from '@admin/services/edit-release/data/types';
import { Dictionary } from '@common/types';

export interface DataFileView {
  dataFiles: DataFile[];
}

export interface AdhocFileView {
  adhocFiles: AdhocFile[];
}

const dataFilesByReleaseId: Dictionary<DataFileView> = {
  'my-publication-1-release-1': {
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
          fileName: 'absence_natcharacteristics.csv',
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

const adhocFilesByReleaseId: Dictionary<AdhocFileView> = {
  'my-publication-1-release-1': {
    adhocFiles: [
      {
        title: 'Custom chart',
        file: {
          id: 'file-1',
          fileName: 'custom-chart.png',
        },
        fileSize: {
          size: 1.2,
          unit: 'Mb',
        },
      },
      {
        title: 'Design diagram',
        file: {
          id: 'file-2',
          fileName: 'custom-chart.jpg',
        },
        fileSize: {
          size: 3.5,
          unit: 'Mb',
        },
      },
    ],
  },
};

export default {
  getDataFilesForRelease: (releaseId: string) =>
    dataFilesByReleaseId[releaseId],

  getAdhocFilesForRelease: (releaseId: string) =>
    adhocFilesByReleaseId[releaseId],
};
