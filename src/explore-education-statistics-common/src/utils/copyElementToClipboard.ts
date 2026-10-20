import { RefObject } from 'react';

export default async function copyElementToClipboard(
  elementRef: RefObject<HTMLElement | null>,
) {
  if (!elementRef.current) return;

  const clipboardItem = new ClipboardItem({
    'text/plain': new Blob([elementRef.current.innerText], {
      type: 'text/plain',
    }),
    'text/html': new Blob([elementRef.current.outerHTML], {
      type: 'text/html',
    }),
  });

  await navigator.clipboard.write([clipboardItem]);
}
