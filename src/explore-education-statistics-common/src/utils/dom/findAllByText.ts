/**
 * Find elements that match the given
 * {@param text} and a {@param selector}.
 *
 * Additional options can be supplied via {@param options}.
 */
export default function findAllByText<T extends HTMLElement = HTMLElement>(
  text: string,
  selector: string,
  options?: {
    rootElement?: Element;
    useLowerCase?: boolean;
  },
): T[] {
  const { rootElement = document, useLowerCase = false } = options ?? {};

  return Array.from(rootElement.querySelectorAll(selector)).filter(element => {
    const searchText = useLowerCase
      ? (element.textContent || '').toLocaleLowerCase()
      : element.textContent || '';

    return searchText.includes(text);
  }) as T[];
}
