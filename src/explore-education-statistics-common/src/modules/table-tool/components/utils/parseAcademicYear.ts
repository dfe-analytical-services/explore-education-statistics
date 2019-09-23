export default function parseYear(year?: number): string {
  if (!year) {
    return '';
  }

  const yearString = year.toString();

  if (yearString.length === 6) {
    return `${yearString.substring(0, 4)}/${yearString.substring(4, 6)}`;
  }

  return `${year}/${Number(yearString.substring(2, 4)) + 1}`;
}
