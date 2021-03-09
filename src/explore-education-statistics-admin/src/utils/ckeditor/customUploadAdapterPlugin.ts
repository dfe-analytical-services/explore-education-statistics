import { Editor } from '@admin/types/ckeditor';
import CustomUploadAdapter, {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import { FileRepository } from '@ckeditor/ckeditor5-upload';

export default function customUploadAdapterPlugin(
  onUpload: ImageUploadHandler,
  onCancel?: ImageUploadCancelHandler,
) {
  return class CustomUploadAdapterPlugin {
    constructor(editor: Editor) {
      const fileRepository = editor.plugins.get<FileRepository>(
        'FileRepository',
      );

      fileRepository.createUploadAdapter = loader => {
        return new CustomUploadAdapter(loader, onUpload, onCancel);
      };
    }
  };
}
