import Plugin from '@ckeditor/ckeditor5-core/src/plugin';

import GlossaryEditing from './GlossaryEditing';
import GlossaryUI from './GlossaryUi';

export default class Glossary extends Plugin {
  static get pluginName() {
    return 'Glossary';
  }

  static get requires() {
    return [GlossaryEditing, GlossaryUI];
  }

  /**
   * These commands can be called from the host app. When the editor is ready assign `glossaryPlugin = editor.plugins.get('Glossary');`
   */
  /**
   * Creates a link to a glossary item
   * glossaryPlugin.addGlossaryItem(item)
   * @param {text: string; url: string} item
   */
  addGlossaryItem(item) {
    this.editor.execute('addGlossaryItem', item);
  }
}
