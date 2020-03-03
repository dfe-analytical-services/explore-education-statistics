import AccordionSection, {
  EditableAccordionSectionProps,
} from '@admin/components/EditableAccordionSection';
import ContentBlocks from '@admin/modules/editable-components/EditableContentBlocks';
import { ContentType } from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import {
  updateSectionBlockOrder,
  deleteContentSectionBlock,
  updateContentSectionBlock,
  attachContentSectionBlock,
  addContentSectionBlock,
} from '@admin/pages/release/edit-release/content/helpers';
import { useReleaseDispatch } from '@admin/pages/release/edit-release/content/ReleaseContext';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import {
  EditableContentBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@admin/types';
import Button from '@common/components/Button';
import React, { useContext } from 'react';
import AddDataBlockButton from './AddDataBlockButton';

export interface ReleaseContentAccordionSectionProps {
  id: string;
  contentItem: ContentType;
  index: number;
  onHeadingChange?: EditableAccordionSectionProps['onHeadingChange'];
  onContentChange: (content?: EditableContentBlock[]) => void;
  onRemoveSection?: EditableAccordionSectionProps['onRemoveSection'];
  canAddBlocks?: boolean;
  release: EditableRelease;
}

const ReleaseContentAccordionSection = ({
  release,
  id: sectionId,
  index,
  contentItem,
  onHeadingChange,
  onContentChange,
  onRemoveSection,
  canAddBlocks = true,
  ...restOfProps
}: ReleaseContentAccordionSectionProps) => {
  const dispatch = useReleaseDispatch();
  const { caption, heading } = contentItem;
  const [isReordering, setIsReordering] = React.useState(false);
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  const onBlockSaveOrder = async (order: Dictionary<number>) => {
    if (releaseId && sectionId) {
      const newBlocks = await releaseContentService.updateContentSectionBlocksOrder(
        releaseId,
        sectionId,
        order,
      );
      onContentChange(newBlocks);
    }
  };

  const onSectionAddTextBlock = async (type: 'MarkdownBlock' | 'HTMLBlock') => {
    if (releaseId && sectionId) {
      await releaseContentService.addContentSectionBlock(releaseId, sectionId, {
        body: '',
        type,
        order: contentItem.content ? contentItem.content.length : undefined,
      });
      const {
        content: newContentBlocks,
      } = await releaseContentService.getContentSection(releaseId, sectionId);

      onContentChange(newContentBlocks);
    }
  };

  return (
    <AccordionSection
      id={sectionId}
      index={index}
      heading={heading || ''}
      caption={caption}
      onHeadingChange={onHeadingChange}
      onRemoveSection={onRemoveSection}
      sectionId={sectionId}
      headerButtons={
        <a
          role="button"
          tabIndex={0}
          onClick={() => setIsReordering(!isReordering)}
          onKeyPress={e => {
            if (e.key === 'Enter') setIsReordering(!isReordering);
          }}
          className={`govuk-button ${!isReordering &&
            'govuk-button--secondary'} govuk-!-margin-right-2`}
        >
          {isReordering ? 'Save order' : 'Reorder'}
        </a>
      }
      {...restOfProps}
    >
      <ContentBlocks
        id={`${heading}-content`}
        isReordering={isReordering}
        sectionId={sectionId}
        onContentChange={onContentChange}
        onBlockSaveOrder={order => {
          updateSectionBlockOrder(
            dispatch,
            release.id,
            release.headlinesSection.id,
            'headlinesSection',
            order,
          );
        }}
        onBlockContentChange={(blockId, bodyContent) =>
          updateContentSectionBlock(
            dispatch,
            release.id,
            release.headlinesSection.id,
            blockId,
            'headlinesSection',
            bodyContent,
          )
        }
        onBlockDelete={(blockId: string) =>
          deleteContentSectionBlock(
            dispatch,
            release.id,
            release.headlinesSection.id,
            blockId,
            'headlinesSection',
          )
        }
        content={contentItem.content}
        allowComments
      />

      {!isReordering && (
        <div className="govuk-!-margin-bottom-8 dfe-align--center">
          <Button
            variant="secondary"
            onClick={() => {
              addContentSectionBlock(
                dispatch,
                releaseId,
                sectionId,
                'content',
                {
                  type: 'MarkdownBlock',
                  order: contentItem.content ? contentItem.content.length : 0,
                  body: '',
                },
              );
            }}
          >
            Add text block
          </Button>
          <AddDataBlockButton
            onAddDataBlock={(datablockId: string) => {
              attachContentSectionBlock(
                dispatch,
                releaseId,
                sectionId,
                'content',
                {
                  contentBlockId: datablockId,
                  order: contentItem.content ? contentItem.content.length : 0,
                },
              );
            }}
          />
        </div>
      )}
    </AccordionSection>
  );
};

export default ReleaseContentAccordionSection;
