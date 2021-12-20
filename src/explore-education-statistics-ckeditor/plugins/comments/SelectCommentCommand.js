import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Sets the name of the comment that has been selected by the host app.
 */
export default class SelectCommentCommand extends Command {
  execute(options) {
    this.editor.model.change(() => {
      this.commentName = `comment:${options.id}`;
    });
  }
}
