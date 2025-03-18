import { useExportButtonContext } from '@common/contexts/ExportButtonContext';
import html2canvas, { Options as HTML2CanvasOptions } from 'html2canvas';
import { useCallback, useState } from 'react';

export type UseGenerateImageType = [
  (callback?: BlobCallback) => Promise<string | null>,
  {
    isLoading: boolean;
  },
];

export type UseGenerateImageArgs = {
  options?: HTML2CanvasOptions;
  quality?: number;
  type?: string;
};

export type UseCurrentPngType = [
  (callback?: BlobCallback) => Promise<string | null>,
  {
    isLoading: boolean;
    ref: React.MutableRefObject<HTMLDivElement | null>;
  },
];

export function useCurrentPng(
  options?: Partial<HTML2CanvasOptions>,
): UseCurrentPngType {
  const ref = useExportButtonContext();

  const [isLoading, setIsLoading] = useState(false);

  const getPng = useCallback(
    async (callback?: BlobCallback): Promise<string | null> => {
      if (!ref?.current) return null;

      setIsLoading(true);

      try {
        const canvas = await html2canvas(ref.current as HTMLElement, {
          logging: false,
          ...options,
        });

        if (callback) {
          canvas.toBlob(callback, 'image/png', 1.0);
        }

        return canvas.toDataURL('image/png', 1.0);
      } finally {
        setIsLoading(false);
      }
    },
    [options, ref],
  );

  return [
    getPng,
    {
      ref,
      isLoading,
    },
  ];
}
