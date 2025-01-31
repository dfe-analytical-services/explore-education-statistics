import Plugin from '@ckeditor/ckeditor5-core/src/plugin';

import CommentsEditing from './CommentsEditing';
import CommentsUI from './CommentsUi';

export default class Comments extends Plugin {
  static get pluginName() {
    return 'Comments';
  }

  static get requires() {
    return [CommentsEditing, CommentsUI];
  }

  /**
   * These commands can be called from the host app. When the editor is ready assign `commentsPlugin = editor.plugins.get('Comments');`
   */
  /**
   * Creates a comment marker with the id of the created comment.
   * commentsPlugin.addComment(id)
   * @param {string} id
   */
  addCommentMarker(id) {
    this.editor.execute('addComment', { id });
  }

  /**
   * Select the comment marker with the given id.
   * @param {string} id
   */
  selectCommentMarker(id) {
    this.editor.execute('selectComment', { id });
  }

  /**
   * Remove the comment marker with the given id.
   * @param {string} id
   */
  removeCommentMarker(id) {
    this.editor.execute('removeComment', { id });
  }

  /**
   * Resolve / unresolve a comment marker.
   * @param {string} id
   * @param {boolean} resolved
   */
  resolveCommentMarker(id, resolved) {
    this.editor.execute('resolveComment', { id, resolved });
  }
}
