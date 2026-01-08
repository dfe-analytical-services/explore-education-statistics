import { Plugin } from 'ckeditor5';
import AddGlossaryItemCommand from './AddGlossaryItemCommand';

export default class GlossaryEditing extends Plugin {
  constructor(editor) {
    super(editor);
    this.config = editor.config.get('glossary');
  }

  init() {
    if (!this.config) {
      return;
    }
    const { editor } = this;

    /**
     * Set up commands
     */
    editor.commands.add('addGlossaryItem', new AddGlossaryItemCommand(editor));
  }
}
