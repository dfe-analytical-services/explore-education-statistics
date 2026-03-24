import createLegendItemSorter from '@common/modules/charts/util/createLegendItemSorter';
import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { LegendPayload } from 'recharts/types/component/DefaultLegendContent';

describe('createLegendItemSorter', () => {
  const mockConfigItems = [
    { dataKey: 'admission-camden' },
    { dataKey: 'admission-barnsley' },
    { dataKey: 'admission-greenwich' },
  ] as DataSetCategoryConfig[];

  test('returns the correct index for a matching dataKey', () => {
    const sorter = createLegendItemSorter(mockConfigItems);

    expect(
      sorter({
        dataKey: 'admission-greenwich',
        value: 'Greenwich',
      } as LegendPayload),
    ).toBe(2);
    expect(
      sorter({ dataKey: 'admission-camden', value: 'Camden' } as LegendPayload),
    ).toBe(0);
    expect(
      sorter({
        dataKey: 'admission-barnsley',
        value: 'Barnsley',
      } as LegendPayload),
    ).toBe(1);
  });

  test('returns 9999 if the payload item does not have a dataKey', () => {
    const sorter = createLegendItemSorter(mockConfigItems);

    // Testing a payload where dataKey is undefined
    const missingDataKeyPayload = { value: 'Unknown Region' } as LegendPayload;

    expect(sorter(missingDataKeyPayload)).toBe(9999);
  });

  test('returns 9999 if the payload dataKey is not found in the config array', () => {
    const sorter = createLegendItemSorter(mockConfigItems);

    const unknownDataKeyPayload = {
      dataKey: 'admission-birmingham',
      value: 'Birmingham',
    } as LegendPayload;

    expect(sorter(unknownDataKeyPayload)).toBe(9999);
  });
});
