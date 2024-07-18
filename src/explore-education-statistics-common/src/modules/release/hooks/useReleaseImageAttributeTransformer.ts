import { replaceReleaseIdPlaceholders } from '@common/modules/release/utils/releaseImageUrls';
import { Dictionary } from '@common/types';
import { useCallback } from 'react';

export type ImageAttributeTransformer = (
  attributes: Dictionary<string>,
) => Dictionary<string>;

export default function useReleaseImageAttributeTransformer(options: {
  releaseVersionId: string;
  rootUrl?: string;
}): ImageAttributeTransformer {
  const { releaseVersionId, rootUrl = '' } = options;

  return useCallback(
    ({ src, srcset, ...attributes }: Dictionary<string>) => {
      return {
        ...attributes,
        src: src
          ? `${rootUrl}${replaceReleaseIdPlaceholders(src, releaseVersionId)}`
          : '',
        srcset: srcset
          ? `${rootUrl}${replaceReleaseIdPlaceholders(
              srcset,
              releaseVersionId,
            )}`
          : '',
      };
    },
    [releaseVersionId, rootUrl],
  );
}
