import typedKeys from '@common/utils/object/typedKeys';
import pickBy from 'lodash/pickBy';
import sortBy from 'lodash/sortBy';

export const locationCodeKeys = [
  'id',
  'code',
  'oldCode',
  'urn',
  'laEstab',
  'ukprn',
] as const;

export type LocationCodeKey = (typeof locationCodeKeys)[number];

export type LocationCodeLabels = Record<LocationCodeKey, string>;

const defaultLabels = {
  id: 'ID',
  code: 'Code',
  oldCode: 'Old code',
  urn: 'URN',
  laEstab: 'LAESTAB',
  ukprn: 'UKPRN',
} as const satisfies LocationCodeLabels;

export interface LocationCodeEntry {
  key: LocationCodeKey;
  label: string;
  value: string;
}

export default function getLocationCodeEntries(
  location: Partial<LocationCodeLabels>,
  overrideLabels: Partial<LocationCodeLabels> = {},
): LocationCodeEntry[] {
  const labels = {
    ...defaultLabels,
    ...overrideLabels,
  };

  const keys = typedKeys<Partial<LocationCodeLabels>>(
    pickBy(
      location,
      (_, key) =>
        labels[key as LocationCodeKey] && location[key as LocationCodeKey],
    ),
  );

  return sortBy(keys, key => locationCodeKeys.indexOf(key)).map(key => {
    return {
      key,
      label: labels[key],
      value: location[key] ?? '',
    };
  });
}
