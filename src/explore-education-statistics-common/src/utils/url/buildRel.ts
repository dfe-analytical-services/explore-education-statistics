import compact from 'lodash/compact';

export default function buildRel(
  newValues: string[],
  existingRel?: string,
): string {
  const relValues = existingRel ? compact(existingRel.split(' ')) : [];

  newValues.forEach(value => {
    if (!relValues.includes(value)) {
      relValues.push(value);
    }
  });

  return relValues.join(' ').trim();
}
