import Command from '@ckeditor/ckeditor5-core/src/command';
import { markerTypes } from './constants';

/**
 * Toggles comment markers to or from resolved comment markers.
 */
export default class ToggleResolveCommentCommand extends Command {
  execute(options) {
    const { editor } = this;

    const markerName = options.resolved
      ? `${markerTypes.resolvedComment}:${options.id}`
      : `${markerTypes.comment}:${options.id}`;
    const marker = editor.model.markers.get(markerName);

    if (marker) {
      const range = marker.getRange();

      editor.model.change(writer => {
        writer.removeMarker(markerName);

        const newMarkerName = options.resolved
          ? `${markerTypes.comment}:${options.id}`
          : `${markerTypes.resolvedComment}:${options.id}`;

        writer.addMarker(newMarkerName, {
          affectsData: true,
          range,
          usingOperation: true,
        });
      });
    }
  }
}
