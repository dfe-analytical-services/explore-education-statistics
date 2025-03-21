export default function sortAlphabeticalTotalsFirst(a: string, b: string) {
  if (a.toLocaleLowerCase() === 'total') {
    return -1;
  }
  if (b.toLocaleLowerCase() === 'total') {
    return 1;
  }
  if (a < b) {
    return -1;
  }
  if (a > b) {
    return 1;
  }
  return 0;
}
