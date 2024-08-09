import { LocationCandidate } from '@admin/services/apiDataSetVersionService';
import pickBy from 'lodash/pickBy';
import sortBy from 'lodash/sortBy';

export const locationFields = [
  'code',
  'oldCode',
  'urn',
  'laEstab',
  'ukprn',
] as const satisfies readonly (keyof LocationCandidate)[];

export type LocationField = (typeof locationFields)[number];

export default function getApiDataSetLocationCodes(
  candidate: LocationCandidate,
): string {
  const entries = Object.entries(
    pickBy(
      candidate,
      (_, key) =>
        locationFields.includes(key as LocationField) &&
        candidate[key as LocationField],
    ),
  ) as [LocationField, string][];

  if (entries.length === 1 && entries[0][0] === 'code') {
    return entries[0][1];
  }

  return sortBy(entries, ([key]) => locationFields.indexOf(key))
    .map(([key, value]) => {
      switch (key) {
        case 'code':
          return `Code: ${value}`;
        case 'oldCode':
          return `Old code: ${value}`;
        case 'urn':
          return `URN: ${value}`;
        case 'laEstab':
          return `LAESTAB: ${value}`;
        case 'ukprn':
          return `UKPRN: ${value}`;
        default:
          return '';
      }
    })
    .join(', ');
}
