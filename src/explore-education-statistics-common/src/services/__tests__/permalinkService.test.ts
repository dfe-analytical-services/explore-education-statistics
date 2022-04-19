import { dataApi as _dataApi } from '@common/services/api';
import permalinkService, { Permalink } from '@common/services/permalinkService';

jest.mock('@common/services/api');

const dataApi = _dataApi as jest.Mocked<typeof _dataApi>;

describe('permalinkService', () => {
  describe('merging of locations in permalink table data', () => {
    const permalink: Permalink = {
      id: '',
      created: '',
      configuration: {
        tableHeaders: {
          columnGroups: [],
          columns: [],
          rowGroups: [],
          rows: [],
        },
      },
      fullTable: {
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
                value: 'unique-provider-1',
                label: 'Unique Provider 1',
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
                code: 'unique-provider-1',
                name: 'Unique Provider 1',
              },
            },
          },
        ],
      },
      status: 'Current',
    };

    const expectedPermalink: Permalink = {
      id: '',
      created: '',
      configuration: {
        tableHeaders: {
          columnGroups: [],
          columns: [],
          rowGroups: [],
          rows: [],
        },
      },
      fullTable: {
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
                value: 'unique-provider-1',
                label: 'Unique Provider 1',
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
                code: 'unique-provider-1',
                name: 'Unique Provider 1',
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
      },
      status: 'Current',
    };

    test('locations returned in `getPermalink` response are deduplicated', async () => {
      dataApi.get.mockResolvedValue(permalink);

      const response = await permalinkService.getPermalink('');

      expect(response).toEqual(expectedPermalink);
    });
  });
});
