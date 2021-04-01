import client from '@admin/services/utils/service';
import { ImageUploadResult } from '@admin/types/ckeditor';
import { ImageProgressHandler } from '@admin/utils/ckeditor/CustomUploadAdapter';
import { CancellablePromise } from '@common/types/promise';

const methodologyImageService = {
  upload(
    methodologyId: string,
    file: File,
    onProgress?: ImageProgressHandler,
  ): CancellablePromise<ImageUploadResult> {
    const data = new FormData();
    data.append('file', file);

    return client.post(`/methodologies/${methodologyId}/images`, data, {
      onUploadProgress(event: ProgressEvent) {
        onProgress?.(event.loaded, event.total);
      },
    });
  },
};

export default methodologyImageService;
