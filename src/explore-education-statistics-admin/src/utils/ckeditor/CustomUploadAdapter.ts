import { UploadAdapter, ImageUploadResult } from '@admin/types/ckeditor';
import { FileLoader } from '@ckeditor/ckeditor5-upload';

export type ImageProgressHandler = (current: number, total: number) => void;

export type ImageUploadHandler = (
  file: File,
  updateProgress: ImageProgressHandler,
) => Promise<ImageUploadResult>;

export type ImageUploadCancelHandler = () => void;

export default class CustomUploadAdapter implements UploadAdapter {
  private request?: Promise<ImageUploadResult>;

  constructor(
    private readonly loader: FileLoader,
    private readonly onUpload: ImageUploadHandler,
    private readonly onCancel?: ImageUploadCancelHandler,
  ) {}

  async upload(): Promise<ImageUploadResult> {
    const file = await this.loader.file;

    this.request = this.onUpload(file, this.handleProgress);

    return this.request;
  }

  abort(): void {
    this.onCancel?.();
  }

  private handleProgress: ImageProgressHandler = (current, total) => {
    this.loader.uploaded = current;
    this.loader.uploadTotal = total;
  };
}
