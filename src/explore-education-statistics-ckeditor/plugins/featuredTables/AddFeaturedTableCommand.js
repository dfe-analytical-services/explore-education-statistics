import Command from '@ckeditor/ckeditor5-core/src/command';

// not used yet

/**
 * Replaces a placeholder marker with a comment marker using the id of the created comment.
 */
export default class AddFeaturedTableCommand extends Command {
  execute(selectedTable) {
    const { editor } = this;
    const fastTrackUrl = 'http://localhost:3000/data-tables/fast-track/'; // to do pass in as config

    editor.model.change(writer => {
      const insertPosition = editor.model.document.selection.getFirstPosition();

      writer.insertText(
        selectedTable.label,
        {
          linkHref: `${fastTrackUrl}${selectedTable.id}`,
        },
        insertPosition,
      );
    });
  }
}
