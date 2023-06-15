import client from '@admin/services/utils/service';
import { ImageUploadResult } from '@admin/types/ckeditor';
import { ImageProgressHandler } from '@admin/utils/ckeditor/CustomUploadAdapter';
import { CancellablePromise } from '@common/types/promise';
import { AxiosProgressEvent } from 'axios';

const methodologyImageService = {
  upload(
    methodologyId: string,
    file: File,
    onProgress?: ImageProgressHandler,
  ): CancellablePromise<ImageUploadResult> {
    const data = new FormData();
    data.append('file', file);

    return client.post(`/methodologies/${methodologyId}/images`, data, {
      onUploadProgress(event: AxiosProgressEvent) {
        onProgress?.(event.loaded, event.total as number);
      },
    });
  },
};

export default methodologyImageService;
