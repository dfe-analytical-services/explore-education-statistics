/**
 * Concatenate a {@param list} of strings with commas
 * and terminate with an `and` if necessary.
 * This function will strip out any empty strings as well.
 */
export default function commaList(list: string[]) {
  if (list.length === 1) {
    return list[0];
  }

  if (list.length > 1) {
    const filteredList = list.map(item => item.trim()).filter(Boolean);

    return `${filteredList.slice(0, -1).join(', ')} and ${filteredList.slice(
      -1,
    )}`;
  }

  return '';
}
