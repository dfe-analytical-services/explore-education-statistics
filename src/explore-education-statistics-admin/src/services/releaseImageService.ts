import client from '@admin/services/utils/service';
import { ImageUploadResult } from '@admin/types/ckeditor';
import { ImageProgressHandler } from '@admin/utils/ckeditor/CustomUploadAdapter';
import { AxiosProgressEvent } from 'axios';

const releaseImageService = {
  upload(
    releaseId: string,
    file: File,
    options: {
      signal?: AbortSignal;
      onProgress?: ImageProgressHandler;
    } = {},
  ): Promise<ImageUploadResult> {
    const data = new FormData();
    data.append('file', file);

    return client.post(`/releases/${releaseId}/images`, data, {
      signal: options.signal,
      onUploadProgress(event: AxiosProgressEvent) {
        options.onProgress?.(event.loaded, event.total ?? 0);
      },
    });
  },
};

export default releaseImageService;
