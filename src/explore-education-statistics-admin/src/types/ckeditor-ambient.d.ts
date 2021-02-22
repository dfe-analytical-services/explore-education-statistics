/* eslint-disable import/no-duplicates */

// Life is pain. CKEditor still doesn't have type declarations.
// See: https://github.com/ckeditor/ckeditor5/issues/504

// Real CKEditor module declarations (for values that
// we can actually import) should go here.

declare module 'explore-education-statistics-ckeditor' {
  import { EditorClass, EditorConfig } from '@admin/types/ckeditor';

  // https://ckeditor.com/docs/ckeditor5/latest/api/module_editor-classic_classiceditor-ClassicEditor.html
  interface CustomEditor extends EditorClass {
    new (
      element: string | HTMLElement,
      config: EditorConfig,
    ): CustomEditorInstance;

    create(element: string | HTMLElement): Promise<CustomEditorInstance>;
  }

  interface CustomEditorInstance {
    destroy: () => void;
  }

  const editor: CustomEditor;
  export default editor;
}

declare module '@ckeditor/ckeditor5-react' {
  import { EditorClass, EditorConfig, Editor } from '@admin/types/ckeditor';
  import { ChangeEvent, ReactElement } from 'react';

  export interface CKEditorProps {
    editor: EditorClass;
    config?: EditorConfig;
    data: string;
    // TODO: Don't think this actually emits `ChangeEvent`
    onChange: (event: ChangeEvent, editor: Editor) => void;
    onFocus: () => void;
    onBlur: () => void;
    onReady: (editor: Editor) => void;
  }

  export const CKEditor: (props: CKEditorProps) => ReactElement<CKEditorProps>;
}

declare module '@ckeditor/ckeditor5-upload' {
  import { Plugin, UploadAdapter } from '@admin/types/ckeditor';

  // https://ckeditor.com/docs/ckeditor5/latest/api/module_upload_filerepository-FileLoader.html
  export interface FileLoader {
    id: number;
    file: Promise<File>;
    data?: File;
    status: 'idle' | 'reading' | 'uploading' | 'aborted' | 'error';
    uploaded: number;
    uploadTotal: number | null;
    uploadedPercent: number;

    abort(): void;
    read(): Promise<string>;
  }

  // https://ckeditor.com/docs/ckeditor5/latest/api/module_upload_filerepository-FileRepository.html
  export interface FileRepository extends Plugin {
    createUploadAdapter(loader: FileLoader): UploadAdapter;
  }
}
