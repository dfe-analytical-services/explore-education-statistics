import getTargetRelation from '@common/utils/url/getTargetRelation';

describe('getTargetRelation', () => {
  test('returns internal-public for public URL', () => {
    expect(
      getTargetRelation('https://explore-education-statistics.service.gov.uk'),
    ).toBe('internal-public');
  });

  test('returns internal-admin for admin URL', () => {
    expect(
      getTargetRelation(
        'https://admin.explore-education-statistics.service.gov.uk',
      ),
    ).toBe('internal-admin');
  });

  test('returns external for external URL', () => {
    expect(getTargetRelation('https://gov.uk/')).toBe('external');
  });
});
