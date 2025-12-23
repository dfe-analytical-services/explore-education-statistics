/**
 * @license Copyright (c) 2014-2021, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or https://ckeditor.com/legal/ckeditor-oss-license
 */
import {
  Autosave,
  Autoformat,
  BlockQuote,
  Bold,
  Essentials,
  Heading,
  Image,
  ImageCaption,
  ImageResize,
  ImageStyle,
  ImageToolbar,
  ImageUpload,
  Indent,
  Link,
  List,
  MediaEmbed,
  Paragraph,
  PasteFromOffice,
  Table,
  TableToolbar,
  TableCaption,
  TextTransformation,
  Alignment,
  ClassicEditor,
} from 'ckeditor5';
import Comments from '../plugins/comments/Comments';
import FeaturedTables from '../plugins/featuredTables/FeaturedTables';
import Glossary from '../plugins/glossary/Glossary';

class Editor extends ClassicEditor {}

// Plugins to include in the build.
Editor.builtinPlugins = [
  Alignment,
  Autoformat,
  Autosave,
  BlockQuote,
  Bold,
  Comments,
  Essentials,
  FeaturedTables,
  Glossary,
  Heading,
  Image,
  ImageCaption,
  ImageResize,
  ImageStyle,
  ImageToolbar,
  ImageUpload,
  Indent,
  Link,
  List,
  MediaEmbed,
  Paragraph,
  PasteFromOffice,
  Table,
  TableToolbar,
  TableCaption,
  TextTransformation,
];

export default Editor;
