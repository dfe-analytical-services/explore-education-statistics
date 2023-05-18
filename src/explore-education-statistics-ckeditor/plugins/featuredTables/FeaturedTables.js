import Plugin from '@ckeditor/ckeditor5-core/src/plugin';

import FeaturedTablesEditing from './FeaturedTablesEditing';
import FeaturedTablesUI from './FeaturedTablesUi';

export default class FeaturedTables extends Plugin {
  static get pluginName() {
    return 'FeaturedTables';
  }

  static get requires() {
    return [FeaturedTablesEditing, FeaturedTablesUI];
  }
}
