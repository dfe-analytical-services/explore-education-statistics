import sortPublishingOrganisationTitles from '../sortPublishingOrganisationTitles';

describe('sortPublishingOrganisationTitles', () => {
  test('returns an empty array when no organisations provided', () => {
    expect(sortPublishingOrganisationTitles([])).toEqual([]);
  });

  test('sorts organisations alphabetically', () => {
    expect(
      sortPublishingOrganisationTitles(['Ofsted', 'Ofqual', 'Skills England']),
    ).toEqual(['Ofqual', 'Ofsted', 'Skills England']);
  });

  test('sorts Department for Education first', () => {
    expect(
      sortPublishingOrganisationTitles([
        'Ofsted',
        'Skills England',
        'Department for Education',
        'Ofqual',
      ]),
    ).toEqual([
      'Department for Education',
      'Ofqual',
      'Ofsted',
      'Skills England',
    ]);
  });

  test('returns a single organisation unchanged', () => {
    expect(sortPublishingOrganisationTitles(['Ofsted'])).toEqual(['Ofsted']);
  });
});
