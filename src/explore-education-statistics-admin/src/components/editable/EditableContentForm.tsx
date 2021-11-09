import styles from '@admin/components/editable/EditableContentForm.module.scss';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { CommentUndoRedoActions, Element } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import AddCommentForm from '@admin/components/comments/AddCommentForm';
import { toggleResolveCommentHandler } from '@admin/components/comments/Comment';
import CommentsList from '@admin/components/comments/CommentsList';
import { Comment } from '@admin/services/types/content';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form } from '@common/components/form';
import { BlockCommentsState } from '@admin/components/form/FormEditor';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import releaseContentCommentService from '@admin/services/releaseContentCommentService';
import { CommentsPendingDeletion } from '@admin/pages/release/content/contexts/ReleaseContentContext';
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
  comments?: Comment[];
  commentsPendingDeletion?: CommentsPendingDeletion;
  content: string;
  hideLabel?: boolean;
  handleBlur?: (isDirty: boolean) => void;
  id: string;
  isSaving?: boolean;
  label: string;
  releaseId?: string;
  sectionId?: string;
  onBlockCommentsChange?: (blockId: string, comments: Comment[]) => void;
  onCancel: () => void;
  onCommentsPendingDeletionChange?: (
    blockId: string,
    commentId?: string,
  ) => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSubmit: (content: string, closeEditor?: boolean) => void;
}

const EditableContentForm = ({
  allowComments = false,
  autoSave = false,
  commentsPendingDeletion,
  content,
  hideLabel = false,
  handleBlur,
  id,
  isSaving = false,
  label,
  comments,
  releaseId,
  sectionId,
  onBlockCommentsChange,
  onCancel,
  onCommentsPendingDeletionChange,
  onImageUpload,
  onImageUploadCancel,
  onSubmit,
}: Props) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [showAddCommentForm, toggleAddCommentForm] = useToggle(false);
  const [selectedComment, setSelectedComment] = useState<SelectedComment>({
    commentId: '',
    fromEditor: false,
  });
  const [markersOrder, setMarkersOrder] = useState<string[]>([]);
  const [blockCommentsState, setBlockCommentsState] = useState<
    BlockCommentsState
  >({});

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
    if (
      type === 'undoRemoveComment' ||
      type === 'redoAddComment' ||
      type === 'undoAddComment' ||
      type === 'redoRemoveComment'
    ) {
      if (onCommentsPendingDeletionChange) {
        onCommentsPendingDeletionChange(id, commentId);
      }
      if (onBlockCommentsChange && comments) {
        onBlockCommentsChange(id.replace('block-', ''), comments);
      }

      return;
    }

    if (type === 'undoResolveComment' || type === 'redoUnresolveComment') {
      const commentToUnresolve = comments?.find(
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
      const commentToResolve = comments?.find(
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

  const handleUpdateComment = (updatedComment: Comment) => {
    if (!comments) {
      return;
    }
    const index = comments.findIndex(
      comment => comment.id === updatedComment.id,
    );
    const updatedComments = [...comments];
    updatedComments[index] = updatedComment;

    if (onBlockCommentsChange) {
      onBlockCommentsChange(id.replace('block-', ''), updatedComments);
    }
  };

  const handleToggleResolveComment = async ({
    comment,
    resolve,
    updateMarker = true,
  }: toggleResolveCommentHandler) => {
    const resolvedComment = {
      ...comment,
      setResolved: resolve !== undefined ? resolve : !comment.resolved,
    };
    const updatedComment = await releaseContentCommentService.updateContentSectionComment(
      resolvedComment,
    );
    if (updateMarker) {
      if (comment.resolved) {
        setBlockCommentsState({
          unresolving: comment.id,
        });
      }
      if (!comment.resolved) {
        setBlockCommentsState({
          resolving: comment.id,
        });
      }
    }
    handleUpdateComment(updatedComment);
  };

  return (
    <div className={styles.container} ref={containerRef}>
      {showAddCommentForm && (
        <AddCommentForm
          blockId={id}
          containerRef={containerRef}
          releaseId={releaseId}
          sectionId={sectionId}
          onCancel={() => {
            setBlockCommentsState({
              removing: 'commentplaceholder',
            });
            toggleAddCommentForm.off();
          }}
          onSave={comment => {
            setBlockCommentsState({
              adding: comment.id,
            });

            const updatedComments = comments
              ? [...comments, comment]
              : [comment];

            if (onBlockCommentsChange) {
              onBlockCommentsChange(id.replace('block-', ''), updatedComments);
            }

            toggleAddCommentForm.off();
          }}
        />
      )}
      <div
        className={classNames(styles.commentsSidebar, { showAddCommentForm })}
      >
        {allowComments && comments && comments.length > 0 && (
          <CommentsList
            comments={comments}
            commentsPendingDeletion={
              commentsPendingDeletion && commentsPendingDeletion[id]
                ? commentsPendingDeletion[id]
                : []
            }
            markersOrder={markersOrder}
            selectedComment={selectedComment}
            onCommentRemoved={commentId => {
              if (onCommentsPendingDeletionChange) {
                onCommentsPendingDeletionChange(id, commentId);
              }
              if (onBlockCommentsChange) {
                onBlockCommentsChange(id.replace('block-', ''), comments);
              }
              setBlockCommentsState({
                removing: commentId,
              });
            }}
            onCommentResolved={handleToggleResolveComment}
            onCommentSelect={commentId =>
              setSelectedComment({ commentId, fromEditor: false })
            }
            onCommentUpdated={handleUpdateComment}
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
            if (
              commentsPendingDeletion &&
              commentsPendingDeletion[id] &&
              comments?.length
            ) {
              const promises: Promise<void>[] = [];
              commentsPendingDeletion[id].forEach(commentId => {
                if (comments.find(comment => comment.id === commentId)) {
                  promises.push(
                    releaseContentCommentService.deleteContentSectionComment(
                      commentId,
                    ),
                  );
                }
              });
              await Promise.all(promises);
              if (onCommentsPendingDeletionChange) {
                onCommentsPendingDeletionChange(id);
              }
            }
            onSubmit(values.content);
          }}
        >
          <Form id={`${id}-form`}>
            <FormFieldEditor<FormValues>
              id={id}
              allowComments={allowComments}
              blockCommentsState={blockCommentsState}
              focusOnInit
              handleBlur={handleBlur}
              hideLabel={hideLabel}
              label={label}
              name="content"
              selectedComment={selectedComment}
              validateElements={validateElements}
              onAddCommentClicked={toggleAddCommentForm.on}
              onAutoSave={values => {
                setBlockCommentsState({});
                onSubmit(values, false);
              }}
              onCancelComment={toggleAddCommentForm.off}
              onCommentMarkerClicked={commentId =>
                setSelectedComment({ commentId, fromEditor: true })
              }
              onCommentMarkerRemoved={commentId => {
                if (onCommentsPendingDeletionChange) {
                  onCommentsPendingDeletionChange(id, commentId);
                }
                if (onBlockCommentsChange && comments) {
                  onBlockCommentsChange(id.replace('block-', ''), comments);
                }
              }}
              onImageUpload={onImageUpload}
              onImageUploadCancel={onImageUploadCancel}
              onUndoRedo={handleUndoRedoComment}
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
            </ButtonGroup>
          </Form>
        </Formik>
      </div>
    </div>
  );
};

export default EditableContentForm;
