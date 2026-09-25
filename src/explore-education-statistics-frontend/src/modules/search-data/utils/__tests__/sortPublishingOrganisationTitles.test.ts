import sortPublishingOrganisationTitles from '../sortPublishingOrganisationTitles';

describe('sortPublishingOrganisationTitles', () => {
  // TODO EES-7673 - remove this test case once all releases have an organisation
  test('returns Department for Education when no organisations given', () => {
    expect(sortPublishingOrganisationTitles()).toEqual([
      'Department for Education',
    ]);
  });

  // TODO EES-7673 - remove this test case once all releases have an organisation
  test('returns Department for Education when organisations are empty', () => {
    expect(sortPublishingOrganisationTitles([])).toEqual([
      'Department for Education',
    ]);
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
