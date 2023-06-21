/**
 * Find a sibling of an {@param element}
 * that matches the given {@param selector}.
 */
export default function findPreviousSibling<
  T extends HTMLElement = HTMLElement
>(element: Element, selector: string): T | null {
  let sibling = element.previousElementSibling;

  while (sibling && !sibling.matches(selector)) {
    sibling = sibling.previousElementSibling;
  }

  return (sibling as T) || null;
}
