import releaseImageService from '@admin/services/releaseImageService';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import { useCallback, useRef } from 'react';

interface UseImageUploadReturn {
  handleImageUpload: ImageUploadHandler;
  handleImageUploadCancel: ImageUploadCancelHandler;
}

export default function useReleaseImageUpload(
  releaseVersionId: string,
): UseImageUploadReturn {
  const abortController = useRef<AbortController>(null);

  const handleImageUpload: ImageUploadHandler = useCallback(
    async (file, onProgress) => {
      abortController.current = new AbortController();

      return releaseImageService.upload(releaseVersionId, file, {
        signal: abortController.current.signal,
        onProgress,
      });
    },
    [releaseVersionId],
  );

  const handleImageUploadCancel: ImageUploadCancelHandler = useCallback(() => {
    abortController.current?.abort();
  }, []);

  return {
    handleImageUpload,
    handleImageUploadCancel,
  };
}
