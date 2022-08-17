import { HeadingOption, ResizeOption } from '@admin/types/ckeditor';
import { Dictionary } from '@admin/types';

export const toolbarConfigs: Dictionary<string[]> = {
  full: [
    'heading',
    '|',
    'bold',
    'italic',
    'link',
    '|',
    'bulletedList',
    'numberedList',
    '|',
    'blockQuote',
    'insertTable',
    'toggleTableCaption',
    'imageUpload',
    '|',
    'redo',
    'undo',
    '|',
    'comment',
  ],
  simple: [
    'bold',
    'italic',
    'link',
    '|',
    'bulletedList',
    'numberedList',
    '|',
    'redo',
    'undo',
  ],
};

export const defaultAllowedHeadings: string[] = ['h3', 'h4', 'h5'];

export const headingOptions: HeadingOption[] = [
  {
    model: 'heading1',
    view: 'h1',
    title: 'Heading 1',
    class: 'ck-heading_heading1',
  },
  {
    model: 'heading2',
    view: 'h2',
    title: 'Heading 2',
    class: 'ck-heading_heading2',
  },
  {
    model: 'heading3',
    view: 'h3',
    title: 'Heading 3',
    class: 'ck-heading_heading3',
  },
  {
    model: 'heading4',
    view: 'h4',
    title: 'Heading 4',
    class: 'ck-heading_heading4',
  },
  {
    model: 'heading5',
    view: 'h5',
    title: 'Heading 5',
    class: 'ck-heading_heading5',
  },
];

export const imageToolbar: string[] = [
  'imageTextAlternative',
  '|',
  'imageResize:50',
  'imageResize:75',
  'imageResize:original',
];

export const resizeOptions: ResizeOption[] = [
  {
    name: 'imageResize:original',
    value: null,
    label: 'Original',
    icon: 'original',
  },
  {
    name: 'imageResize:50',
    value: '50',
    label: '50%',
    icon: 'medium',
  },
  {
    name: 'imageResize:75',
    value: '75',
    label: '75%',
    icon: 'large',
  },
];

export const tableContentToolbar: string[] = [
  'tableColumn',
  'tableRow',
  'mergeTableCells',
];
