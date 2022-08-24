export default function getTableHeaderGroupId(legend: string) {
  return `group-${legend.replace(/\s+/g, '-').toLowerCase()}`;
}
