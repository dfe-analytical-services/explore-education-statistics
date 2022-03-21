import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import { scrollViewportToShowTarget } from '@ckeditor/ckeditor5-utils/src/dom/scroll';
import AddCommentCommand from './AddCommentCommand';
import AddCommentPlaceholderCommand from './AddCommentPlaceholderCommand';
import SelectCommentCommand from './SelectCommentCommand';
import RemoveCommentCommand from './RemoveCommentCommand';
import ToggleResolveCommentCommand from './ToggleResolveCommentCommand';
import { markerTypes } from './constants';

export default class CommentsEditing extends Plugin {
  constructor(editor) {
    super(editor);
    this.config = editor.config.get('comments');
    this.selectedComment = '';
    this.removedMarkers = [];
  }

  init() {
    if (!this.config) {
      return;
    }
    const { editor } = this;
    const viewDocument = editor.editing.view.document;

    this.defineSchema();
    this.defineConverters();

    /**
     * Set up commands
     */
    editor.commands.add('addComment', new AddCommentCommand(editor));
    editor.commands.add(
      'addCommentPlaceholder',
      new AddCommentPlaceholderCommand(editor),
    );
    const selectCommentCommand = new SelectCommentCommand(editor);
    editor.commands.add('selectComment', selectCommentCommand);
    const removeCommentCommand = new RemoveCommentCommand(editor);
    editor.commands.add('removeComment', removeCommentCommand);
    editor.commands.add(
      'resolveComment',
      new ToggleResolveCommentCommand(editor),
    );

    editor.on('ready', () => {
      /**
       * Watch for undo operations we need to handle.
       */
      editor.commands.get('undo').on('execute', () => {
        const docVersion = editor.model.document.version;
        const lastOperation = editor.model.document.history.getOperation(
          docVersion - 1,
        );

        if (lastOperation.name?.startsWith(`${markerTypes.comment}:`)) {
          if (
            lastOperation.batch.operations[0].name?.startsWith(
              `${markerTypes.resolvedComment}:`,
            )
          ) {
            this.config.undoRedoComment(
              'undoResolveComment',
              lastOperation.name,
            );
            return;
          }

          if (
            lastOperation.batch.operations.length === 1 &&
            !lastOperation.newRange
          ) {
            this.config.undoRedoComment(
              'undoAddComment',
              lastOperation.batch.operations[0].name,
            );
            return;
          }

          this.removedMarkers = this.removedMarkers.filter(
            marker => marker !== lastOperation.name,
          );
          this.config.undoRedoComment('undoRemoveComment', lastOperation.name);
          return;
        }

        if (lastOperation.name?.startsWith(`${markerTypes.resolvedComment}:`)) {
          this.config.undoRedoComment(
            'undoUnresolveComment',
            lastOperation.name,
          );
        }
      });

      /**
       * Watch for redo operations we need to handle.
       */
      editor.commands.get('redo').on('execute', () => {
        const docVersion = editor.model.document.version;
        const lastOperation = editor.model.document.history.getOperation(
          docVersion - 1,
        );

        if (lastOperation.name?.startsWith(`${markerTypes.comment}:`)) {
          if (
            lastOperation.batch.operations[0].name?.startsWith(
              `${markerTypes.resolvedComment}:`,
            )
          ) {
            this.config.undoRedoComment(
              'redoUnresolveComment',
              lastOperation.name,
            );
            return;
          }

          if (
            lastOperation.batch.operations.length === 1 &&
            !lastOperation.oldRange
          ) {
            this.config.undoRedoComment('redoAddComment', lastOperation.name);
            return;
          }

          this.config.undoRedoComment('redoRemoveComment', lastOperation.name);
          return;
        }

        if (lastOperation.name?.startsWith(`${markerTypes.resolvedComment}:`)) {
          this.config.undoRedoComment('redoResolveComment', lastOperation.name);
        }
      });
    });

    /**
     * Set the selected comment and update the marker when the select comment command is executed.
     */
    this.listenTo(selectCommentCommand, 'execute', () => {
      this.selectedComment = selectCommentCommand.commentName;
      const markerCollection = [...editor.model.markers];
      let selectedMarker;
      markerCollection.forEach(marker => {
        if (
          marker.name.startsWith(`${markerTypes.comment}:`) &&
          marker.name === selectCommentCommand.commentName
        ) {
          selectedMarker = marker;
        }
        editor.model.change(writer => writer.updateMarker(marker));
      });
      if (selectedMarker) {
        const viewRange = editor.editing.mapper.toViewRange(
          selectedMarker.getRange(),
        );
        const domRange = editor.editing.view.domConverter.viewRangeToDom(
          viewRange,
        );
        scrollViewportToShowTarget({ target: domRange, viewportOffset: 100 });
      }
    });

    /**
     * Watch for removing data in the editor and if it's removing a marker send message to host app.
     */
    editor.model.document.registerPostFixer(() => {
      const changes = editor.model.document.differ.getChanges();
      const changedMarkers = editor.model.document.differ.getChangedMarkers();

      changes.forEach(change => {
        if (change.type === 'remove' && changedMarkers.length) {
          changedMarkers.forEach(changedMarker => {
            // If it's not a comment marker or already being removed, do nothing
            if (
              !changedMarker.name.startsWith(`${markerTypes.comment}:`) ||
              this.removedMarkers.includes(changedMarker.name)
            ) {
              return;
            }

            // Marker has been deleted
            if (
              changedMarker.data.newRange?.start.root.rootName ===
                '$graveyard' &&
              changedMarker.data.newRange?.end.root.rootName === '$graveyard'
            ) {
              this.removedMarkers.push(changedMarker.name);
              this.config.commentRemoved(changedMarker.name);
              return;
            }

            // All the content of the marker has been deleted
            const marker = editor.model.markers.get(changedMarker.name);
            if (marker?.getRange().isCollapsed) {
              this.removedMarkers.push(marker.name);
              this.config.commentRemoved(marker.name);
              editor.model.change(writer => {
                writer.removeMarker(marker.name);
              });
            }
          });
        }
      });
    });

    /**
     * Send message to host app and update the marker when click on a comment marker.
     */
    this.listenTo(viewDocument, 'click', () => {
      const { model } = editor;
      const { document } = model;
      const placeholderMarkerName = markerTypes.commentPlaceholder;
      let selectedMarkerName = '';

      [...model.markers].forEach(marker => {
        if (
          (marker.name.startsWith(`${markerTypes.comment}:`) ||
            marker.name === placeholderMarkerName) &&
          isInMarker(document.selection, marker)
        ) {
          selectedMarkerName = marker.name;
        }
      });

      const shouldUpdateMarkers =
        (selectedMarkerName && selectedMarkerName !== this.selectedComment) ||
        (!selectedMarkerName && this.selectedComment);

      this.config.commentSelected(selectedMarkerName);
      this.selectedComment = selectedMarkerName;

      // Remove add comment placeholder if click outside it.
      if (
        model.markers.has(placeholderMarkerName) &&
        selectedMarkerName !== placeholderMarkerName
      ) {
        editor.model.change(writer =>
          writer.removeMarker(placeholderMarkerName),
        );
        this.config.commentCancelled();
      }

      if (shouldUpdateMarkers) {
        [...model.markers].forEach(marker => {
          editor.model.change(writer => writer.updateMarker(marker.name));
        });
      }
    });
  }

  defineConverters() {
    const { conversion } = this.editor;

    // Comment markers
    conversion.for('editingDowncast').markerToHighlight({
      model: markerTypes.comment,
      view: data => {
        const classes = ['commentStyle'];
        if (this.selectedComment === data.markerName) {
          classes.push('commentStyle--active');
        }
        return { classes };
      },
    });

    conversion.for('dataDowncast').markerToData({
      model: markerTypes.comment,
    });

    conversion.for('upcast').dataToMarker({
      view: markerTypes.comment,
    });

    // Comment placeholder markers
    conversion.for('editingDowncast').markerToHighlight({
      model: markerTypes.commentPlaceholder,
      view: () => {
        return { classes: ['commentPlaceholderStyle'] };
      },
    });

    conversion.for('dataDowncast').markerToData({
      model: markerTypes.commentPlaceholder,
    });

    conversion.for('upcast').dataToMarker({
      view: markerTypes.commentPlaceholder,
    });

    // Resolved comment markers
    conversion.for('editingDowncast').markerToHighlight({
      model: markerTypes.resolvedComment,
      view: () => {
        return { classes: ['resolvedCommentStyle'] };
      },
    });

    conversion.for('dataDowncast').markerToData({
      model: markerTypes.resolvedComment,
    });

    conversion.for('upcast').dataToMarker({
      view: markerTypes.resolvedComment,
    });
  }

  defineSchema() {
    const { schema } = this.editor.model;

    schema.register(markerTypes.comment, {
      allowWhere: '$text',
      allowContentOf: '$marker',
      allowAttributes: ['id'],
      isInline: true,
      isSelectable: true,
    });

    schema.register(markerTypes.resolvedComment, {
      allowWhere: '$text',
      allowContentOf: '$marker',
      allowAttributes: ['id'],
      isInline: true,
      isSelectable: true,
    });
  }
}

function isInMarker(selection, marker) {
  const markerRange = marker.getRange();
  return markerRange.containsRange(selection.getFirstRange(), true);
}
