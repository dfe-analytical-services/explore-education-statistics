import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import AddFeaturedTableLinkCommand from './AddFeaturedTableLinkCommand';

export default class FeaturedTablesEditing extends Plugin {
  constructor(editor) {
    super(editor);
    this.config = editor.config.get('featuredTables');
  }

  init() {
    if (!this.config) {
      return;
    }
    const { editor } = this;

    /**
     * Set up commands
     */
    editor.commands.add(
      'addFeaturedTableLink',
      new AddFeaturedTableLinkCommand(editor),
    );
  }
}
