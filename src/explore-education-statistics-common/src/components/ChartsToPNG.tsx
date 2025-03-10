/* eslint-disable import/no-extraneous-dependencies */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { useExportButtonContext } from '@common/contexts/ExportButtonContext';
import html2canvas, { Options as HTML2CanvasOptions } from 'html2canvas';
import { useCallback, useState } from 'react';

export type UseGenerateImage<T extends HTMLElement = HTMLDivElement> = [
  (callback?: BlobCallback) => Promise<string | undefined>,
  {
    isLoading: boolean;
  },
];

export type UseGenerateImageArgs = {
  options?: HTML2CanvasOptions;
  quality?: number;
  type?: string;
};

export function useGenerateImage(
  args?: UseGenerateImageArgs,
): UseGenerateImage {
  const [isLoading, setIsLoading] = useState(false);
  const ref = useExportButtonContext();

  const generateImage = useCallback(
    // eslint-disable-next-line consistent-return
    async (callback?: BlobCallback) => {
      if (ref && ref.current) {
        setIsLoading(true);

        return html2canvas(ref.current, {
          logging: false,
          ...args?.options,
        }).then(canvas => {
          if (callback) {
            canvas.toBlob(callback, args?.type, args?.quality);
          }
          setIsLoading(false);
          return canvas.toDataURL(args?.type, args?.quality);
        });
      }
    },
    [args, ref],
  );

  return [
    generateImage,
    {
      isLoading,
    },
  ];
}

export type UseCurrentPng = [
  (callback?: BlobCallback) => Promise<string | undefined>,
  {
    isLoading: boolean;
    ref: React.MutableRefObject<any>;
  },
];

export function useCurrentPng(
  options?: Partial<HTML2CanvasOptions>,
): UseCurrentPng {
  const ref = useExportButtonContext();

  const [isLoading, setIsLoading] = useState(false);

  const getPng = useCallback(
    // eslint-disable-next-line consistent-return
    async (callback?: BlobCallback) => {
      if (ref !== null && ref?.current) {
        setIsLoading(true);

        return html2canvas(ref.current as HTMLElement, {
          logging: false,
          ...options,
        }).then(canvas => {
          if (callback) {
            canvas.toBlob(callback, 'image/png', 1.0);
          }
          setIsLoading(false);
          return canvas.toDataURL('image/png', 1.0);
        });
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

export interface CurrentPngProps {
  chartRef: React.RefObject<any>;
  getPng: (
    options?: Partial<HTML2CanvasOptions>,
  ) => Promise<string | undefined>;
  isLoading: boolean;
}
