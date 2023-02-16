import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import {
  TableDataQuery,
  SelectedPublication,
} from '@common/services/tableBuilderService';
import { Release } from '@common/services/publicationService';

export const testQuery: TableDataQuery = {
  publicationId: '536154f5-7f82-4dc7-060a-08d9097c1945',
  subjectId: '1f1b1780-a607-454e-b331-08d9097c40f5',
  indicators: [
    '18a27dde-e54e-46d0-6656-08d9097c4255',
    '6240d58e-c160-4c39-6657-08d9097c4255',
  ],
  filters: [
    'bfd88241-1130-4df8-9e49-a411618d082f',
    '3f223187-a2aa-420c-a3d8-e2a94f77e4b5',
  ],
  locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
  timePeriod: {
    startYear: 2020,
    startCode: 'W23',
    endYear: 2020,
    endCode: 'W26',
  },
};

export const testTable: FullTable = {
  subjectMeta: {
    filters: {
      Date: {
        name: 'date',
        options: [
          new CategoryFilter({
            value: '3f223187-a2aa-420c-a3d8-e2a94f77e4b5',
            label: '02/06/2020',
            group: 'Default',
            isTotal: false,
            category: 'Date',
          }),
          new CategoryFilter({
            value: 'bfd88241-1130-4df8-9e49-a411618d082f',
            label: '02/04/2021',
            group: 'Default',
            isTotal: false,
            category: 'Date',
          }),
        ],
        order: 0,
      },
    },
    footnotes: [],
    indicators: [
      new Indicator({
        value: '18a27dde-e54e-46d0-6656-08d9097c4255',
        label: 'Number of open settings',
        unit: '',
        name: 'open_settings',
      }),
      new Indicator({
        value: '6240d58e-c160-4c39-6657-08d9097c4255',
        label: 'Proportion of settings open',
        unit: '%',
        decimalPlaces: 0,
        name: 'proportion_of_settings_open',
      }),
    ],
    locations: [
      new LocationFilter({
        id: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
        value: 'england',
        label: 'England',
        level: 'country',
      }),
    ],
    boundaryLevels: [
      {
        id: 1,
        label:
          'Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
      },
    ],
    publicationName: 'Test publication',
    subjectName: 'dates',
    timePeriodRange: [
      new TimePeriodFilter({
        label: '2020 Week 23',
        year: 2020,
        code: 'W23',
        order: 0,
      }),
      new TimePeriodFilter({
        label: '2020 Week 24',
        year: 2020,
        code: 'W24',
        order: 1,
      }),
      new TimePeriodFilter({
        label: '2020 Week 25',
        year: 2020,
        code: 'W25',
        order: 2,
      }),
      new TimePeriodFilter({
        label: '2020 Week 26',
        year: 2020,
        code: 'W26',
        order: 3,
      }),
    ],
    geoJsonAvailable: true,
  },
  results: [
    {
      filters: ['3f223187-a2aa-420c-a3d8-e2a94f77e4b5'],
      geographicLevel: 'country',
      locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
      measures: {
        '18a27dde-e54e-46d0-6656-08d9097c4255': '22500',
        '6240d58e-c160-4c39-6657-08d9097c4255': '0.9',
      },
      timePeriod: '2020_W23',
    },
    {
      filters: ['bfd88241-1130-4df8-9e49-a411618d082f'],
      geographicLevel: 'country',
      locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
      measures: {
        '18a27dde-e54e-46d0-6656-08d9097c4255': '18700',
        '6240d58e-c160-4c39-6657-08d9097c4255': '0.76',
      },
      timePeriod: '2021_W14',
    },
  ],
};

export const testTableHeaders: TableHeadersConfig = {
  columnGroups: [],
  rowGroups: [
    [
      new Indicator({
        value: '18a27dde-e54e-46d0-6656-08d9097c4255',
        label: 'Number of open settings',
        unit: '',
        name: 'open_settings',
      }),
      new Indicator({
        value: '6240d58e-c160-4c39-6657-08d9097c4255',
        label: 'Proportion of settings open',
        unit: '%',
        decimalPlaces: 0,
        name: 'proportion_of_settings_open',
      }),
    ],
  ],
  columns: [
    new CategoryFilter({
      value: 'bfd88241-1130-4df8-9e49-a411618d082f',
      label: '02/04/2021',
      group: 'Default',
      isTotal: false,
      category: 'Date',
    }),
    new CategoryFilter({
      value: '3f223187-a2aa-420c-a3d8-e2a94f77e4b5',
      label: '02/06/2020',
      group: 'Default',
      isTotal: false,
      category: 'Date',
    }),
  ],
  rows: [
    new TimePeriodFilter({
      label: '2020 Week 23',
      year: 2020,
      code: 'W23',
      order: 0,
    }),
    new TimePeriodFilter({
      label: '2020 Week 24',
      year: 2020,
      code: 'W24',
      order: 1,
    }),
    new TimePeriodFilter({
      label: '2020 Week 25',
      year: 2020,
      code: 'W25',
      order: 2,
    }),
    new TimePeriodFilter({
      label: '2020 Week 26',
      year: 2020,
      code: 'W26',
      order: 3,
    }),
  ],
};

export const testSelectedPublicationWithLatestRelease: SelectedPublication = {
  id: '536154f5-7f82-4dc7-060a-08d9097c1945',
  title: 'Test publication',
  slug: 'test-publication',
  selectedRelease: {
    id: 'latest-release-id',
    latestData: true,
    slug: 'latest-release-slug',
    title: 'Latest Release Title',
  },
  latestRelease: {
    title: 'Latest Release Title',
  },
};

export const testSelectedPublicationWithNonLatestRelease: SelectedPublication = {
  id: '536154f5-7f82-4dc7-060a-08d9097c1945',
  title: 'Test publication',
  slug: 'test-publication',
  selectedRelease: {
    id: 'selected-release-id',
    latestData: false,
    slug: 'selected-release-slug',
    title: 'Selected Release Title',
  },
  latestRelease: {
    title: 'Latest Release Title',
  },
};

export const testPublicationRelease: Release = {
  id: '',
  title: '',
  yearTitle: '',
  coverageTitle: '',
  releaseName: '',
  published: '',
  slug: '',
  summarySection: {
    id: '',
    order: 0,
    heading: '',
    content: [],
  },
  keyStatistics: [],
  keyStatisticsSecondarySection: {
    id: '',
    order: 0,
    heading: '',
    content: [],
  },
  headlinesSection: {
    id: '',
    order: 0,
    heading: '',
    content: [],
  },
  relatedDashboardsSection: {
    id: '',
    order: 0,
    heading: '',
    content: [],
  },
  publication: {
    id: '',
    slug: '',
    title: '',
    releases: [],
    legacyReleases: [],
    topic: {
      theme: {
        title: '',
      },
    },
    contact: {
      teamName: 'The team name',
      teamEmail: 'team@name.com',
      contactName: 'A person',
      contactTelNo: '012345',
    },
    methodologies: [
      {
        id: 'm1',
        title: 'methodology title',
        slug: 'm1',
      },
    ],
  },
  latestRelease: true,
  relatedInformation: [],
  type: 'NationalStatistics',
  updates: [],
  content: [],
  downloadFiles: [],
  hasPreReleaseAccessList: true,
  hasDataGuidance: true,
};
