import Command from '@ckeditor/ckeditor5-core/src/command';
import { markerTypes } from './constants';

/**
 * Replaces a placeholder marker with a comment marker using the id of the created comment.
 */
export default class AddCommentCommand extends Command {
  execute(options) {
    const { editor } = this;
    const range = editor.model.document.selection.getFirstRange();

    editor.model.change(writer => {
      if (editor.model.markers.has(markerTypes.commentPlaceholder)) {
        writer.removeMarker(markerTypes.commentPlaceholder);
      }

      writer.addMarker(`${markerTypes.comment}:${options.id}`, {
        affectsData: true,
        range,
        usingOperation: true,
      });
    });
  }
}
