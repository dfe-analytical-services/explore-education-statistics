import EditableBlockLockedMessage from '@admin/components/editable/EditableBlockLockedMessage';
import { useCommentsContext } from '@admin/contexts/CommentsContext';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import styles from '@admin/components/editable/EditableContentBlock.module.scss';
import glossaryService from '@admin/services/glossaryService';
import { UserDetails } from '@admin/services/types/user';
import { ToolbarGroup, ToolbarOption } from '@admin/types/ckeditor';
import {
  ImageUploadCancelHandler,
  ImageUploadHandler,
} from '@admin/utils/ckeditor/CustomUploadAdapter';
import ContentHtml from '@common/components/ContentHtml';
import Tooltip from '@common/components/Tooltip';
import { Dictionary } from '@common/types';
import sanitizeHtml, {
  commentTagAttributes,
  commentTags,
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
  TagFilter,
} from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import mapValues from 'lodash/mapValues';
import React, { ReactNode, useMemo, useRef } from 'react';

interface EditableContentBlockProps {
  actionThrottle?: number;
  allowComments?: boolean;
  editable?: boolean;
  editButtonLabel?: ReactNode | string;
  id: string;
  idleTimeout?: number;
  isEditing?: boolean;
  isLoading?: boolean;
  label: string;
  locked?: string;
  lockedBy?: UserDetails;
  hideLabel?: boolean;
  removeButtonLabel?: ReactNode | string;
  toolbarConfig?:
    | ReadonlyArray<ToolbarOption | ToolbarGroup>
    | Array<ToolbarOption | ToolbarGroup>;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  value: string;
  onActive?: () => void;
  onAutoSave?: (value: string) => void;
  onBlur?: (isDirty: boolean) => void;
  onCancel?: () => void;
  onDelete: () => void;
  onEditing: () => void;
  onIdle?: () => void;
  onImageUpload?: ImageUploadHandler;
  onImageUploadCancel?: ImageUploadCancelHandler;
  onSubmit: (value: string) => void;
}

const EditableContentBlock = ({
  actionThrottle,
  allowComments = false,
  editable = true,
  editButtonLabel,
  id,
  idleTimeout,
  isLoading,
  isEditing,
  label,
  locked,
  lockedBy,
  hideLabel = false,
  removeButtonLabel,
  toolbarConfig,
  transformImageAttributes,
  value,
  onActive,
  onAutoSave,
  onBlur,
  onCancel,
  onDelete,
  onEditing,
  onIdle,
  onImageUpload,
  onImageUploadCancel,
  onSubmit,
}: EditableContentBlockProps) => {
  const { comments } = useCommentsContext();
  const contentRef = useRef<HTMLDivElement>(null);

  const sanitizeOptions: SanitizeHtmlOptions = useMemo(() => {
    const commentTagFilter: TagFilter = frame =>
      comments.every(comment => comment.id !== frame.attribs.name);

    return {
      ...defaultSanitizeOptions,
      allowedTags: [
        ...(defaultSanitizeOptions.allowedTags ?? []),
        ...commentTags,
      ],
      allowedAttributes: {
        ...(defaultSanitizeOptions.allowedAttributes ?? {}),
        ...commentTagAttributes,
      },
      filterTags: mapValues(commentTagAttributes, () => commentTagFilter),
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
  }, [comments, transformImageAttributes]);

  if (isEditing && !lockedBy) {
    return (
      <EditableContentForm
        actionThrottle={actionThrottle}
        allowComments={allowComments}
        content={value ? sanitizeHtml(value, sanitizeOptions) : ''}
        label={label}
        hideLabel={hideLabel}
        id={id}
        idleTimeout={idleTimeout}
        toolbarConfig={toolbarConfig}
        onAction={onActive}
        onAutoSave={onAutoSave}
        onBlur={onBlur}
        onIdle={onIdle}
        onImageUpload={onImageUpload}
        onImageUploadCancel={onImageUploadCancel}
        onCancel={onCancel}
        onSubmit={onSubmit}
      />
    );
  }

  const isEditable = editable && !isLoading && !lockedBy;

  const blockLockedMessage = lockedBy
    ? `This block is being edited by ${lockedBy?.displayName}`
    : undefined;

  return (
    <CommentsWrapper
      allowComments={allowComments}
      commentType="inline"
      blockLockedMessage={blockLockedMessage}
      id={id}
      showCommentsList={false}
      onViewComments={onEditing}
    >
      {locked && lockedBy && (
        <EditableBlockLockedMessage locked={locked} lockedBy={lockedBy} />
      )}

      <EditableBlockWrapper
        editButtonLabel={editButtonLabel}
        isLoading={isLoading}
        lockedBy={lockedBy}
        removeButtonLabel={removeButtonLabel}
        onEdit={editable ? onEditing : undefined}
        onDelete={editable ? onDelete : undefined}
      >
        <Tooltip enabled={!!lockedBy} followMouse text={blockLockedMessage}>
          {({ ref }) => (
            // eslint-disable-next-line jsx-a11y/click-events-have-key-events,jsx-a11y/no-static-element-interactions
            <div
              className={classNames(styles.preview, {
                [styles.readOnly]: !isEditing,
                [styles.editable]: isEditable,
                [styles.locked]: !!lockedBy,
              })}
              ref={ref}
              onClick={isEditable ? onEditing : undefined}
            >
              <div inert="" ref={contentRef}>
                <ContentHtml
                  getGlossaryEntry={glossaryService.getEntry}
                  html={value || '<p>This section is empty</p>'}
                  sanitizeOptions={sanitizeOptions}
                />
              </div>
              {lockedBy && (
                <span className={styles.lockedMessage}>
                  {`${lockedBy.displayName} is editing`}
                </span>
              )}
            </div>
          )}
        </Tooltip>
      </EditableBlockWrapper>
    </CommentsWrapper>
  );
};

export default EditableContentBlock;
