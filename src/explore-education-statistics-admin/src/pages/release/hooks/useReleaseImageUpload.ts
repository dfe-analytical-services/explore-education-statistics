import releaseImageService from '@admin/services/releaseImageService';
import { ImageUploadResult } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import { CancellablePromise } from '@common/types/promise';
import { useCallback, useRef } from 'react';

interface UseImageUploadReturn {
  handleImageUpload: ImageUploadHandler;
  handleImageUploadCancel: ImageUploadCancelHandler;
}

export default function useReleaseImageUpload(
  releaseId: string,
): UseImageUploadReturn {
  const promise = useRef<CancellablePromise<ImageUploadResult>>();

  const handleImageUpload: ImageUploadHandler = useCallback(
    async (file, updateProgress) => {
      promise.current = releaseImageService
        .upload(releaseId, file, updateProgress)
        .then() as CancellablePromise<ImageUploadResult>;

      return promise.current;
    },
    [releaseId],
  );

  const handleImageUploadCancel: ImageUploadCancelHandler = useCallback(() => {
    promise.current?.cancel();
  }, []);

  return {
    handleImageUpload,
    handleImageUploadCancel,
  };
}
