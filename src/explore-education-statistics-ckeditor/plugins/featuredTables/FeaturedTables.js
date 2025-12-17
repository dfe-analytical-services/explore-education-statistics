import { Plugin } from 'ckeditor5';
import FeaturedTablesEditing from './FeaturedTablesEditing';
import FeaturedTablesUI from './FeaturedTablesUi';

export default class FeaturedTables extends Plugin {
  static get pluginName() {
    return 'FeaturedTables';
  }

  static get requires() {
    return [FeaturedTablesEditing, FeaturedTablesUI];
  }

  /**
   * These commands can be called from the host app. When the editor is ready assign `featuredTablesPlugin = editor.plugins.get('FeaturedTables');`
   */
  /**
   * Creates a link to a glossary item
   * featuredTablesPlugin.addFeaturedTableLink(item)
   * @param {text: string; url: string} item
   */
  addFeaturedTableLink(item) {
    this.editor.execute('addFeaturedTableLink', item);
  }
}
