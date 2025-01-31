import Command from '@ckeditor/ckeditor5-core/src/command';
import { markerTypes } from './constants';

/**
 * Removes a comment marker or placeholder marker.
 */
export default class RemoveCommentCommand extends Command {
  execute(options) {
    const { editor } = this;

    const commentName =
      options.id === markerTypes.commentPlaceholder
        ? markerTypes.commentPlaceholder
        : `${markerTypes.comment}:${options.id}`;

    if (editor.model.markers.has(commentName)) {
      editor.model.change(writer => writer.removeMarker(commentName));
    }
  }
}
