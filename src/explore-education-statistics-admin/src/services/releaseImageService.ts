import client from '@admin/services/utils/service';
import { ImageUploadResult } from '@admin/types/ckeditor';
import { ImageProgressHandler } from '@admin/utils/ckeditor/CustomUploadAdapter';
import { CancellablePromise } from '@common/types/promise';
import { AxiosProgressEvent } from 'axios';

const releaseImageService = {
  upload(
    releaseId: string,
    file: File,
    onProgress?: ImageProgressHandler,
  ): CancellablePromise<ImageUploadResult> {
    const data = new FormData();
    data.append('file', file);

    return client.post(`/releases/${releaseId}/images`, data, {
      onUploadProgress(event: AxiosProgressEvent) {
        onProgress?.(event.loaded, event.total ?? 0);
      },
    });
  },
};

export default releaseImageService;
