import Command from '@ckeditor/ckeditor5-core/src/command';
import { markerTypes } from './constants';

/**
 * Replaces a placeholder marker with a comment marker using the id of the created comment.
 */
export default class AddCommentCommand extends Command {
  execute(options) {
    const { editor } = this;

    editor.model.change(writer => {
      [...editor.model.markers].forEach(marker => {
        if (marker.name === markerTypes.commentPlaceholder) {
          writer.addMarker(`${markerTypes.comment}:${options.id}`, {
            affectsData: true,
            range: marker.getRange(),
            usingOperation: true,
          });

          writer.removeMarker(marker.name);
        }
      });
    });
  }
}
