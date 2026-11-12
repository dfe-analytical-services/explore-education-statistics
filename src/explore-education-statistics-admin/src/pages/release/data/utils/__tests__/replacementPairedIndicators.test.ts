import {
  getManageIndicatorsPath,
  getReplacementPairedIndicators,
} from '@admin/pages/release/data/utils/replacementPairedIndicators';
import { IndicatorsMappingsPlan } from '@admin/services/dataReplacementService';

describe('replacementPairedIndicators', () => {
  describe('getReplacementPairedIndicators', () => {
    const createMapping = (
      id: string,
      label: string,
      candidateKey?: string,
    ) => ({
      source: { id, label, name: id },
      type: candidateKey ? ('AutoSet' as const) : ('Unset' as const),
      candidateKey,
    });

    const testIndicatorMappings: IndicatorsMappingsPlan = {
      candidates: {},
      mappings: {
        'indicator-1': createMapping('indicator-1', 'Indicator 1', 'new-1'),
        'indicator-2': createMapping('indicator-2', 'Indicator 2', 'new-2'),
        'indicator-3': createMapping('indicator-3', 'Indicator 3', 'new-3'),
        'indicator-4': createMapping('indicator-4', 'Indicator 4'),
        'indicator-5': createMapping('indicator-5', 'Indicator 5'),
      },
    };

    test('maintains pairs with both indicators mapped', () => {
      expect(
        getReplacementPairedIndicators(
          [
            {
              parentIndicatorId: 'indicator-1',
              childIndicatorId: 'indicator-2',
            },
          ],
          testIndicatorMappings,
        ),
      ).toEqual([
        {
          parentIndicatorId: 'indicator-1',
          parentLabel: 'Indicator 1',
          childIndicatorId: 'indicator-2',
          childLabel: 'Indicator 2',
          maintained: true,
        },
      ]);
    });

    test('drops pairs with the parent or child indicator unmapped', () => {
      const result = getReplacementPairedIndicators(
        [
          { parentIndicatorId: 'indicator-3', childIndicatorId: 'indicator-4' },
          { parentIndicatorId: 'indicator-5', childIndicatorId: 'indicator-1' },
        ],
        testIndicatorMappings,
      );

      expect(result.map(pair => pair.maintained)).toEqual([false, false]);
    });

    test('drops pairs with an indicator missing from the mappings', () => {
      expect(
        getReplacementPairedIndicators(
          [{ parentIndicatorId: 'indicator-1', childIndicatorId: 'unknown' }],
          testIndicatorMappings,
        ),
      ).toEqual([
        {
          parentIndicatorId: 'indicator-1',
          parentLabel: 'Indicator 1',
          childIndicatorId: 'unknown',
          childLabel: 'unknown',
          maintained: false,
        },
      ]);
    });
  });

  describe('getManageIndicatorsPath', () => {
    test('opens managing indicators for the data set', () => {
      expect(
        getManageIndicatorsPath({
          publicationId: 'publication-1',
          releaseVersionId: 'release-1',
          subjectId: 'subject-1',
        }),
      ).toBe(
        '/publication/publication-1/release/release-1/data?manageIndicators=subject-1#reordering',
      );
    });

    test('links to the tab when there is no data set', () => {
      expect(
        getManageIndicatorsPath({
          publicationId: 'publication-1',
          releaseVersionId: 'release-1',
        }),
      ).toBe('/publication/publication-1/release/release-1/data#reordering');
    });
  });
});
