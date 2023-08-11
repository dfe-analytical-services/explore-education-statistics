import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Adds the glossary link
 */
export default class AddGlossaryItemCommand extends Command {
  execute(item) {
    const { editor } = this;

    editor.model.change(writer => {
      const insertPosition = editor.model.document.selection.getFirstPosition();

      writer.insertText(
        item.text,
        {
          linkHref: item.url,
        },
        insertPosition,
      );
    });
  }
}
