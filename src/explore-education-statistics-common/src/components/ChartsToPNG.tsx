import { useRefContext } from '@common/contexts/RefContext';
import html2canvas, { Options as HTML2CanvasOptions } from 'html2canvas';
import { useCallback } from 'react';

type UseCurrentPngType = [(callback?: BlobCallback) => Promise<string | null>];

const useCurrentPng = (
  options?: Partial<HTML2CanvasOptions>,
): UseCurrentPngType => {
  const ref = useRefContext();

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

  return [getPng];
};

export default useCurrentPng;
