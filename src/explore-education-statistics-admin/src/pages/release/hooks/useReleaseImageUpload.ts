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
  releaseId: string,
): UseImageUploadReturn {
  const abortController = useRef<AbortController>();

  const handleImageUpload: ImageUploadHandler = useCallback(
    async (file, onProgress) => {
      abortController.current = new AbortController();

      return releaseImageService.upload(releaseId, file, {
        signal: abortController.current.signal,
        onProgress,
      });
    },
    [releaseId],
  );

  const handleImageUploadCancel: ImageUploadCancelHandler = useCallback(() => {
    abortController.current?.abort();
  }, []);

  return {
    handleImageUpload,
    handleImageUploadCancel,
  };
}
