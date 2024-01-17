export default function generateRunIdentifier() {
  const date = new Date();
  const utcFormat = date.toISOString().replace('T', ' ').substring(0, 19);
  let random = '';

  // Avoid empty string being created if `Math.random` doesn't generate high enough value
  while (random ==='') {
    random = Math.random().toString(36).slice(2, 7);
  }

  return `${utcFormat} ${random}`;
}

