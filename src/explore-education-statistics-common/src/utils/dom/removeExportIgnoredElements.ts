/**
 * Class name for elements that should be left out when
 * exporting content e.g. as an image.
 */
export const exportIgnoreClassName = 'dfe-export-ignore';

const exportIgnoreSelector = [
  `.${exportIgnoreClassName}`,
  // Content of closed details elements isn't visible,
  // so shouldn't be included in exports either.
  'details:not([open]) > :not(summary)',
].join(', ');

/**
 * Remove any elements from {@param root} that should
 * not be included in exports. This mutates {@param root},
 * so should only be used on a cloned element.
 */
export default function removeExportIgnoredElements(root: HTMLElement) {
  root
    .querySelectorAll(exportIgnoreSelector)
    .forEach(element => element.remove());
}
