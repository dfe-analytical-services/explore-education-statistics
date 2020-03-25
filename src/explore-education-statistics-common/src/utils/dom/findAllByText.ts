/**
 * Find elements that match the given
 * {@param text} and a {@param selector}.
 * Elements can also be matched by lower cased
 * text content if {@param useLowerCase} is true.
 */
export default function findAllByText<T extends HTMLElement = HTMLElement>(
  text: string,
  selector: string,
  useLowerCase = false,
): T[] {
  return Array.from(document.querySelectorAll(selector)).filter(element => {
    const searchText = useLowerCase
      ? (element.textContent || '').toLocaleLowerCase()
      : element.textContent || '';

    return searchText.includes(text);
  }) as T[];
}
