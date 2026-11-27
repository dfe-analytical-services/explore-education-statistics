import eesTenant from '@common/tenants/eesTenant';
import testTenant from '@common-test/testTenant';
import { testDataSetFile } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import getDataSetFileMetaCSVW from '@frontend/modules/data-catalogue/utils/getDataSetFileMetaCSVW';
import _getCurrentTenant from '@frontend/utils/getCurrentTenant';

jest.mock('@frontend/utils/getCurrentTenant');

const getCurrentTenant = _getCurrentTenant as jest.MockedFunction<
  typeof _getCurrentTenant
>;

describe('getDataSetFileMetaCSVW', () => {
  function getMeta(dataSetFile = testDataSetFile) {
    return JSON.parse(getDataSetFileMetaCSVW({ dataSetFile }).content ?? '');
  }

  test('returns the EES metadata', () => {
    getCurrentTenant.mockReturnValue(eesTenant);

    const meta = getMeta({ ...testDataSetFile, summary: '' });

    expect(meta.description).toBe(
      `Explore education statistics data set ${testDataSetFile.title} from Publication 1`,
    );
    expect(meta.url).toBe(
      'https://explore-education-statistics.service.gov.uk/data-catalogue/data-set/data-set-file-id',
    );
    expect(meta.sameAs).toBe(meta.url);
    expect(meta.includedInDataCatalog).toEqual({
      '@type': 'DataCatalog',
      name: 'Explore education statistics datasets',
      description:
        'A Department for Education repository for education statistics',
      url: 'https://explore-education-statistics.service.gov.uk/data-catalogue',
    });

    const expectedOrganisation = {
      '@type': 'Organization',
      name: 'Department for Education',
      description: eesTenant.organisation.description,
      url: 'https://www.gov.uk/government/organisations/department-for-education',
      sameAs: 'https://ror.org/0320bge18',
    };
    expect(meta.creator).toEqual([expectedOrganisation]);
    expect(meta.publisher).toEqual(expectedOrganisation);
  });

  test('prefixes a short summary to the generated description', () => {
    getCurrentTenant.mockReturnValue(eesTenant);

    const meta = getMeta({ ...testDataSetFile, summary: 'Short summary' });

    expect(meta.description).toBe(
      `Short summary - Explore education statistics data set ${testDataSetFile.title} from Publication 1`,
    );
  });

  test('returns metadata from a different tenant', () => {
    getCurrentTenant.mockReturnValue(testTenant);

    const meta = getMeta({ ...testDataSetFile, summary: '' });

    expect(meta.description).toBe(
      `Test statistics data set ${testDataSetFile.title} from Publication 1`,
    );
    expect(meta.url).toBe(
      'https://test-statistics-service.example.com/data-catalogue/data-set/data-set-file-id',
    );
    expect(meta.includedInDataCatalog).toEqual({
      '@type': 'DataCatalog',
      name: 'Test statistics datasets',
      description: 'A Test Organisation repository for testing statistics',
      url: 'https://test-statistics-service.example.com/data-catalogue',
    });

    // The test tenant has no ROR record, so sameAs is left out.
    const expectedOrganisation = {
      '@type': 'Organization',
      name: 'Test Organisation',
      description: 'Test Organisation is responsible for testing things.',
      url: 'https://test-organisation.example.com',
    };
    expect(meta.creator).toEqual([expectedOrganisation]);
    expect(meta.publisher).toEqual(expectedOrganisation);
    expect(JSON.stringify(meta)).not.toMatch(
      /Department for Education|explore-education-statistics|ror\.org/,
    );
  });
});
