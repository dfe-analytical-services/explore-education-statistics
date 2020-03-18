/**
 * Find a parent element matching the given
 * {@param selector} for an {@param element}.
 */
export default function findParent<T extends HTMLElement = HTMLElement>(
  element: HTMLElement,
  selector: string,
): T | null {
  let currentElement = element.parentElement;

  while (currentElement && currentElement !== document.documentElement) {
    if (currentElement.matches(selector)) {
      return currentElement as T;
    }

    currentElement = currentElement.parentElement;
  }

  return null;
}
