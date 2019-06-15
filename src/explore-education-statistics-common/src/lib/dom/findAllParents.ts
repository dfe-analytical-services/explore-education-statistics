/**
 * Find all parent elements matching the given
 * {@param selector} for an {@param element}.
 */
export default function findAllParents<T extends HTMLElement = HTMLElement>(
  element: HTMLElement,
  selector: string,
): T[] {
  const parents: T[] = [];
  let currentElement = element.parentElement;

  while (currentElement && currentElement !== document.documentElement) {
    if (currentElement.matches(selector)) {
      parents.push(currentElement as T);
    }

    currentElement = currentElement.parentElement;
  }

  return parents;
}
