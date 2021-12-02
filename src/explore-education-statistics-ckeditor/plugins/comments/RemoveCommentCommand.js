import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Removes a comment marker or placeholder marker.
 */
export default class RemoveCommentCommand extends Command {
  execute(options) {
    const { editor } = this;

    const commentName =
      options.id === 'commentplaceholder'
        ? 'commentplaceholder'
        : `comment:${options.id}`;

    if (editor.model.markers.has(commentName)) {
      editor.model.change(writer => writer.removeMarker(commentName));
    }
  }
}
