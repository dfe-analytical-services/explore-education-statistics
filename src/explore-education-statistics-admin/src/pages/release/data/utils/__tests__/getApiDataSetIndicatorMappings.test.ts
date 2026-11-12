import getApiDataSetIndicatorMappings from '@admin/pages/release/data/utils/getApiDataSetIndicatorMappings';

describe('getApiDataSetIndicatorMappings', () => {
  test('orders indicators naturally by label when indicator keys are integer-like', () => {
    // Integer-like keys are enumerated first by Object.entries,
    // regardless of the order the API returns them in.
    const { autoMappedIndicators, mappableIndicators, newIndicators } =
      getApiDataSetIndicatorMappings({
        mappings: {
          'pupils-aged-5': {
            publicId: 'pupils-aged-5-public-id',
            source: { label: 'Pupils aged 5' },
            type: 'AutoMapped',
            candidateKey: 'pupils-aged-5',
          },
          '11': {
            publicId: 'indicator-11-public-id',
            source: { label: 'Pupils aged 11' },
            type: 'AutoMapped',
            candidateKey: '11',
          },
          absence: {
            publicId: 'absence-public-id',
            source: { label: 'Absence' },
            type: 'AutoNone',
          },
          '2': {
            publicId: 'indicator-2-public-id',
            source: { label: 'Exclusions' },
            type: 'AutoNone',
          },
        },
        candidates: {
          'pupils-aged-5': { label: 'Pupils aged 5' },
          '11': { label: 'Pupils aged 11' },
          enrolments: { label: 'Enrolments' },
          '3': { label: 'Fixed term exclusions' },
        },
      });

    expect(
      autoMappedIndicators.map(indicator => indicator.mapping.sourceKey),
    ).toEqual(['pupils-aged-5', '11']);
    expect(
      mappableIndicators.map(indicator => indicator.mapping.sourceKey),
    ).toEqual(['absence', '2']);
    expect(newIndicators.map(indicator => indicator.key)).toEqual([
      'enrolments',
      '3',
    ]);
  });
});
