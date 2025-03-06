/* eslint-disable import/no-extraneous-dependencies */
/* eslint-disable @typescript-eslint/no-explicit-any */
import html2canvas, { Options as HTML2CanvasOptions } from 'html2canvas';
import { Component, createRef, useCallback, useState } from 'react';
import { useRefContext } from './ExportButtonMenu';

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

/**
 * @param {{options: HTML2CanvasOptions, type: string, quality: number}} [args] Optional arguments.
 * @param {HTML2CanvasOptions} [args.options] An html2canvas Options object.
 * @param {number} [args.quality] Applies if the type is an image format that supports variable quality (such as "image/jpeg"), and is a number in the range 0.0 to 1.0 inclusive indicating the desired quality level for the resulting image.
 * @param {string} [args.type] Controls the type of the image to be returned (e.g. PNG or JPEG). The default is "image/png"; that type is also used if the given type isn't supported.
 *
 * [Reference: html.spec.whatwg.org](https://html.spec.whatwg.org/multipage/canvas.html#dom-canvas-todataurl-dev)
 */
export function useGenerateImage(
  args?: UseGenerateImageArgs
): UseGenerateImage {
  const [isLoading, setIsLoading] = useState(false);
  const ref = useRefContext();

  console.log(ref);
  
  const generateImage = useCallback(
    // eslint-disable-next-line consistent-return
    async (callback?: BlobCallback) => {
      if (ref && ref.current) {
        setIsLoading(true);

        return html2canvas(ref.current, {
          logging: false,
          ...args?.options,
        }).then((canvas) => {
          if (callback) {
            canvas.toBlob(callback, args?.type, args?.quality);
          }
          setIsLoading(false);
          return canvas.toDataURL(args?.type, args?.quality);
        });
      }
    },
    [args, ref]
  );

  return [
    generateImage,
    {
      isLoading
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

/**
 * @param options - optional html2canvas Options object
 */
export function useCurrentPng(options?: Partial<HTML2CanvasOptions>): UseCurrentPng {
  const ref = useRefContext();

  console.log(ref);
  const [isLoading, setIsLoading] = useState(false);

  const getPng = useCallback(
    // eslint-disable-next-line consistent-return
    async (callback?: BlobCallback) => {
      if (ref !== null && ref?.current) {
        setIsLoading(true);

        return html2canvas(ref.current as HTMLElement, {
          logging: false,
          ...options,
        }).then((canvas) => {
          if (callback) {
            canvas.toBlob(callback, 'image/png', 1.0);
          }
          setIsLoading(false);
          return canvas.toDataURL('image/png', 1.0);
        });
      }
    },
    [options, ref]
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
  getPng: (options?: Partial<HTML2CanvasOptions>) => Promise<string | undefined>;
  isLoading: boolean;
}