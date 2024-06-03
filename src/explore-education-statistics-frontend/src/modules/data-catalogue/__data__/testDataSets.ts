import { PaginatedList } from '@common/services/types/pagination';
import {
  ApiDataSet,
  ApiDataSetVersion,
} from '@frontend/services/apiDataSetService';
import {
  DataSetFile,
  DataSetFileSummary,
} from '@frontend/services/dataSetFileService';

export const testDataSetFileSummaries: DataSetFileSummary[] = [
  {
    api: {
      id: 'api-data-set-id-1',
      version: '1.0',
    },
    id: 'data-set-file-id-1',
    fileExtension: 'csv',
    fileId: 'file-id-1',
    filename: 'file-name-1',
    fileSize: '100 kb',
    meta: {
      timePeriodRange: {
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
    lastUpdated: '2023-12-01',
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
    id: 'data-set-file-2',
    fileExtension: 'csv',
    fileId: 'file-id-2',
    filename: 'file-name-2',
    fileSize: '100 kb',
    meta: {
      timePeriodRange: {
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
    lastUpdated: '2023-12-01',
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
    id: 'data-set-file-id-3',
    fileExtension: 'csv',
    fileId: 'file-id-3',
    filename: 'file-name-3',
    fileSize: '100 kb',
    meta: {
      timePeriodRange: {
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
    lastUpdated: '2023-12-01',
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

export const testDataSetFile: DataSetFile = {
  id: 'data-set-file-id',
  file: {
    id: 'file-id',
    name: 'file name',
    size: 'file size',
    meta: {
      timePeriodRange: {
        from: '2023',
        to: '2024',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['Local authority', 'National'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    dataCsvPreview: { headers: ['column_1'], rows: [['1']] },
    variables: [{ value: 'column_1', label: 'Column 1 is for something' }],
    subjectId: 'subject-id',
  },
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
    lastUpdated: '2023-12-01',
    slug: 'release-slug',
    title: 'Release 1',
    type: 'NationalStatistics',
  },
  summary: 'Data set 1 summary',
  title: 'Data set 1',
  footnotes: [{ id: 'footnote-1', label: 'Footnote 1' }],
};

export const testDataSetWithApi: DataSetFile = {
  ...testDataSetFile,
  api: {
    id: 'api-data-set-id',
    version: '1.0',
  },
};

export const testApiDataSetVersion: ApiDataSetVersion = {
  version: '1.0',
  type: 'Major',
  status: 'Published',
  published: '2024-05-13',
  notes: 'Test notes',
  totalResults: 1,
  timePeriods: { start: '2019', end: '2020' },
  geographicLevels: [],
  filters: [],
  indicators: [],
};

export const testApiDataSetVersions: PaginatedList<ApiDataSetVersion> = {
  paging: {
    page: 1,
    pageSize: 10,
    totalResults: 10,
    totalPages: 1,
  },
  results: [
    { ...testApiDataSetVersion, version: '2.0' },
    { ...testApiDataSetVersion, version: '1.2', status: 'Deprecated' },
    { ...testApiDataSetVersion, version: '1.0', status: 'Withdrawn' },
  ],
};

export const testApiDataSet: ApiDataSet = {
  id: 'api-data-set-id',
  title: 'Test title',
  summary: 'Test summary',
  status: 'Published',
  latestVersion: {
    version: '1.0',
    published: '2024-05-13',
    totalResults: 1,
    timePeriods: { start: '2019', end: '2020' },
    geographicLevels: [],
    filters: [],
    indicators: [],
  },
};
