import tableBuilderService, {
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { dataApi as _dataApi } from '@common/services/api';

jest.mock('@common/services/api');

const dataApi = _dataApi as jest.Mocked<typeof _dataApi>;

describe('tableBuilderService', () => {
  describe('merging of locations in subject meta', () => {
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
        level2: {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
        },
        level3: {
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
        level2: {
          legend: '',
          options: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
        },
        level3: {
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

    test('locations returned by `getSubjectMeta` are deduplicated', async () => {
      dataApi.get.mockResolvedValue(originalMeta);
      const meta = await tableBuilderService.getSubjectMeta('');
      expect(meta).toEqual(expectedMeta);
    });

    test('locations returned by `filterSubjectMeta` are deduplicated', async () => {
      dataApi.post.mockResolvedValue(originalMeta);
      const meta = await tableBuilderService.filterSubjectMeta({
        subjectId: '',
      });
      expect(meta).toEqual(expectedMeta);
    });
  });

  describe('merging of locations in table data', () => {
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
        locations: {
          provider: [
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
              label: 'Unique Provider 2',
            },
            {
              value: 'duplicate-provider',
              label: 'Duplicate Provider 1',
            },
          ],
          level2: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
          level3: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 3 Location',
            },
          ],
        },
      },
      results: [
        {
          filters: [],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '10',
            'indicator-2': '20',
            'indicator-3': '30',
          },
          location: {
            provider: {
              code: 'duplicate-provider',
              name: 'Duplicate Provider 1',
            },
          },
        },
        {
          filters: [],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '40',
            'indicator-2': '50',
            'indicator-3': '60',
          },
          location: {
            provider: {
              code: 'duplicate-provider',
              name: 'Duplicate Provider 2',
            },
          },
        },
        {
          filters: [],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '70',
            'indicator-2': '80',
            'indicator-3': '90',
          },
          location: {
            provider: {
              code: 'unique-provider',
              name: 'Unique Provider',
            },
          },
        },
      ],
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
        locations: {
          provider: [
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
              label: 'Unique Provider 2',
            },
          ],
          level2: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 2 Location',
            },
          ],
          level3: [
            {
              value: 'duplicate-code-across-levels',
              label: 'Level 3 Location',
            },
          ],
        },
      },
      results: [
        {
          filters: [],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '70',
            'indicator-2': '80',
            'indicator-3': '90',
          },
          location: {
            provider: {
              code: 'unique-provider',
              name: 'Unique Provider',
            },
          },
        },
        {
          filters: [],
          geographicLevel: 'provider',
          timePeriod: '',
          measures: {
            'indicator-1': '50',
            'indicator-2': '70',
            'indicator-3': '90',
          },
          location: {
            provider: {
              code: 'duplicate-provider',
              name: 'Duplicate Provider 1 / Duplicate Provider 2',
            },
          },
        },
      ],
    };

    test('locations returned by `getTableData` are deduplicated', async () => {
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

    test('locations returned by `getDataBlockTableData` are deduplicated', async () => {
      dataApi.get.mockResolvedValue(tableData);

      const response = await tableBuilderService.getDataBlockTableData('', '');

      expect(response).toEqual(expectedTableData);
    });
  });
});
