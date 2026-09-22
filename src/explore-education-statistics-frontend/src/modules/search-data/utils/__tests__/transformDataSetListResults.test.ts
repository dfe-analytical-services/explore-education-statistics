/* eslint-disable no-restricted-syntax */
import { AzureDataSetIndexItem } from '@frontend/services/azureDataSetService';
import transformDataSetListResults from '../transformDataSetListResults';

// The fields selected by the `search-datasets` API route
type SelectedIndexItem = Pick<
  AzureDataSetIndexItem,
  | 'dataSetFileId'
  | 'fileId'
  | 'filename'
  | 'fileExtension'
  | 'fileSize'
  | 'title'
  | 'content'
  | 'themeId'
  | 'themeTitle'
  | 'publicationId'
  | 'publicationTitle'
  | 'publicationSlug'
  | 'releaseId'
  | 'releaseTitle'
  | 'releaseSlug'
  | 'latestData'
  | 'isSuperseded'
  | 'published'
  | 'lastUpdated'
  | 'api'
  | 'numDataFileRows'
  | 'geographicLevelDetails'
  | 'indicators'
  | 'filters'
  | 'releaseType'
  | 'timePeriodRange'
>;

function testResults(
  documents: SelectedIndexItem[],
): Parameters<typeof transformDataSetListResults>[0] {
  async function* iterate() {
    for (const document of documents) {
      yield { document };
    }
  }

  return iterate() as unknown as Parameters<
    typeof transformDataSetListResults
  >[0];
}

const testDocument: SelectedIndexItem = {
  dataSetFileId: 'data-set-file-1',
  fileId: 'file-1',
  filename: 'file-1',
  fileExtension: 'csv',
  fileSize: '1 KB',
  title: 'Data set 1',
  content: 'Data set 1 summary',
  themeId: 'theme-1',
  themeTitle: 'Theme 1',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSlug: 'publication-1-slug',
  releaseId: 'release-1',
  releaseTitle: 'Release 1',
  releaseSlug: 'release-1-slug',
  latestData: true,
  isSuperseded: false,
  published: '2024-01-01T00:00:00Z',
  lastUpdated: '2024-02-01T00:00:00Z',
  api: { id: '', version: '' },
  numDataFileRows: 100,
  geographicLevelDetails: [],
  indicators: ['Indicator 1'],
  filters: ['Filter 1'],
  releaseType: 'OfficialStatistics',
  timePeriodRange: { from: '2010', to: '2020' },
};

describe('transformDataSetListResults', () => {
  test('splits geographic levels into those available in full and those that are CSV only', async () => {
    const results = await transformDataSetListResults(
      testResults([
        {
          ...testDocument,
          geographicLevelDetails: [
            { code: 'NAT', label: 'National', csvOnly: false },
            { code: 'SCH', label: 'School', csvOnly: true },
            { code: 'REG', label: 'Regional', csvOnly: false },
            { code: 'PROV', label: 'Provider', csvOnly: true },
          ],
        },
      ]),
    );

    expect(results[0].meta.geographicLevels).toEqual(['National', 'Regional']);
    expect(results[0].meta.geographicLevelsCsvOnly).toEqual([
      'School',
      'Provider',
    ]);
  });

  test('returns all geographic levels as CSV only when every level is CSV only', async () => {
    const results = await transformDataSetListResults(
      testResults([
        {
          ...testDocument,
          geographicLevelDetails: [
            { code: 'SCH', label: 'School', csvOnly: true },
          ],
        },
      ]),
    );

    expect(results[0].meta.geographicLevels).toEqual([]);
    expect(results[0].meta.geographicLevelsCsvOnly).toEqual(['School']);
  });

  test('returns empty geographic level lists when there are no geographic levels', async () => {
    const results = await transformDataSetListResults(
      testResults([{ ...testDocument, geographicLevelDetails: [] }]),
    );

    expect(results[0].meta.geographicLevels).toEqual([]);
    expect(results[0].meta.geographicLevelsCsvOnly).toEqual([]);
  });
});
