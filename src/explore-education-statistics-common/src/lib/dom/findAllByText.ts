/**
 * Find elements that match the given
 * {@param text} and a {@param selector}.
 */
export default function findAllByText<T extends HTMLElement = HTMLElement>(
  text: string,
  selector: string,
): T[] {
  return Array.from(document.querySelectorAll(selector)).filter(element =>
    (element.textContent || '')
      .toLocaleLowerCase()
      .includes(text.toLocaleLowerCase()),
  ) as T[];
}
