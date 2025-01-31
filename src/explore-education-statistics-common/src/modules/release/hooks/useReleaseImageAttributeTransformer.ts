import { replaceReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';
import { Dictionary } from '@common/types';
import { useCallback } from 'react';

export type ImageAttributeTransformer = (
  attributes: Dictionary<string>,
) => Dictionary<string>;

export default function useReleaseImageAttributeTransformer(options: {
  releaseId: string;
  rootUrl?: string;
}): ImageAttributeTransformer {
  const { releaseId, rootUrl = '' } = options;

  return useCallback(
    ({ src, srcset, ...attributes }: Dictionary<string>) => {
      return {
        ...attributes,
        src: src
          ? `${rootUrl}${replaceReleaseIdPlaceholders(src, releaseId)}`
          : '',
        srcset: srcset
          ? `${rootUrl}${replaceReleaseIdPlaceholders(srcset, releaseId)}`
          : '',
      };
    },
    [releaseId, rootUrl],
  );
}
