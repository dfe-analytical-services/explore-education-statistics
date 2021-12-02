import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Toggles comment markers to or from resolved comment markers.
 */
export default class ToggleResolveCommentCommand extends Command {
  execute(options) {
    const { editor } = this;

    const markerName = options.resolved
      ? `resolvedcomment:${options.id}`
      : `comment:${options.id}`;
    const marker = editor.model.markers.get(markerName);

    if (marker) {
      const range = marker.getRange();

      editor.model.change(writer => {
        writer.removeMarker(markerName);

        const newMarkerName = options.resolved
          ? `comment:${options.id}`
          : `resolvedcomment:${options.id}`;

        writer.addMarker(newMarkerName, {
          affectsData: true,
          range,
          usingOperation: true,
        });
      });
    }
  }
}
