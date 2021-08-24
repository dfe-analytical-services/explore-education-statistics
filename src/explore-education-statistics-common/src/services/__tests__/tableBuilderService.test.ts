import tableBuilderService, {
  SubjectMeta,
  TableDataResponse,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { dataApi as _dataApi } from '@common/services/api';
import * as _combineMeasures from '@common/services/util/combineMeasuresWithDuplicateLocationCodes';

jest.mock('@common/services/api');
jest.mock('@common/services/util/combineMeasuresWithDuplicateLocationCodes');

const dataApi = _dataApi as jest.Mocked<typeof _dataApi>;
const combineMeasures = _combineMeasures as jest.Mocked<
  typeof _combineMeasures
>;

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

  test(
    'locations with duplicate levels and codes are merged in getTableData(), and ' +
      'combineMeasuresWithDuplicateLocationCodes() called to merge any table rows belonging to duplicate Locations',
    async () => {
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

      const combinedLocationResults: TableDataResult[] = [
        {
          geographicLevel: 'mock-response',
          location: {},
          filters: [],
          measures: {},
          timePeriod: '',
        },
      ];

      combineMeasures.default.mockReturnValue(combinedLocationResults);

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
        results: combinedLocationResults,
      };

      dataApi.post.mockResolvedValue(tableData);

      const response = await tableBuilderService.getTableData({
        releaseId: '',
        filters: [],
        subjectId: '',
        locations: {},
        indicators: [],
      });

      const deduplicatedLocations = [
        {
          level: 'provider',
          value: 'duplicate-provider',
          label: 'Duplicate Provider 1 / Duplicate Provider 2',
        },
      ];

      expect(combineMeasures.default).toHaveBeenCalledWith(
        tableData.results,
        deduplicatedLocations,
      );

      expect(response).toEqual(expectedTableData);
    },
  );

  test(
    'locations with duplicate levels and codes are merged in getDataBlockTableData(), and ' +
      'combineMeasuresWithDuplicateLocationCodes() called to merge any table rows belonging to duplicate Locations',
    async () => {
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

      const combinedLocationResults: TableDataResult[] = [
        {
          geographicLevel: 'mock-response',
          location: {},
          filters: [],
          measures: {},
          timePeriod: '',
        },
      ];

      combineMeasures.default.mockReturnValue(combinedLocationResults);

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
        results: combinedLocationResults,
      };

      dataApi.get.mockResolvedValue(tableData);

      const response = await tableBuilderService.getDataBlockTableData('', '');

      const deduplicatedLocations = [
        {
          level: 'provider',
          value: 'duplicate-provider',
          label: 'Duplicate Provider 1 / Duplicate Provider 2',
        },
      ];

      expect(combineMeasures.default).toHaveBeenCalledWith(
        tableData.results,
        deduplicatedLocations,
      );

      expect(response).toEqual(expectedTableData);
    },
  );
});
