import { FileRepository } from 'ckeditor5';
import { Editor, PluginName, PluginClass } from '@admin/types/ckeditor';
import CustomUploadAdapter, {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';

export default function customUploadAdapterPlugin(
  onUpload: ImageUploadHandler,
  onCancel?: ImageUploadCancelHandler,
) {
  return class CustomUploadAdapterPlugin implements PluginClass {
    get pluginName(): PluginName {
      return 'CustomUploadAdapter';
    }

    constructor(editor: Editor) {
      const fileRepository =
        editor.plugins.get<FileRepository>('FileRepository');

      fileRepository.createUploadAdapter = loader => {
        return new CustomUploadAdapter(loader, onUpload, onCancel);
      };
    }
  };
}
