import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Adds a placeholder marker while a comment is being added.
 */
export default class AddCommentPlaceholderCommand extends Command {
  refresh() {
    const { editor } = this;
    const range = editor.model.document.selection.getFirstRange();
    const items = [...range.getItems()];

    // Can select an empty range by double clicking at the end of a line.
    const isEmptyRange =
      items.length === 1 &&
      (items[0].name === 'paragraph' || items[0].name === 'softBreak') &&
      !items[0].textNode;

    // Only enable the button when a range with content is selected.
    this.isEnabled =
      !editor.model.document.selection.isCollapsed && !isEmptyRange;
  }

  execute() {
    const { editor } = this;
    const range = editor.model.document.selection.getFirstRange();

    editor.model.change(writer => {
      writer.addMarker(`commentplaceholder`, {
        affectsData: true,
        range,
        usingOperation: true,
      });
    });
  }
}
