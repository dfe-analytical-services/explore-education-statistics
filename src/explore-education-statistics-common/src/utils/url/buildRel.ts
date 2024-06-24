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
