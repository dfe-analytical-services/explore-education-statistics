import { LocationCandidate } from '@admin/services/apiDataSetVersionService';
import pickBy from 'lodash/pickBy';
import sortBy from 'lodash/sortBy';

const fields = [
  'code',
  'oldCode',
  'urn',
  'laEstab',
  'ukprn',
] as const satisfies readonly (keyof LocationCandidate)[];

type Field = (typeof fields)[number];

export default function getApiDataSetLocationCodes(
  candidate: LocationCandidate,
): string {
  const entries = Object.entries(
    pickBy(
      candidate,
      (_, key) => fields.includes(key as Field) && candidate[key as Field],
    ),
  ) as [Field, string][];

  if (entries.length === 1 && entries[0][0] === 'code') {
    return entries[0][1];
  }

  return sortBy(entries, ([key]) => fields.indexOf(key))
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
