import { Command } from 'ckeditor5';
import { markerTypes } from './constants';

/**
 * Sets the name of the comment that has been selected by the host app.
 */
export default class SelectCommentCommand extends Command {
  execute(options) {
    this.editor.model.change(() => {
      this.commentName = `${markerTypes.comment}:${options.id}`;
    });
  }
}
