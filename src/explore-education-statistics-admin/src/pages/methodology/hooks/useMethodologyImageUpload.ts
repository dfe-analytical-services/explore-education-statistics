import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import { useCallback, useRef } from 'react';
import methodologyImageService from '@admin/services/methodologyImageService';

interface UseImageUploadReturn {
  handleImageUpload: ImageUploadHandler;
  handleImageUploadCancel: ImageUploadCancelHandler;
}

export default function useMethodologyImageUpload(
  methodologyId: string,
): UseImageUploadReturn {
  const abortController = useRef<AbortController>();

  const handleImageUpload: ImageUploadHandler = useCallback(
    async (file, onProgress) => {
      abortController.current = new AbortController();

      return methodologyImageService.upload(methodologyId, file, {
        signal: abortController.current.signal,
        onProgress,
      });
    },
    [methodologyId],
  );

  const handleImageUploadCancel: ImageUploadCancelHandler = useCallback(() => {
    abortController.current?.abort();
  }, []);

  return {
    handleImageUpload,
    handleImageUploadCancel,
  };
}
