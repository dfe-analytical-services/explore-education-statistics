import Plugin from '@ckeditor/ckeditor5-core/src/plugin';
import { scrollViewportToShowTarget } from '@ckeditor/ckeditor5-utils/src/dom/scroll';
import AddCommentCommand from './addCommentCommand';
import AddCommentPlaceholderCommand from './addCommentPlaceholderCommand';
import SelectCommentCommand from './selectCommentCommand';
import RemoveCommentCommand from './removeCommentCommand';
import ToggleResolveCommentCommand from './toggleResolveCommentCommand';

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
    this.editor.commands.add('addComment', new AddCommentCommand(this.editor));
    this.editor.commands.add(
      'addCommentPlaceholder',
      new AddCommentPlaceholderCommand(this.editor),
    );
    const selectCommentCommand = new SelectCommentCommand(this.editor);
    this.editor.commands.add('selectComment', selectCommentCommand);
    const removeCommentCommand = new RemoveCommentCommand(this.editor);
    this.editor.commands.add('removeComment', removeCommentCommand);
    this.editor.commands.add(
      'resolveComment',
      new ToggleResolveCommentCommand(this.editor),
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

        if (lastOperation.name && lastOperation.name.startsWith('comment:')) {
          if (
            lastOperation.batch.operations[0].name &&
            lastOperation.batch.operations[0].name.startsWith(
              'resolvedcomment:',
            )
          ) {
            this.config.undoRedoComment(
              'undoResolveComment',
              lastOperation.name,
            );
            return;
          }

          this.removedMarkers = this.removedMarkers.filter(
            marker => marker !== lastOperation.name,
          );
          this.config.undoRedoComment('undoRemoveComment', lastOperation.name);
          return;
        }

        if (
          lastOperation.name &&
          lastOperation.name === 'commentplaceholder' &&
          lastOperation.newRange &&
          !lastOperation.oldRange
        ) {
          this.config.undoRedoComment(
            'undoAddComment',
            lastOperation.batch.operations[0].name,
          );
          return;
        }

        if (
          lastOperation.name &&
          lastOperation.name.startsWith('resolvedcomment:')
        ) {
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

        if (lastOperation.name && lastOperation.name.startsWith('comment:')) {
          if (
            lastOperation.batch.operations[0].name &&
            lastOperation.batch.operations[0].name.startsWith(
              'resolvedcomment:',
            )
          ) {
            this.config.undoRedoComment(
              'redoUnresolveComment',
              lastOperation.name,
            );
            return;
          }

          if (
            lastOperation.batch.operations[0].name &&
            lastOperation.batch.operations[0].name === 'commentplaceholder'
          ) {
            this.config.undoRedoComment('redoAddComment', lastOperation.name);
            return;
          }

          this.config.undoRedoComment('redoRemoveComment', lastOperation.name);
          return;
        }

        if (
          lastOperation.name &&
          lastOperation.name.startsWith('resolvedcomment:')
        ) {
          this.config.undoRedoComment('redoResolveComment', lastOperation.name);
        }
      });
    });

    /**
     * Remove the marker when the remove comment command is executed.
     */
    this.listenTo(removeCommentCommand, 'execute', () => {
      if (editor.model.markers.has(removeCommentCommand.commentName)) {
        editor.model.change(writer =>
          writer.removeMarker(removeCommentCommand.commentName),
        );
      }
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
          marker.name.startsWith('comment:') &&
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
              !changedMarker.name.startsWith('comment:') ||
              this.removedMarkers.includes(changedMarker.name)
            ) {
              return;
            }

            // Marker has been deleted
            if (
              changedMarker.data.newRange.start.root.rootName ===
                '$graveyard' &&
              changedMarker.data.newRange.end.root.rootName === '$graveyard'
            ) {
              this.removedMarkers.push(changedMarker.name);
              this.config.commentRemoved(changedMarker.name);
              return;
            }

            // All the content of the marker has been deleted
            const marker = editor.model.markers.get(changedMarker.name);
            if (marker.getRange().isCollapsed) {
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
      const placeholderMarkerName = 'commentplaceholder';
      let selectedMarkerName = '';

      [...model.markers].forEach(marker => {
        if (
          (marker.name.startsWith('comment:') ||
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
      model: 'comment',
      view: data => {
        const classes = ['commentStyle'];
        if (this.selectedComment === data.markerName) {
          classes.push('commentStyle--active');
        }
        return { classes };
      },
    });

    conversion.for('dataDowncast').markerToData({
      model: 'comment',
    });

    conversion.for('upcast').dataToMarker({
      view: 'comment',
    });

    // Comment placeholder markers
    conversion.for('editingDowncast').markerToHighlight({
      model: 'commentplaceholder',
      view: () => {
        return { classes: ['commentPlaceholderStyle'] };
      },
    });

    conversion.for('dataDowncast').markerToData({
      model: 'commentplaceholder',
    });

    conversion.for('upcast').dataToMarker({
      view: 'commentplaceholder',
    });

    // Resolved comment markers
    conversion.for('editingDowncast').markerToHighlight({
      model: 'resolvedcomment',
      view: () => {
        return { classes: ['resolvedCommentStyle'] };
      },
    });

    conversion.for('dataDowncast').markerToData({
      model: 'resolvedcomment',
    });

    conversion.for('upcast').dataToMarker({
      view: 'resolvedcomment',
    });
  }

  defineSchema() {
    const { schema } = this.editor.model;

    schema.register('comment', {
      allowWhere: '$text',
      allowContentOf: '$marker',
      allowAttributes: ['id'],
      isInline: true,
      isSelectable: true,
    });

    schema.register('resolvedcomment', {
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
