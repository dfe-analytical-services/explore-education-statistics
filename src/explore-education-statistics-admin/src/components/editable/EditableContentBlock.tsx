import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import { CommentsPendingDeletion } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { Comment } from '@admin/services/types/content';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import toHtml from '@admin/utils/markdown/toHtml';
import Button from '@common/components/Button';
import ContentHtml from '@common/components/ContentHtml';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import sanitizeHtml, {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import React, { useCallback, useMemo } from 'react';
import styles from './EditableContentBlock.module.scss';

interface EditableContentBlockProps {
  allowComments?: boolean;
  autoSave?: boolean;
  commentsPendingDeletion?: CommentsPendingDeletion;
  editable?: boolean;
  id: string;
  isSaving?: boolean;
  label: string;
  hideLabel?: boolean;
  value: string;
  handleBlur?: (isDirty: boolean) => void;
  onBlockCommentsChange?: (blockId: string, comments: Comment[]) => void;
  onCommentsPendingDeletionChange?: (
    blockId: string,
    commentId?: string,
  ) => void;
  onCancel?: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSave: (value: string) => void;
  onDelete: () => void;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  useMarkdown?: boolean;
  releaseId?: string;
  sectionId?: string;
  comments?: Comment[];
}

const EditableContentBlock = ({
  allowComments = false,
  autoSave = false,
  commentsPendingDeletion,
  editable = true,
  releaseId,
  sectionId,
  comments,
  id,
  isSaving,
  label,
  hideLabel = false,
  value,
  handleBlur,
  onBlockCommentsChange,
  onCommentsPendingDeletionChange,
  onCancel,
  onImageUpload,
  onImageUploadCancel,
  onSave,
  onDelete,
  transformImageAttributes,
  useMarkdown,
}: EditableContentBlockProps) => {
  const content = useMemo(() => (useMarkdown ? toHtml(value) : value), [
    useMarkdown,
    value,
  ]);

  const [isEditing, toggleEditing] = useToggle(false);

  const sanitizeOptions: SanitizeHtmlOptions = useMemo(() => {
    const commentTags = [
      'comment-start',
      'comment-end',
      'resolvedcomment-start',
      'resolvedcomment-end',
    ];
    const commentAttributes = {
      'comment-start': ['name'],
      'comment-end': ['name'],
      'resolvedcomment-start': ['name'],
      'resolvedcomment-end': ['name'],
    };

    return {
      ...defaultSanitizeOptions,
      allowedTags: defaultSanitizeOptions.allowedTags
        ? [...defaultSanitizeOptions.allowedTags, ...commentTags]
        : [],
      allowedAttributes: defaultSanitizeOptions.allowedAttributes
        ? { ...defaultSanitizeOptions.allowedAttributes, ...commentAttributes }
        : {},
      transformTags: {
        img: (tagName, attribs) => {
          return {
            tagName,
            attribs: transformImageAttributes
              ? transformImageAttributes(attribs)
              : attribs,
          };
        },
      },
    };
  }, [transformImageAttributes]);

  const handleSave = useCallback(
    (nextValue: string, closeEditor = true) => {
      if (closeEditor) {
        toggleEditing.off();
      }
      // No need to handle useMarkdown case
      // as Admin API now converts MarkDownBlocks
      // to HtmlBlocks

      onSave(nextValue);
    },
    [toggleEditing, onSave],
  );

  return (
    <>
      {isEditing ? (
        <EditableContentForm
          allowComments={allowComments}
          autoSave={autoSave}
          commentsPendingDeletion={commentsPendingDeletion}
          id={id}
          isSaving={isSaving}
          releaseId={releaseId}
          comments={comments}
          sectionId={sectionId}
          label={label}
          hideLabel={hideLabel}
          content={content ? sanitizeHtml(content, sanitizeOptions) : ''} // NOTE: Sanitize to transform img src attribs
          onBlockCommentsChange={onBlockCommentsChange}
          onCommentsPendingDeletionChange={onCommentsPendingDeletionChange}
          onImageUpload={onImageUpload}
          onImageUploadCancel={onImageUploadCancel}
          onCancel={() => {
            toggleEditing.off();
            if (onCancel) {
              onCancel();
            }
          }}
          onSubmit={handleSave}
          handleBlur={handleBlur}
        />
      ) : (
        <>
          {allowComments && comments && comments.length > 0 && (
            <div className={styles.commentsButtonContainer}>
              <Button variant="secondary" onClick={toggleEditing.on}>
                View comments
                <br />
                <span className="govuk-!-margin-top-1 govuk-body-s">
                  <strong>
                    {comments?.filter(comment => !comment.resolved).length}
                  </strong>{' '}
                  unresolved
                </span>
              </Button>
            </div>
          )}
          <EditableBlockWrapper
            onEdit={editable ? toggleEditing.on : undefined}
            onDelete={editable ? onDelete : undefined}
          >
            <div
              className={classNames(styles.preview, {
                [styles.readOnly]: !isEditing,
              })}
            >
              {editable ? (
                <div
                  className={styles.editButton}
                  role="button"
                  tabIndex={0}
                  onClick={toggleEditing.on}
                  onKeyPress={e => {
                    switch (e.key) {
                      case 'Enter':
                      case ' ':
                        toggleEditing.on();
                        break;
                      default:
                        break;
                    }
                  }}
                >
                  <ContentHtml
                    html={content || '<p>This section is empty</p>'}
                    sanitizeOptions={sanitizeOptions}
                  />
                </div>
              ) : (
                <ContentHtml
                  html={content || '<p>This section is empty</p>'}
                  sanitizeOptions={sanitizeOptions}
                />
              )}
            </div>
          </EditableBlockWrapper>
        </>
      )}
    </>
  );
};

export default EditableContentBlock;
