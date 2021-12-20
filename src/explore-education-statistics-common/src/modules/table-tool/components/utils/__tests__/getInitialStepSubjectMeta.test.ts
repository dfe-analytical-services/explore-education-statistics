import getInitialStepSubjectMeta, {
  InitialStepSubjectMeta,
} from '@common/modules/table-tool/components/utils/getInitialStepSubjectMeta';
import _tableBuilderService, {
  ReleaseTableDataQuery,
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('getInitialStepSubjectMeta', () => {
  test('returns `initialStep` of 1 when query does not have a `releaseId`', async () => {
    const query: ReleaseTableDataQuery = {
      subjectId: '',
      filters: [],
      indicators: [],
      locations: {},
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 1,
    });
  });

  test('returns `initialStep` of 2 when query does not have a `subjectId`', async () => {
    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: '',
      filters: [],
      indicators: [],
      locations: {},
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 2,
    });
  });

  test('returns `initialStep` of 3 when query has no locations', async () => {
    const subjectMeta: SubjectMeta = {
      locations: {
        country: {
          legend: 'Country',
          options: [{ value: 'england', label: 'England' }],
        },
        localAuthority: {
          legend: 'Local authority',
          options: [{ value: 'sheffield', label: 'Sheffield' }],
        },
      },
      timePeriod: {
        legend: 'Time period',
        hint: '',
        options: [],
      },
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);

    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: 'subject-1',
      locations: {},
      indicators: [],
      filters: [],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 3,
      subjectMeta,
    });

    expect(tableBuilderService.getSubjectMeta).toHaveBeenCalledWith(
      'subject-1',
    );
  });

  test('returns `initialStep` of 3 when table data has not been provided', async () => {
    const subjectMeta: SubjectMeta = {
      locations: {},
      timePeriod: {
        legend: 'Time period',
        hint: '',
        options: [],
      },
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);

    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: 'subject-1',
      locations: {
        country: ['england'],
      },
      indicators: [],
      filters: [],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 3,
      subjectMeta,
    });

    expect(tableBuilderService.getSubjectMeta).toHaveBeenCalledWith(
      'subject-1',
    );
  });

  test('returns `initialStep` of 3 when table data does not have any results', async () => {
    const subjectMeta: SubjectMeta = {
      locations: {
        country: {
          legend: 'Country',
          options: [{ value: 'england', label: 'England' }],
        },
        localAuthority: {
          legend: 'Local authority',
          options: [{ value: 'sheffield', label: 'Sheffield' }],
        },
      },
      timePeriod: {
        legend: 'Time period',
        hint: '',
        options: [],
      },
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);

    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: 'subject-1',
      locations: {
        country: ['england'],
        localAuthority: ['sheffield', 'barnsley'],
      },
      indicators: [],
      filters: [],
    };

    const tableData: TableDataResponse = {
      results: [],
      subjectMeta: {
        publicationName: 'Test publication',
        subjectName: 'Test subject',
        geoJsonAvailable: false,
        footnotes: [],
        boundaryLevels: [],
        locationsHierarchical: {},
        timePeriodRange: [],
        indicators: [],
        filters: {},
      },
    };

    expect(await getInitialStepSubjectMeta(query, tableData)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 3,
      subjectMeta,
    });

    expect(tableBuilderService.getSubjectMeta).toHaveBeenCalledWith(
      'subject-1',
    );
  });

  test('returns `initialStep` of 6 when table can be rendered', async () => {
    const subjectMeta: SubjectMeta = {
      locations: {
        country: {
          legend: 'Country',
          options: [{ value: 'england', label: 'England' }],
        },
      },
      timePeriod: {
        legend: 'Time period',
        hint: '',
        options: [
          { year: 2018, code: 'AY', label: '2018' },
          { year: 2019, code: 'AY', label: '2019' },
          { year: 2020, code: 'AY', label: '2020' },
        ],
      },
      indicators: {
        indicatorGroup1: {
          label: 'Indicator group 1',
          options: [
            {
              label: 'Indicator 1',
              value: 'indicator-1',
              unit: '',
              name: 'indicator_1',
            },
          ],
        },
      },
      filters: {
        filter1: {
          legend: 'Filter 1',
          hint: '',
          name: 'filter_1',
          options: {
            filterGroup1: {
              label: 'Filter Group 1',
              options: [{ value: 'filter-item-1', label: 'Filter item 1' }],
            },
          },
        },
      },
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);

    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: 'subject-1',
      locations: {
        country: ['england'],
      },
      timePeriod: {
        startYear: 2018,
        startCode: 'AY',
        endYear: 2020,
        endCode: 'AY',
      },
      indicators: ['indicator-1'],
      filters: ['filter-item-1'],
    };

    const tableData: TableDataResponse = {
      results: [
        {
          timePeriod: '2018',
          measures: {
            'indicator-1': '123',
          },
          location: {
            country: {
              name: 'England',
              code: 'england',
            },
          },
          geographicLevel: 'country',
          filters: ['filter-item-1'],
        },
      ],
      subjectMeta: {
        publicationName: 'Test publication',
        subjectName: 'Test subject',
        geoJsonAvailable: false,
        footnotes: [],
        boundaryLevels: [],
        locationsHierarchical: {
          country: [{ value: 'england', label: 'England' }],
        },
        timePeriodRange: [{ year: 2018, code: 'AY', label: '2018' }],
        indicators: [
          {
            label: 'Indicator 1',
            value: 'indicator-1',
            unit: '',
            name: 'indicator_1',
          },
        ],
        filters: {
          filter1: {
            legend: 'Filter 1',
            hint: '',
            name: 'filter_1',
            options: {
              filterGroup1: {
                label: 'Filter Group 1',
                options: [{ value: 'filter-item-1', label: 'Filter item 1' }],
              },
            },
          },
        },
      },
    };

    expect(await getInitialStepSubjectMeta(query, tableData)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 6,
      subjectMeta,
    });
  });
});
