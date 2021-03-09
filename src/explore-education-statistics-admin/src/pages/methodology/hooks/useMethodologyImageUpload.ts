import { ImageUploadResult } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import { CancellablePromise } from '@common/types/promise';
import { useCallback, useRef } from 'react';
import methodologyImageService from '@admin/services/methodologyImageService';

interface UseImageUploadReturn {
  handleImageUpload: ImageUploadHandler;
  handleImageUploadCancel: ImageUploadCancelHandler;
}

export default function useMethodologyImageUpload(
  methodologyId: string,
): UseImageUploadReturn {
  const promise = useRef<CancellablePromise<ImageUploadResult>>();

  const handleImageUpload: ImageUploadHandler = useCallback(
    async (file, updateProgress) => {
      promise.current = methodologyImageService
        .upload(methodologyId, file, updateProgress)
        .then() as CancellablePromise<ImageUploadResult>;

      return promise.current;
    },
    [methodologyId],
  );

  const handleImageUploadCancel: ImageUploadCancelHandler = useCallback(() => {
    promise.current?.cancel();
  }, []);

  return {
    handleImageUpload,
    handleImageUploadCancel,
  };
}
