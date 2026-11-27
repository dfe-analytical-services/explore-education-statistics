import formatOrganisationPossessive from '@common/tenants/utils/formatOrganisationPossessive';

describe('formatOrganisationPossessive', () => {
  test('uses the acronym when there is one', () => {
    expect(
      formatOrganisationPossessive({
        acronym: 'DfE',
        nameWithArticle: 'the Department for Education',
      }),
    ).toBe("DfE's");
  });

  test('uses the name with its article when there is no acronym', () => {
    expect(
      formatOrganisationPossessive({
        nameWithArticle: 'the Test Organisation',
      }),
    ).toBe("the Test Organisation's");
  });

  test('treats a blank acronym as absent', () => {
    expect(
      formatOrganisationPossessive({
        acronym: '',
        nameWithArticle: 'the Test Organisation',
      }),
    ).toBe("the Test Organisation's");
  });
});
