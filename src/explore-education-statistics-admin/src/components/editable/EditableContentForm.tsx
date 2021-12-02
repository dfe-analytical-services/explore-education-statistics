import { useCommentsContext } from '@admin/contexts/comments/CommentsContext';
import useEditingActions from '@admin/contexts/editing/useEditingActions';
import useCommentActions from '@admin/contexts/comments/useCommentsActions';
import styles from '@admin/components/editable/EditableContentForm.module.scss';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { CommentUndoRedoActions, Element } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import CommentAddForm from '@admin/components/comments/CommentAddForm';
import { ResolveCommentEvent } from '@admin/components/comments/Comment';
import CommentsList from '@admin/components/comments/CommentsList';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { Formik } from 'formik';
import React, { useCallback, useRef, useState } from 'react';
import classNames from 'classnames';

interface FormValues {
  content: string;
}

export interface SelectedComment {
  commentId: string;
  fromEditor: boolean;
}

export interface Props {
  allowComments?: boolean;
  autoSave?: boolean;
  content: string;
  hideLabel?: boolean;
  handleBlur?: (isDirty: boolean) => void;
  id: string;
  isSaving?: boolean;
  label: string;
  onCancel: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSubmit: (content: string, isAutoSave?: boolean) => void;
}

const EditableContentForm = ({
  allowComments = false,
  autoSave = false,
  content,
  hideLabel = false,
  handleBlur,
  id,
  isSaving = false,
  label,
  onCancel,
  onImageUpload,
  onImageUploadCancel,
  onSubmit,
}: Props) => {
  const {
    comments,
    pendingDeletions,
    onDeletePendingComments,
    onToggleResolveComment,
  } = useCommentsContext();
  const commentActions = useCommentActions();
  const editingActions = useEditingActions();
  const containerRef = useRef<HTMLDivElement>(null);
  const [showCommentAddForm, toggleCommentAddForm] = useToggle(false);
  const [selectedComment, setSelectedComment] = useState<SelectedComment>({
    commentId: '',
    fromEditor: false,
  });
  const [markersOrder, setMarkersOrder] = useState<string[]>([]);

  const validateElements = useCallback((elements: Element[]) => {
    let error: string | undefined;

    elements.some(element => {
      if (element.name === 'image' && !element.getAttribute('alt')) {
        error = 'All images must have alternative (alt) text';
        return true;
      }

      return false;
    });

    return error;
  }, []);

  const handleUndoRedoComment = (
    type: CommentUndoRedoActions,
    commentId: string,
  ) => {
    if (type === 'undoRemoveComment' || type === 'redoAddComment') {
      commentActions.undeleteComment(commentId);
      editingActions.updateUnsavedCommentDeletions(
        id.replace('block-', ''),
        commentId,
      );
      return;
    }
    if (type === 'undoAddComment' || type === 'redoRemoveComment') {
      commentActions.deleteComment(commentId);
      editingActions.updateUnsavedCommentDeletions(
        id.replace('block-', ''),
        commentId,
      );
      return;
    }

    if (type === 'undoResolveComment' || type === 'redoUnresolveComment') {
      const commentToUnresolve = comments.find(
        comment => comment.id === commentId,
      );
      if (commentToUnresolve) {
        handleToggleResolveComment({
          comment: commentToUnresolve,
          resolve: false,
          updateMarker: false,
        });
      }
      return;
    }
    if (type === 'undoUnresolveComment' || type === 'redoResolveComment') {
      const commentToResolve = comments.find(
        comment => comment.id === commentId,
      );
      if (commentToResolve) {
        handleToggleResolveComment({
          comment: commentToResolve,
          resolve: true,
          updateMarker: false,
        });
      }
    }
  };

  const handleToggleResolveComment = async ({
    comment,
    resolve,
    updateMarker = true,
  }: ResolveCommentEvent) => {
    const resolvedComment = {
      ...comment,
      setResolved: resolve !== undefined ? resolve : !comment.resolved,
    };
    const updatedComment = await onToggleResolveComment?.(resolvedComment);
    if (updateMarker) {
      commentActions.setCurrentInteraction({
        type: comment.resolved ? 'unresolving' : 'resolving',
        id: comment.id,
      });
    }
    if (updatedComment) {
      commentActions.updateComment(updatedComment);
    }
    editingActions.updateUnresolvedComments(
      id.replace('block-', ''),
      resolvedComment.id,
    );
  };

  return (
    <div className={styles.container} ref={containerRef}>
      {showCommentAddForm && (
        <CommentAddForm
          blockId={id}
          containerRef={containerRef}
          onCancel={() => {
            commentActions.setCurrentInteraction({
              type: 'removing',
              id: 'commentplaceholder',
            });
            toggleCommentAddForm.off();
          }}
          onSave={comment => {
            commentActions.addComment(comment);
            editingActions.updateUnresolvedComments(
              id.replace('block-', ''),
              comment.id,
            );
            toggleCommentAddForm.off();
          }}
        />
      )}
      <div
        className={classNames(styles.commentsSidebar, {
          [styles.showCommentAddForm]: showCommentAddForm,
        })}
      >
        {allowComments && comments.length > 0 && (
          <CommentsList
            comments={comments}
            markersOrder={markersOrder}
            selectedComment={selectedComment}
            onRemove={commentId => {
              commentActions.deleteComment(commentId);
              commentActions.setCurrentInteraction({
                type: 'removing',
                id: commentId,
              });
              editingActions.updateUnsavedCommentDeletions(
                id.replace('block-', ''),
                commentId,
              );
            }}
            onResolve={handleToggleResolveComment}
            onSelect={commentId =>
              setSelectedComment({ commentId, fromEditor: false })
            }
            onUpdate={commentActions.updateComment}
          />
        )}
      </div>

      <div className={styles.form}>
        <Formik<FormValues>
          initialValues={{
            content,
          }}
          validateOnChange={false}
          validationSchema={Yup.object({
            content: Yup.string().required('Enter content'),
          })}
          onSubmit={async values => {
            if (pendingDeletions.length) {
              await onDeletePendingComments?.(pendingDeletions);
              commentActions.resetPendingDeletion();
            }
            onSubmit(values.content);
          }}
        >
          <Form id={`${id}-form`}>
            <FormFieldEditor<FormValues>
              id={id}
              allowComments={allowComments}
              focusOnInit
              handleBlur={handleBlur}
              hideLabel={hideLabel}
              label={label}
              name="content"
              selectedComment={selectedComment}
              validateElements={validateElements}
              onAutoSave={values => {
                commentActions.setCurrentInteraction(undefined);
                onSubmit(values, true);
              }}
              onCancelComment={toggleCommentAddForm.off}
              onClickAddComment={toggleCommentAddForm.on}
              onClickCommentMarker={commentId =>
                setSelectedComment({ commentId, fromEditor: true })
              }
              onImageUpload={onImageUpload}
              onImageUploadCancel={onImageUploadCancel}
              onRemoveCommentMarker={commentId => {
                commentActions.deleteComment(commentId);
                editingActions.updateUnsavedCommentDeletions(
                  id.replace('block-', ''),
                  commentId,
                );
              }}
              onUndoRedo={(type, commentId) => {
                handleUndoRedoComment(type, commentId);
              }}
              onUpdateMarkersOrder={setMarkersOrder}
            />

            <ButtonGroup>
              <Button type="submit" disabled={isSaving}>
                {autoSave ? 'Save & close' : 'Save'}
              </Button>
              <LoadingSpinner
                inline
                hideText
                loading={autoSave && isSaving}
                size="md"
                text="Saving"
              />
              {!autoSave && (
                <Button variant="secondary" onClick={onCancel}>
                  Cancel
                </Button>
              )}
              {!isSaving && pendingDeletions.length > 0 && (
                <em>(Unsaved deletions)</em>
              )}
            </ButtonGroup>
          </Form>
        </Formik>
      </div>
    </div>
  );
};

export default EditableContentForm;
