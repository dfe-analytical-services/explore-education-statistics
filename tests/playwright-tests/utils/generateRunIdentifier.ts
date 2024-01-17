export default function generateRunIdentifier() {
  const date = new Date();
  const utcFormat = date.toISOString().replace('T', ' ').substring(0, 19);
  let random = '';

  while (random === '') {
    random = Math.random().toString(36).slice(2, 7);
  }

  return `${utcFormat} ${random}`;
}
