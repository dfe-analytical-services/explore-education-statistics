import { JsonElement, JsonElementChild } from '@admin/types/ckeditor';

export default function getInvalidImages(
  elementsJson: JsonElement[],
): JsonElement[] {
  return elementsJson.filter(
    element =>
      isInvalidImage(element) ||
      element.children?.some(child => isInvalidImage(child)),
  );
}

function isInvalidImage(elementJson: JsonElement | JsonElementChild) {
  return (
    (elementJson.name === 'imageBlock' || elementJson.name === 'imageInline') &&
    !elementJson.attributes?.alt
  );
}
