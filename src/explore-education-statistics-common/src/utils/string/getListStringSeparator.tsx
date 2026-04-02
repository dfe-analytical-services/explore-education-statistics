export default function getListStringSeparator(list: unknown[], index: number) {
  if (!list || index < 1 || index > list.length - 1) return '';
  if (index === list.length - 1) return ' and ';
  return ', ';
}
