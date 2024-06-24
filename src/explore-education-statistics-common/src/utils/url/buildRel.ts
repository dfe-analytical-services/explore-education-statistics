import getExternality from '@common/utils/url/getExternality';

export default function buildRel(
  newValues: string[],
  existingRel?: string,
): string {
  const relValues = existingRel ? existingRel.split(' ') : [];

  newValues.forEach(value => {
    if (!relValues.includes(value)) {
      relValues.push(value);
    }
  });

  return relValues.join(' ').trim();
}

export function getRelsForExternality(url: string | URL) {
  const externality = getExternality(url);

  switch (externality) {
    case 'internal':
      return [];
    case 'external-admin':
      return ['nofollow'];
    case 'external-trusted':
      return ['nofollow', 'noopener', 'noreferrer'];
    default:
      return ['noopener', 'noreferrer', 'nofollow', 'external'];
  }
}
