export default function generateRunIdentifier() {
  const date = new Date();
  const utcFormat = date.toISOString().replace('T', ' ').substring(0, 19);
  return `UI Test${utcFormat}`;
}
