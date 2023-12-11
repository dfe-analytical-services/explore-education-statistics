import Command from '@ckeditor/ckeditor5-core/src/command';

/**
 * Adds the featured table link
 */
export default class AddFeaturedTableLinkCommand extends Command {
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
