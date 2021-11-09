import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Adds a placeholder marker while a comment is being added.
 */
export default class AddCommentPlaceholderCommand extends Command {
  refresh() {
    const { editor } = this;
    const range = editor.model.document.selection.getFirstRange();
    const items = range.getItems();
    const itemsArray = [];
    [...items].forEach(item => itemsArray.push(item));
    const item = itemsArray[0];

    // Can select an empty range by double clicking at the end of a line.
    const isEmptyRange =
      itemsArray.length === 1 &&
      item.name &&
      (item.name === 'paragraph' || item.name === 'softBreak') &&
      !item.textNode;

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
