import { RefObject } from 'react';

export default function copyElementToClipboard(
  elementRef: RefObject<HTMLElement>,
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

  navigator.clipboard.write([clipboardItem]);
}
