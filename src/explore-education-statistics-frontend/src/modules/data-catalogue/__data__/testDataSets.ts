import {
  DataSetFile,
  DataSetFileSummary,
} from '@frontend/services/dataSetFileService';

export const testDataSetFileSummaries: DataSetFileSummary[] = [
  {
    hasApiDataSet: true,
    id: 'datasetfile-id-1',
    fileExtension: 'csv',
    fileId: 'file-id-1',
    filename: 'file-name-1',
    fileSize: '100 kb',
    meta: {
      timePeriod: {
        timeIdentifier: 'Calendar year',
        from: '2010',
        to: '2020',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['National', 'Regional'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    latestData: true,
    publication: {
      id: 'publication-1',
      title: 'Publication 1',
    },
    published: new Date('2020-01-01'),
    release: {
      id: 'release-1',
      title: 'Release 1',
    },
    content: 'Data set summary 1',
    theme: {
      id: 'theme-1',
      title: 'Theme 1',
    },
    title: 'Data set 1',
  },
  {
    hasApiDataSet: false,
    id: 'datasetfile-id-2',
    fileExtension: 'csv',
    fileId: 'file-id-2',
    filename: 'file-name-2',
    fileSize: '100 kb',
    meta: {
      timePeriod: {
        timeIdentifier: 'Calendar year',
        from: '2010',
        to: '2020',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['National', 'Regional'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    latestData: true,
    publication: {
      id: 'publication-1',
      title: 'Publication 1',
    },
    published: new Date('2020-01-01'),
    release: {
      id: 'release-1',
      title: 'Release 1',
    },
    content: 'Data set summary 1',
    theme: {
      id: 'theme-2',
      title: 'Theme 2',
    },
    title: 'Data set 2',
  },
  {
    id: 'datasetfile-id-3',
    fileExtension: 'csv',
    fileId: 'file-id-3',
    filename: 'file-name-3',
    fileSize: '100 kb',
    meta: {
      timePeriod: {
        timeIdentifier: 'Calendar year',
        from: '2010',
        to: '2020',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['National', 'Regional'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    latestData: true,
    publication: {
      id: 'publication-2',
      title: 'Publication 2',
    },
    published: new Date('2020-01-01'),
    release: {
      id: 'release-1',
      title: 'Release 1',
    },
    content: 'Data set summary 1',
    theme: {
      id: 'theme-2',
      title: 'Theme 2',
    },
    title: 'Data set 3',
  },
];

export const testDataSet: DataSetFile = {
  id: 'datasetfile-id',
  file: { id: 'file-id', name: 'file name', size: 'file size' },
  release: {
    id: 'release-id',
    isLatestPublishedRelease: true,
    publication: {
      id: 'publication-id',
      slug: 'publication-slug',
      themeTitle: 'Theme 1',
      title: 'Publication 1',
    },
    published: new Date('2024-01-01'),
    slug: 'release-slug',
    title: 'Release 1',
    type: 'NationalStatistics',
  },
  summary: 'Data set 1 summary',
  title: 'Data set 1',
  meta: {
    timePeriod: {
      timeIdentifier: 'Calendar year',
      from: '2023',
      to: '2024',
    },
    filters: ['Filter 1', 'Filter 2'],
    geographicLevels: ['Local authority', 'National'],
    indicators: ['Indicator 1', 'Indicator 2'],
  },
};
