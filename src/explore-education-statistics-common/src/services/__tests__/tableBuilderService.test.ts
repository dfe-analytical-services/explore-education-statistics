import tableBuilderService, {
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { dataApi as _dataApi } from '@common/services/api';

jest.mock('@common/services/api');

const dataApi = _dataApi as jest.Mocked<typeof _dataApi>;

describe('tableBuilderService', () => {
  test('locations with duplicate levels and codes are merged in getSubjectMeta()', async () => {
    const originalMeta: SubjectMeta = {
      filters: {},
      timePeriod: {
        legend: '',
        options: [],
      },
      indicators: {},
      locations: {
        provider: {
          legend: '',
          options: [
            {
              value: 'unique-provider',
              label: 'Unique Provider',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 2',
            },
            {
              value: 'unique-provider-2',
              label: 'Unique Provider-2',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 1',
            },
          ],
        },
        'level-2': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
        },
        'level-3': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 3 Location',
            },
          ],
        },
      },
    };

    const expectedMeta: SubjectMeta = {
      filters: {},
      timePeriod: {
        legend: '',
        options: [],
      },
      indicators: {},
      locations: {
        provider: {
          legend: '',
          options: [
            {
              value: 'unique-provider',
              label: 'Unique Provider',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 1 / Duplicate Provider 2',
            },
            {
              value: 'unique-provider-2',
              label: 'Unique Provider-2',
            },
          ],
        },
        'level-2': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
        },
        'level-3': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 3 Location',
            },
          ],
        },
      },
    };

    dataApi.get.mockResolvedValue(originalMeta);

    const meta = await tableBuilderService.getSubjectMeta('');
    expect(meta).toEqual(expectedMeta);
  });

  test('locations with duplicate levels and codes are merged in filterSubjectMeta()', async () => {
    const originalMeta: SubjectMeta = {
      filters: {},
      timePeriod: {
        legend: '',
        options: [],
      },
      indicators: {},
      locations: {
        provider: {
          legend: '',
          options: [
            {
              value: 'unique-provider',
              label: 'Unique Provider',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 2',
            },
            {
              value: 'unique-provider-2',
              label: 'Unique Provider-2',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 1',
            },
          ],
        },
        'level-2': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
        },
        'level-3': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 3 Location',
            },
          ],
        },
      },
    };

    const expectedMeta: SubjectMeta = {
      filters: {},
      timePeriod: {
        legend: '',
        options: [],
      },
      indicators: {},
      locations: {
        provider: {
          legend: '',
          options: [
            {
              value: 'unique-provider',
              label: 'Unique Provider',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 1 / Duplicate Provider 2',
            },
            {
              value: 'unique-provider-2',
              label: 'Unique Provider-2',
            },
          ],
        },
        'level-2': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
        },
        'level-3': {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 3 Location',
            },
          ],
        },
      },
    };

    dataApi.post.mockResolvedValue(originalMeta);

    const meta = await tableBuilderService.filterSubjectMeta({
      subjectId: '',
    });
    expect(meta).toEqual(expectedMeta);
  });

  test('locations with duplicate levels and codes are merged in getTableData()', async () => {
    const tableData: TableDataResponse = {
      subjectMeta: {
        geoJsonAvailable: false,
        filters: {},
        footnotes: [],
        indicators: [],
        boundaryLevels: [],
        subjectName: '',
        timePeriodRange: [],
        publicationName: '',
        locations: [
          {
            level: 'provider',
            value: 'unique-provider',
            label: 'Unique Provider',
          },
          {
            level: 'provider',
            value: 'duplicate-provider',
            label: 'Duplicate Provider 2',
          },
          {
            level: 'provider',
            value: 'unique-provider-2',
            label: 'Unique Provider 2',
          },
          {
            level: 'provider',
            value: 'duplicate-provider',
            label: 'Duplicate Provider 1',
          },
          {
            level: 'level-2',
            value: 'duplicate-code-across-levels',
            label: 'Level 2 Location',
          },
          {
            level: 'level-3',
            value: 'duplicate-code-across-levels',
            label: 'Level 3 Location',
          },
        ],
      },
      results: [],
    };

    const expectedTableData: TableDataResponse = {
      subjectMeta: {
        geoJsonAvailable: false,
        filters: {},
        footnotes: [],
        indicators: [],
        boundaryLevels: [],
        subjectName: '',
        timePeriodRange: [],
        publicationName: '',
        locations: [
          {
            level: 'provider',
            value: 'unique-provider',
            label: 'Unique Provider',
          },
          {
            level: 'provider',
            value: 'duplicate-provider',
            label: 'Duplicate Provider 1 / Duplicate Provider 2',
          },
          {
            level: 'provider',
            value: 'unique-provider-2',
            label: 'Unique Provider 2',
          },
          {
            level: 'level-2',
            value: 'duplicate-code-across-levels',
            label: 'Level 2 Location',
          },
          {
            level: 'level-3',
            value: 'duplicate-code-across-levels',
            label: 'Level 3 Location',
          },
        ],
      },
      results: [],
    };

    dataApi.post.mockResolvedValue(tableData);

    const response = await tableBuilderService.getTableData({
      releaseId: '',
      filters: [],
      subjectId: '',
      locations: {},
      indicators: [],
    });
    expect(response).toEqual(expectedTableData);
  });

  test('locations with duplicate levels and codes are merged in getDataBlockTableData()', async () => {
    const tableData: TableDataResponse = {
      subjectMeta: {
        geoJsonAvailable: false,
        filters: {},
        footnotes: [],
        indicators: [],
        boundaryLevels: [],
        subjectName: '',
        timePeriodRange: [],
        publicationName: '',
        locations: [
          {
            level: 'provider',
            value: 'unique-provider',
            label: 'Unique Provider',
          },
          {
            level: 'provider',
            value: 'duplicate-provider',
            label: 'Duplicate Provider 2',
          },
          {
            level: 'provider',
            value: 'unique-provider-2',
            label: 'Unique Provider 2',
          },
          {
            level: 'provider',
            value: 'duplicate-provider',
            label: 'Duplicate Provider 1',
          },
          {
            level: 'level-2',
            value: 'duplicate-code-across-levels',
            label: 'Level 2 Location',
          },
          {
            level: 'level-3',
            value: 'duplicate-code-across-levels',
            label: 'Level 3 Location',
          },
        ],
      },
      results: [],
    };

    const expectedTableData: TableDataResponse = {
      subjectMeta: {
        geoJsonAvailable: false,
        filters: {},
        footnotes: [],
        indicators: [],
        boundaryLevels: [],
        subjectName: '',
        timePeriodRange: [],
        publicationName: '',
        locations: [
          {
            level: 'provider',
            value: 'unique-provider',
            label: 'Unique Provider',
          },
          {
            level: 'provider',
            value: 'duplicate-provider',
            label: 'Duplicate Provider 1 / Duplicate Provider 2',
          },
          {
            level: 'provider',
            value: 'unique-provider-2',
            label: 'Unique Provider 2',
          },
          {
            level: 'level-2',
            value: 'duplicate-code-across-levels',
            label: 'Level 2 Location',
          },
          {
            level: 'level-3',
            value: 'duplicate-code-across-levels',
            label: 'Level 3 Location',
          },
        ],
      },
      results: [],
    };

    dataApi.get.mockResolvedValue(tableData);

    const response = await tableBuilderService.getDataBlockTableData('', '');
    expect(response).toEqual(expectedTableData);
  });
});
