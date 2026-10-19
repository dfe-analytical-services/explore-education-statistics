import {
  getPairedIndicatorChildIds,
  normalisePairedIndicators,
  orderPairedIndicatorOptions,
} from '@common/modules/table-tool/utils/pairedIndicators';
import { IndicatorOption } from '@common/services/tableBuilderService';

describe('pairedIndicators', () => {
  const createOption = (
    value: string,
    parentIndicatorId?: string,
  ): IndicatorOption => ({
    label: `Label ${value}`,
    value,
    unit: '',
    name: value,
    parentIndicatorId,
  });

  describe('getPairedIndicatorChildIds', () => {
    test('returns child ids by parent id across groups', () => {
      const result = getPairedIndicatorChildIds([
        {
          options: [
            createOption('indicator-1'),
            createOption('indicator-2', 'indicator-1'),
          ],
        },
        {
          options: [
            createOption('indicator-3'),
            createOption('indicator-4', 'indicator-3'),
            createOption('indicator-5'),
          ],
        },
      ]);

      expect(result).toEqual({
        'indicator-1': 'indicator-2',
        'indicator-3': 'indicator-4',
      });
    });

    test('returns empty object when there are no paired indicators', () => {
      expect(
        getPairedIndicatorChildIds([
          {
            options: [createOption('indicator-1'), createOption('indicator-2')],
          },
        ]),
      ).toEqual({});
    });
  });

  describe('normalisePairedIndicators', () => {
    const childIdsByParentId = { 'indicator-1': 'indicator-2' };

    test('adds the child of a selected parent', () => {
      expect(
        normalisePairedIndicators(
          ['indicator-3', 'indicator-1'],
          childIdsByParentId,
        ),
      ).toEqual(['indicator-3', 'indicator-1', 'indicator-2']);
    });

    test('removes a child without its parent', () => {
      expect(
        normalisePairedIndicators(
          ['indicator-2', 'indicator-3'],
          childIdsByParentId,
        ),
      ).toEqual(['indicator-3']);
    });

    test('does not duplicate an already selected child', () => {
      expect(
        normalisePairedIndicators(
          ['indicator-2', 'indicator-1'],
          childIdsByParentId,
        ),
      ).toEqual(['indicator-1', 'indicator-2']);
    });

    test('returns the selection unchanged when there are no paired indicators', () => {
      const selected = ['indicator-1', 'indicator-2'];

      expect(normalisePairedIndicators(selected, {})).toBe(selected);
    });
  });

  describe('orderPairedIndicatorOptions', () => {
    test('moves a child directly after its parent', () => {
      const result = orderPairedIndicatorOptions([
        createOption('indicator-2', 'indicator-1'),
        createOption('indicator-3'),
        createOption('indicator-1'),
      ]);

      expect(result.map(option => option.value)).toEqual([
        'indicator-3',
        'indicator-1',
        'indicator-2',
      ]);
    });

    test('keeps a child in place when its parent is in another group', () => {
      const result = orderPairedIndicatorOptions([
        createOption('indicator-2', 'indicator-other-group'),
        createOption('indicator-3'),
      ]);

      expect(result.map(option => option.value)).toEqual([
        'indicator-2',
        'indicator-3',
      ]);
    });
  });
});
