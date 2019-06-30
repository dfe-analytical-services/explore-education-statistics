/**
 * Find elements that match the given
 * {@param id} and a {@param selector}.
 */
export default function findAllByText<T extends HTMLElement = HTMLElement>(
  id: string,
  selector: string,
): T[] | null {
  const element = document.getElementById(id);
  return element != null
    ? (Array.from(element.querySelectorAll(selector)) as T[])
    : null;
}
