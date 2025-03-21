import { useExportButtonContext } from '@common/contexts/ExportButtonContext';
import html2canvas, { Options as HTML2CanvasOptions } from 'html2canvas';
import { useCallback } from 'react';

export type UseCurrentPngType = [
  (callback?: BlobCallback) => Promise<string | null>,
  {
    ref: React.MutableRefObject<HTMLDivElement | null>;
  },
];

export function useCurrentPng(
  options?: Partial<HTML2CanvasOptions>,
): UseCurrentPngType {
  const ref = useExportButtonContext();

  const getPng = useCallback(
    async (callback?: BlobCallback): Promise<string | null> => {
      if (!ref?.current) return null;

      const canvas = await html2canvas(ref.current as HTMLElement, {
        logging: false,
        ...options,
      });

      if (callback) {
        canvas.toBlob(callback, 'image/png', 1.0);
      }

      return canvas.toDataURL('image/png', 1.0);
    },
    [options, ref],
  );

  return [
    getPng,
    {
      ref,
    },
  ];
}
