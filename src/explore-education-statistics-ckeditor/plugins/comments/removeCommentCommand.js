import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Removes a comment marker or placeholder marker.
 */
export default class RemoveCommentCommand extends Command {
  execute(options) {
    this.editor.model.change(() => {
      this.commentName =
        options.id === 'commentplaceholder'
          ? 'commentplaceholder'
          : `comment:${options.id}`;
    });
  }
}
