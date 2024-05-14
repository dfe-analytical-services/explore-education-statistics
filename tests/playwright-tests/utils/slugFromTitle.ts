export default function slugFromTitle(title: string) {
  return title.replace(/\W+/g, ' ').trim().toLowerCase().replace(/\s+/g, '-');
}
