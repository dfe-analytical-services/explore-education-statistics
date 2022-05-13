import { queryHelpers } from '@testing-library/dom';

function getDescribedByIds(element: HTMLElement) {
  const describedBy = element.getAttribute('aria-describedby');

  if (!describedBy) {
    throw queryHelpers.getElementError(
      'Element does not have aria-describedby attribute',
      element,
    );
  }

  return describedBy.split(' ');
}

export function queryAllDescribedBy(
  element: HTMLElement,
  container: HTMLElement = document.body,
): (HTMLElement | null)[] {
  return getDescribedByIds(element).map(id => {
    const els = queryHelpers.queryAllByAttribute('id', container, id);

    if (els.length > 1) {
      throw queryHelpers.getElementError(
        `Found multiple elements matching aria-describedby id: ${id}`,
        container,
      );
    }

    return els[0];
  });
}

export function getAllDescribedBy(
  element: HTMLElement,
  container: HTMLElement = document.body,
): HTMLElement[] {
  const ids = getDescribedByIds(element);

  const elements = ids.map(id => {
    const els = queryHelpers.queryAllByAttribute('id', container, id);

    if (els.length > 1) {
      throw queryHelpers.getElementError(
        `Found multiple elements matching aria-describedby id: ${id}`,
        container,
      );
    }

    if (!els.length) {
      throw queryHelpers.getElementError(
        `Could not find element matching aria-describedby id: ${id}`,
        container,
      );
    }

    return els[0];
  });

  if (!elements.length) {
    throw queryHelpers.getElementError(
      `Could not find any elements matching aria-describedby ids: ${ids.join(
        ', ',
      )}`,
      container,
    );
  }

  return elements;
}

export function getDescribedBy(
  element: HTMLElement,
  container: HTMLElement = document.body,
): HTMLElement {
  const ids = getDescribedByIds(element);

  if (ids.length > 1) {
    throw queryHelpers.getElementError(
      'Element cannot have multiple aria-describedby ids',
      element,
    );
  }

  return getAllDescribedBy(element, container)[0];
}

export function queryDescribedBy(
  container: HTMLElement = document.body,
  element: HTMLElement,
): HTMLElement | null {
  const ids = getDescribedByIds(element);

  if (ids.length > 1) {
    throw queryHelpers.getElementError(
      'Element cannot have multiple aria-describedby ids',
      element,
    );
  }

  return queryAllDescribedBy(element, container)[0];
}
