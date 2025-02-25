import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import EditableEmbedBlock from '@admin/components/editable/EditableEmbedBlock';
import CommentsWrapper from '@admin/components/comments/CommentsWrapper';
import { releaseToolbarConfigFull } from '@admin/config/ckEditorConfig';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import useReleaseImageUpload from '@admin/pages/release/hooks/useReleaseImageUpload';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import { EditableBlock } from '@admin/services/types/content';
import Gate from '@common/components/Gate';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import useReleaseImageAttributeTransformer from '@common/modules/release/hooks/useReleaseImageAttributeTransformer';
import isBrowser from '@common/utils/isBrowser';
import React from 'react';
import { generatePath } from 'react-router';
import useToggle from '@common/hooks/useToggle';
import noop from 'lodash/noop';

interface Props {
  allowComments?: boolean;
  allowImages?: boolean;
  block: EditableBlock;
  editable?: boolean;
  publicationId: string;
  releaseVersionId: string;
  visible?: boolean;
}

const PrototypeReleaseEditableBlock = ({
  allowComments = false,
  allowImages = false,
  block,
  editable = true,
  publicationId,
  releaseVersionId,
  visible,
}: Props) => {
  const getChartFile = useGetChartFile(releaseVersionId);

  const { handleImageUpload, handleImageUploadCancel } =
    useReleaseImageUpload(releaseVersionId);

  const transformImageAttributes = useReleaseImageAttributeTransformer({
    releaseVersionId,
  });

  const [showCommentAddForm, toggleCommentAddForm] = useToggle(false);

  const blockId = `block-${block.id}`;

  function renderBlock() {
    switch (block.type) {
      case 'DataBlock':
        return (
          <CommentsWrapper
            commentType="block"
            id={block.id}
            showCommentAddForm={showCommentAddForm}
            testId={`data-block-comments-${block.name}`}
            onAddCancel={toggleCommentAddForm.off}
            onAddSave={toggleCommentAddForm.off}
            onAdd={toggleCommentAddForm.on}
          >
            <EditableBlockWrapper
              dataBlockEditLink={generatePath<ReleaseDataBlockRouteParams>(
                releaseDataBlockEditRoute.path,
                {
                  publicationId,
                  releaseVersionId,
                  dataBlockId: block.id,
                },
              )}
              onDelete={noop}
            >
              <Gate condition={!!visible}>
                <DataBlockTabs
                  releaseVersionId={releaseVersionId}
                  id={blockId}
                  dataBlock={block}
                  getInfographic={getChartFile}
                />
              </Gate>
            </EditableBlockWrapper>
          </CommentsWrapper>
        );
      case 'EmbedBlockLink': {
        return (
          <CommentsWrapper
            commentType="block"
            id={block.id}
            showCommentAddForm={showCommentAddForm}
            testId="embed-block-comments"
            onAddCancel={toggleCommentAddForm.off}
            onAddSave={toggleCommentAddForm.off}
            onAdd={toggleCommentAddForm.on}
          >
            <EditableEmbedBlock
              editable={editable}
              block={block}
              visible={visible}
              onDelete={noop}
              onSubmit={noop}
            />
          </CommentsWrapper>
        );
      }
      case 'HtmlBlock':
      case 'MarkDownBlock': {
        return (
          <EditableContentBlock
            allowComments={allowComments}
            editable={editable && !isBrowser('IE')}
            hideLabel
            id={blockId}
            label="Content block"
            toolbarConfig={releaseToolbarConfigFull}
            transformImageAttributes={transformImageAttributes}
            useMarkdown={block.type === 'MarkDownBlock'}
            value={block.body}
            onActive={noop}
            onAutoSave={noop}
            onBlur={noop}
            onSubmit={noop}
            onDelete={noop}
            onEditing={noop}
            onImageUpload={allowImages ? handleImageUpload : undefined}
            onImageUploadCancel={
              allowImages ? handleImageUploadCancel : undefined
            }
          />
        );
      }
      default:
        return <div>Unable to edit content</div>;
    }
  }

  return (
    <CommentsContextProvider
      comments={block.comments}
      onDelete={() => Promise.resolve()}
      onPendingDelete={noop}
      onPendingDeleteUndo={noop}
      onCreate={() =>
        Promise.resolve({
          id: '1',
          content: '',
          created: '',
          createdBy: { id: 'a', lastName: '', firstName: '', email: '' },
        })
      }
      onUpdate={() => Promise.resolve()}
    >
      {renderBlock()}
    </CommentsContextProvider>
  );
};

export default PrototypeReleaseEditableBlock;
