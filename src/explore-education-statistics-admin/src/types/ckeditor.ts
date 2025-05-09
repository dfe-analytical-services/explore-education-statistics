// Declare our own custom CKEditor types that aren't
// exposed by any of the packages that we consume.

// https://ckeditor.com/docs/ckeditor5/latest/api/module_core_editor_editor-Editor.html
import { Dictionary } from '@common/types';

export interface EditorClass {
  new (config: EditorConfig): Editor;
}

export interface Editor {
  plugins: PluginCollection;
  editing: {
    view: {
      document: Document;
      focus(): void;
      change(callback: (writer: DowncastWriter) => void): void;
    };
  };
  model: Model;
  getData(): string;
}

export interface CommentsPluginConfig {
  addComment: () => void;
  commentCancelled: () => void;
  commentRemoved: (markerId: string) => void;
  commentSelected: (markerId?: string) => void;
  undoRedoComment: (type: CommentUndoRedoActions, markerId: string) => void;
}

export interface FeaturedTableLink {
  url: string;
  text: string;
}

export interface FeaturedTablesPluginConfig {
  addFeaturedTableLink: (link: FeaturedTableLink) => void;
}

export interface GlossaryItem {
  url: string;
  text: string;
}

export interface GlossaryPluginConfig {
  addGlossaryItem: (item: GlossaryItem) => void;
}

export interface AutoSavePluginConfig {
  save: () => void;
  waitingTime: number;
}

export interface AlignmentFormat {
  className?: string;
  name: 'left' | 'right' | 'center' | 'justify';
}

export interface AlignmentConfig {
  options: AlignmentFormat['name'][];
}

export interface EditorConfig {
  toolbar: (ToolbarOption | ToolbarGroup)[];
  extraPlugins?: Plugin[];
  image?: {
    toolbar: string[];
    resizeOptions?: ResizeOption[];
  };
  table?: {
    contentToolbar?: string[];
  };
  heading?: {
    options: HeadingOption[];
  };
  link?: {
    decorators: Dictionary<LinkDecoratorAutomatic | LinkDecoratorManual>;
  };
  comments?: CommentsPluginConfig;
  autosave?: AutoSavePluginConfig;
  alignment?: AlignmentConfig;
  featuredTables?: FeaturedTablesPluginConfig;
  glossary?: GlossaryPluginConfig;
}

export interface PluginCollection {
  get<T extends Plugin>(key: string): T;
  has<T extends Plugin>(key: string): T;
}

export interface PluginClass {
  get pluginName(): PluginName;
}

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface Plugin {}

export interface CommentsPlugin extends Plugin {
  addCommentMarker(id: string): void;
  removeCommentMarker(id: string): void;
  resolveCommentMarker(id: string, resolved: boolean): void;
  selectCommentMarker(id: string): void;
}

export interface FeaturedTablesPlugin extends Plugin {
  addFeaturedTableLink(link: FeaturedTableLink): void;
}

export interface GlossaryPlugin extends Plugin {
  addGlossaryItem(item: GlossaryItem): void;
}

export type CommentUndoRedoActions =
  | 'undoRemoveComment'
  | 'undoAddComment'
  | 'redoAddComment'
  | 'undoResolveComment'
  | 'redoUnresolveComment'
  | 'undoUnresolveComment'
  | 'redoResolveComment'
  | 'redoRemoveComment';

export interface LinkDecoratorAutomatic {
  mode: 'automatic';
  callback: (url: string) => boolean | RegExpMatchArray | null;
  attributes: Dictionary<string>;
  styles?: Dictionary<string>;
  classes?: string | string[];
}

export interface LinkDecoratorManual {
  mode: 'manual';
  defaultValue: boolean;
  label: string;
  attributes: Dictionary<string>;
  styles?: Dictionary<string>;
  classes?: string | string[];
}

export interface HeadingOption {
  model:
    | 'heading1'
    | 'heading2'
    | 'heading3'
    | 'heading4'
    | 'heading5'
    | 'paragraph';
  view?: 'h1' | 'h2' | 'h3' | 'h4' | 'h5';
  title: string;
  class?: string;
}

export interface ResizeOption {
  name: string;
  value: string | null;
  label: string;
  icon?: string;
}

// https://ckeditor.com/docs/ckeditor5/latest/framework/guides/deep-dive/upload-adapter.html#the-anatomy-of-the-adapter
export interface UploadAdapter {
  upload(): Promise<ImageUploadResult>;
  abort(): void;
}

export interface ImageUploadResult {
  /**
   * The default url for the image that
   * was uploaded to the server.
   */
  default: string;

  /**
   * Additional urls can be added for different
   * image sizes. This allows us to show more appropriately
   * sized images for the user's screen size.
   *
   * Ideally, we should resize images to multiple
   * sizes for an optimized user experience.
   */
  [size: string]: string;
}

// https://ckeditor.com/docs/ckeditor5/latest/api/module_engine_model_position-Position.html
interface Position {
  readonly index: number;
  isAfter(otherPosition: Position): boolean;
  isBefore(otherPosition: Position): boolean;
}

// https://ckeditor.com/docs/ckeditor5/latest/api/module_engine_model_markercollection-Marker.html
export interface Marker {
  readonly name: string;
  getStart(): Position;
}

// https://ckeditor.com/docs/ckeditor5/latest/api/module_engine_model_model-Model.html
export interface Model {
  readonly markers: Marker[];
  readonly document: Document;
}

// https://ckeditor.com/docs/ckeditor5/latest/api/module_engine_view_downcastwriter-DowncastWriter.html
export interface DowncastWriter {
  readonly document: Document;
  setAttribute(key: string, value: string, element: Element): void;
  removeAttribute(key: string, element: Element): void;
}

// https://ckeditor.com/docs/ckeditor5/latest/api/module_engine_model_document-Document.html
export interface Document {
  getRoot(name?: string): RootElement;
}

export interface Node {
  readonly endOffset: number | null;
  readonly index: number | null;
  readonly isEmpty: boolean;
  readonly name: string;
  readonly nextSibling: Node | null;
  readonly offsetSize: number;
  readonly parent: Element | null;
  readonly previousSibling: Node | null;
  readonly root: Node;
  readonly startOffset: number | null;

  getAttribute<T>(key: string): T | undefined;
  getAttributeKeys(): Iterable<string>;
  hasAttribute(key: string): boolean;
  toJSON(): Record<string, unknown>;
}

export interface Element extends Node {
  readonly name: string;

  getChild(index: number): Node;
  getChildren(): Iterable<Node>;
}

export interface RootElement extends Element {
  readonly document: Document;

  getChild(index: number): Element;
  getChildren(): Iterable<Element>;
}

export type PluginName =
  | 'Alignment'
  | 'Autoformat'
  | 'Autosave'
  | 'BlockQuote'
  | 'Bold'
  | 'Comments'
  | 'CustomUploadAdapter'
  | 'Essentials'
  | 'Glossary'
  | 'Heading'
  | 'Image'
  | 'ImageCaption'
  | 'ImageResize'
  | 'ImageStyle'
  | 'ImageToolbar'
  | 'ImageUpload'
  | 'Indent'
  | 'Link'
  | 'List'
  | 'MediaEmbed'
  | 'Paragraph'
  | 'PasteFromOffice'
  | 'Table'
  | 'TableToolbar'
  | 'TableCaption'
  | 'TextTransformation';

export type ToolbarOption =
  | '|'
  | 'accessibilityHelp'
  | 'alignment'
  | 'blockQuote'
  | 'bold'
  | 'bulletedList'
  | 'comment'
  | 'featuredTables'
  | 'glossary'
  | 'heading'
  | 'imageUpload'
  | 'insertTable'
  | 'link'
  | 'numberedList'
  | 'redo'
  | 'toggleTableCaption'
  | 'undo';

export interface ToolbarGroup {
  label: string;
  icon: string;
  items: ToolbarOption[];
}

// Creating types for the result of calling `toJson` on
// ckEditor Elements to make it easy to deal with.
// The actual type is `Record<string, unknown>`.
export interface JsonElement {
  name: string;
  attributes?: JsonElementAttributes;
  children?: JsonElementChild[];
}

export interface JsonElementChild {
  name?: string;
  attributes?: JsonElementAttributes;
  data?: string;
  children?: JsonElementChild[];
}

// An uncomprehensive list of known attributes on Json Elements
interface JsonElementAttributes {
  alt?: string;
  bold?: boolean;
  headingColumns?: number;
  headingRows?: number;
  linkHref?: string;
  linkOpenInNewTab?: boolean;
  src?: string;
}
