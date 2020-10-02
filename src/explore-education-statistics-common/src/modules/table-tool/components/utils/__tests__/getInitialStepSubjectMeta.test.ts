import getInitialStepSubjectMeta, {
  InitialStepSubjectMeta,
} from '@common/modules/table-tool/components/utils/getInitialStepSubjectMeta';
import _tableBuilderService, {
  ReleaseTableDataQuery,
  SubjectMeta,
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

  test('returns `initialStep` of 3 when query has locations and there are none in subject meta', async () => {
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

  test('returns `initialStep` of 3 when query has locations and some are missing from subject meta', async () => {
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

  test('returns `initialStep` of 4 when query has no time periods', async () => {
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
        options: [],
      },
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

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
      initialStep: 4,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(1);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
  });

  test('returns `initialStep` of 4 when query has time periods and there are none in subject meta', async () => {
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
        options: [],
      },
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: 'subject-1',
      locations: {
        country: ['england'],
      },
      timePeriod: {
        startYear: 2016,
        startCode: 'AY',
        endYear: 2020,
        endCode: 'AY',
      },
      indicators: [],
      filters: [],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 4,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(1);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
  });

  test('returns `initialStep` of 4 when query has time periods and some are missing from subject meta', async () => {
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
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

    const query: ReleaseTableDataQuery = {
      releaseId: 'release-1',
      subjectId: 'subject-1',
      locations: {
        country: ['england'],
      },
      timePeriod: {
        startYear: 2016,
        startCode: 'AY',
        endYear: 2020,
        endCode: 'AY',
      },
      indicators: [],
      filters: [],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 4,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(1);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
  });

  test('returns `initialStep` of 5 when query has indicators and there are none in subject meta', async () => {
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
      indicators: {},
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

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
      indicators: ['indicator-1', 'indicator-2'],
      filters: [],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 5,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(2);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
      timePeriod: query.timePeriod,
    });
  });

  test('returns `initialStep` of 5 when query has indicators and some are missing from subject meta', async () => {
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
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

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
      indicators: ['indicator-1', 'indicator-2'],
      filters: [],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 5,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(2);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
      timePeriod: query.timePeriod,
    });
  });

  test('returns `initialStep` of 5 when query has filters and there are none in subject meta', async () => {
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
      filters: {},
    };

    tableBuilderService.getSubjectMeta.mockResolvedValue(subjectMeta);
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

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
      filters: ['filter-1', 'filter-2'],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 5,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(2);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
      timePeriod: query.timePeriod,
    });
  });

  test('returns `initialStep` of 5 when query has filters and some are missing from subject meta', async () => {
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
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

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
      filters: ['filter-item-1', 'filter-item-2'],
    };

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 5,
      subjectMeta,
    });

    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledTimes(2);
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
    });
    expect(tableBuilderService.filterSubjectMeta).toHaveBeenCalledWith({
      subjectId: 'subject-1',
      locations: query.locations,
      timePeriod: query.timePeriod,
    });
  });

  test('returns `initialStep` of 6 when entire query is valid', async () => {
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
    tableBuilderService.filterSubjectMeta.mockResolvedValue(subjectMeta);

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

    expect(await getInitialStepSubjectMeta(query)).toEqual<
      InitialStepSubjectMeta
    >({
      initialStep: 6,
      subjectMeta,
    });
  });
});
