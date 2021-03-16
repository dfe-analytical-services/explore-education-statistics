import { replaceMethodologyIdPlaceholders } from '@common/modules/methodology/utils/methodologyImageUrls';
import { Dictionary } from '@common/types';
import { useCallback } from 'react';

export type ImageAttributeTransformer = (
  attributes: Dictionary<string>,
) => Dictionary<string>;

export default function useMethodologyImageAttributeTransformer(options: {
  methodologyId: string;
  rootUrl?: string;
}): ImageAttributeTransformer {
  const { methodologyId, rootUrl = '' } = options;

  return useCallback(
    ({ src, srcset, ...attributes }: Dictionary<string>) => {
      return {
        ...attributes,
        src: src
          ? `${rootUrl}${replaceMethodologyIdPlaceholders(src, methodologyId)}`
          : '',
        srcset: srcset
          ? `${rootUrl}${replaceMethodologyIdPlaceholders(
              srcset,
              methodologyId,
            )}`
          : '',
      };
    },
    [methodologyId, rootUrl],
  );
}
