/* eslint-disable no-underscore-dangle */
import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import AddFeaturedTableCommand from './AddFeaturedTableCommand';

export default class FeaturedTablesEditing extends Plugin {
  constructor(editor) {
    super(editor);
    this.config = editor.config.get('featuredTables');
  }

  init() {
    // this._defineSchema();
    // this._defineConverters();

    if (!this.config) {
      return;
    }
    const { editor } = this;

    editor.commands.add(
      'addFeaturedTable',
      new AddFeaturedTableCommand(editor),
    );
  }
}
