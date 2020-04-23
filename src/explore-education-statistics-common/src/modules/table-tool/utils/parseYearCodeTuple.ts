export default function parseYearCodeTuple(value: string): [number, string] {
  const [year, code] = value.split('_');

  const parsedYear = Number(year);

  if (Number.isNaN(parsedYear)) {
    throw new TypeError('Could not parse time period year');
  }

  return [parsedYear, code];
}
