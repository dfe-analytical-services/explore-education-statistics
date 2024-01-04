import { DataSetSummary } from '@frontend/services/dataSetService';

// eslint-disable-next-line import/prefer-default-export
export const testDataSetSummaries: DataSetSummary[] = [
  {
    fileExtension: 'csv',
    fileId: 'file-id-1',
    filename: 'file-name-1',
    fileSize: '100 kb',
    filters: ['Filter 1', 'Filter 2'],
    geographicLevels: ['National', 'Regional'],
    indicators: ['Indicator 1', 'Indicator 2'],
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
    summary: 'Data set summary 1',
    theme: {
      id: 'theme-1',
      title: 'Theme 1',
    },
    timePeriods: {
      from: '2010',
      to: '2020',
    },
    title: 'Data set 1',
  },
  {
    fileExtension: 'csv',
    fileId: 'file-id-2',
    filename: 'file-name-2',
    fileSize: '100 kb',
    filters: ['Filter 1', 'Filter 2'],
    geographicLevels: ['National', 'Regional'],
    indicators: ['Indicator 1', 'Indicator 2'],
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
    summary: 'Data set summary 1',
    theme: {
      id: 'theme-1',
      title: 'Theme 1',
    },
    timePeriods: {
      from: '2010',
      to: '2020',
    },
    title: 'Data set 2',
  },
  {
    fileExtension: 'csv',
    fileId: 'file-id-3',
    filename: 'file-name-3',
    fileSize: '100 kb',
    filters: ['Filter 1', 'Filter 2'],
    geographicLevels: ['National', 'Regional'],
    indicators: ['Indicator 1', 'Indicator 2'],
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
    summary: 'Data set summary 1',
    theme: {
      id: 'theme-1',
      title: 'Theme 1',
    },
    timePeriods: {
      from: '2010',
      to: '2020',
    },
    title: 'Data set 3',
  },
];
