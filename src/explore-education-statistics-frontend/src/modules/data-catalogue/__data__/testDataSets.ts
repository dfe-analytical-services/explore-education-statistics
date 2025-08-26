import { PaginatedList } from '@common/services/types/pagination';
import {
  ApiDataSet,
  ApiDataSetVersion,
} from '@frontend/services/apiDataSetService';
import {
  DataSetCsvPreview,
  DataSetFile,
  DataSetFootnote,
  DataSetFileSummary,
  DataSetVariable,
} from '@frontend/services/dataSetFileService';

export const testDataSetCsvPreview: DataSetCsvPreview = {
  headers: ['time_period', 'geographic_level', 'filter_1', 'indicator_1'],
  rows: [
    ['201819', 'National', 'filter_1_value', '100'],
    ['201920', 'National', 'filter_1_value', '101'],
    ['202021', 'National', 'filter_1_value', '102'],
    ['202122', 'National', 'filter_1_value', '103'],
    ['202223', 'National', 'filter_1_value', '104'],
  ],
};

export const testDataSetVariables: DataSetVariable[] = [
  {
    label: 'Filter 1 label',
    value: 'filter_1',
  },
  {
    label: 'Filter 2 label',
    value: 'filter_2',
  },
  {
    label: 'Indicator 1 label',
    value: 'indicator_1',
  },
  {
    label: 'Indicator 2 label',
    value: 'indicator_2',
  },
  {
    label: 'Indicator 3 label',
    value: 'indicator_3',
  },
  {
    label: 'Indicator 4 label',
    value: 'indicator_4',
  },
];

export const testDataSetFootnotes: DataSetFootnote[] = [
  {
    id: 'footnote-1',
    label: 'Footnote 1',
  },
  {
    id: 'footnote-2',
    label: 'Footnote 2',
  },
];

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
      numDataFileRows: 11,
      timePeriodRange: {
        from: '2010',
        to: '2020',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['National', 'Regional', 'Local authority'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    latestData: true,
    isSuperseded: false,
    publication: {
      id: 'publication-1',
      title: 'Publication 1',
      slug: 'publication-slug',
    },
    published: new Date('2020-01-01'),
    lastUpdated: '2023-12-01',
    release: {
      id: 'release-1',
      title: 'Release 1',
      slug: 'release-slug',
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
      numDataFileRows: 12,
      timePeriodRange: {
        from: '2010',
        to: '2020',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['National', 'Regional'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    latestData: true,
    isSuperseded: false,
    publication: {
      id: 'publication-1',
      title: 'Publication 1',
      slug: 'publication-slug',
    },
    published: new Date('2020-01-01'),
    lastUpdated: '2023-12-01',
    release: {
      id: 'release-1',
      title: 'Release 1',
      slug: 'release-slug',
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
      numDataFileRows: 13,
      timePeriodRange: {
        from: '2010',
        to: '2020',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['National', 'Regional'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    latestData: true,
    isSuperseded: false,
    publication: {
      id: 'publication-2',
      title: 'Publication 2',
      slug: 'publication-2-slug',
    },
    published: new Date('2020-01-01'),
    lastUpdated: '2023-12-01',
    release: {
      id: 'release-1',
      title: 'Release 1',
      slug: 'release-slug',
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
      numDataFileRows: 65,
      timePeriodRange: {
        from: '2023',
        to: '2024',
      },
      filters: ['Filter 1', 'Filter 2'],
      geographicLevels: ['Local authority', 'National'],
      indicators: ['Indicator 1', 'Indicator 2'],
    },
    dataCsvPreview: testDataSetCsvPreview,
    variables: testDataSetVariables,
    subjectId: 'subject-id',
  },
  release: {
    id: 'release-id',
    isLatestPublishedRelease: true,
    isSuperseded: false,
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
    type: 'AccreditedOfficialStatistics',
  },
  summary: 'Data set 1 summary',
  title: 'Data set 1',
  footnotes: testDataSetFootnotes,
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
  file: {
    id: 'file-id',
  },
  release: {
    title: 'Release title',
    slug: 'release-slug',
  },
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
    {
      ...testApiDataSetVersion,
      version: '2.0',
      file: {
        id: 'file-1-id',
      },
      release: {
        title: 'Release 1 title',
        slug: 'release-1-slug',
      },
    },
    {
      ...testApiDataSetVersion,
      version: '1.2',
      status: 'Deprecated',
      file: {
        id: 'file-2-id',
      },
      release: {
        title: 'Release 2 title',
        slug: 'release-2-slug',
      },
    },
    {
      ...testApiDataSetVersion,
      version: '1.0',
      status: 'Withdrawn',
      file: {
        id: 'file-3-id',
      },
      release: {
        title: 'Release 3 title',
        slug: 'release-3-slug',
      },
    },
  ],
};

export const testPatchApiDataSetVersions: PaginatedList<ApiDataSetVersion> = {
  paging: {
    page: 1,
    pageSize: 10,
    totalResults: 10,
    totalPages: 1,
  },
  results: [
    {
      ...testApiDataSetVersion,
      version: '2.0.2',
      file: {
        id: 'file-1-id',
      },
      release: {
        title: 'Release 1 title',
        slug: 'release-1-slug',
      },
    },
    {
      ...testApiDataSetVersion,
      version: '2.0.1',
      file: {
        id: 'file-1-id',
      },
      release: {
        title: 'Release 1 title',
        slug: 'release-1-slug',
      },
    },
    {
      ...testApiDataSetVersion,
      version: '2.0',
      file: {
        id: 'file-1-id',
      },
      release: {
        title: 'Release 1 title',
        slug: 'release-1-slug',
      },
    },
    {
      ...testApiDataSetVersion,
      version: '1.1',
      status: 'Deprecated',
      file: {
        id: 'file-2-id',
      },
      release: {
        title: 'Release 2 title',
        slug: 'release-2-slug',
      },
    },
    {
      ...testApiDataSetVersion,
      version: '1.0',
      status: 'Withdrawn',
      file: {
        id: 'file-3-id',
      },
      release: {
        title: 'Release 3 title',
        slug: 'release-3-slug',
      },
    },
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
    file: {
      id: 'data-set-file-id',
    },
  },
};
