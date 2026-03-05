import { JsonElement } from '@admin/types/ckeditor';

export default function checkMaxLength(
  elements: JsonElement[],
  maxChars: number,
): string {
  const contentLength = elements
    .flatMap(element => element.children?.flatMap(child => child))
    .reduce<number>((acc, el) => {
      return el && el.data ? acc + el.data.length : acc;
    }, 0);

  return contentLength > maxChars
    ? `You have used ${contentLength} characters and the limit is ${maxChars}.`
    : '';
}
