import EditableAccordionSection from '@admin/components/editable/EditableAccordionSection';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import DataBlockSelectForm from '@admin/pages/release/content/components/DataBlockSelectForm';
import EditableEmbedForm, {
  EditableEmbedFormValues,
} from '@admin/components/editable/EditableEmbedForm';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableBlock } from '@admin/services/types/content';
import focusAddedSectionBlockButton from '@admin/utils/focus/focusAddedSectionBlockButton';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Modal from '@common/components/Modal';
import Tooltip from '@common/components/Tooltip';
import useToggle from '@common/hooks/useToggle';
import { ContentSection } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import { isFuture } from 'date-fns';
import React, {
  memo,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

export interface ReleaseContentAccordionSectionProps {
  id: string;
  section: ContentSection<EditableBlock>;
  onRemoveSection: (sectionId: string) => void;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}

const ReleaseContentAccordionSection = ({
  section,
  onRemoveSection,
  transformFeaturedTableLinks,
  ...props
}: ReleaseContentAccordionSectionProps) => {
  const { id: sectionId, caption, content: sectionContent = [] } = section;

  const { editingMode, unsavedCommentDeletions, unsavedBlocks } =
    useEditingContext();

  const { release } = useReleaseContentState();
  const actions = useReleaseContentActions();

  const { user } = useAuthContext();

  const [isReordering, toggleIsReordering] = useToggle(false);
  const [showDataBlockForm, toggleDataBlockForm] = useToggle(false);
  const [showEmbedDashboardForm, toggleEmbedDashboardForm] = useToggle(false);

  const [blocks, setBlocks] = useState<EditableBlock[]>(sectionContent);

  const addTextBlockButton = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    setBlocks(sectionContent);
  }, [sectionContent]);

  const heading = useMemo(() => {
    if (
      blocks.some(block => unsavedBlocks.includes(block.id)) ||
      blocks.some(block =>
        Object.keys(unsavedCommentDeletions).includes(block.id),
      )
    ) {
      return `${section.heading} (unsaved changes)`;
    }

    return section.heading;
  }, [blocks, section, unsavedBlocks, unsavedCommentDeletions]);

  const addBlock = useCallback(async () => {
    const newBlock = await actions.addContentSectionBlock({
      releaseVersionId: release.id,
      sectionId,
      sectionKey: 'content',
      block: {
        type: 'HtmlBlock',
        order: sectionContent.length,
        body: '',
      },
    });

    focusAddedSectionBlockButton(newBlock.id);
  }, [actions, release.id, sectionId, sectionContent.length]);

  const addEmbedBlock = useCallback(
    async (embedBlock: EditableEmbedFormValues) => {
      await actions.addEmbedSectionBlock({
        releaseVersionId: release.id,
        sectionId,
        sectionKey: 'content',
        request: {
          title: embedBlock.title,
          url: embedBlock.url,
          contentSectionId: sectionId,
        },
      });
    },
    [actions, release.id, sectionId],
  );

  const attachDataBlock = useCallback(
    async (contentBlockId: string) => {
      await actions.attachContentSectionBlock({
        releaseVersionId: release.id,
        sectionId,
        sectionKey: 'content',
        block: {
          contentBlockId,
          order: sectionContent.length,
        },
      });
    },
    [actions, release.id, sectionId, sectionContent.length],
  );

  const reorderBlocks = useCallback(async () => {
    const order = blocks.reduce<Dictionary<number>>((acc, block, newIndex) => {
      acc[block.id] = newIndex;
      return acc;
    }, {});

    await actions.updateSectionBlockOrder({
      releaseVersionId: release.id,
      sectionId,
      sectionKey: 'content',
      order,
    });
  }, [blocks, actions, release.id, sectionId]);

  const handleHeadingChange = useCallback(
    async (title: string) => {
      await actions.updateContentSectionHeading({
        sectionId,
        title,
        releaseVersionId: release.id,
      });
    },
    [actions, sectionId, release.id],
  );

  const onAfterDeleteBlock = () => {
    setTimeout(() => {
      addTextBlockButton.current?.focus();
    }, 100);
  };

  const hasLockedBlocks = blocks.some(
    block => block.lockedUntil && isFuture(new Date(block.lockedUntil)),
  );

  return (
    <EditableAccordionSection
      {...props}
      heading={heading}
      disabledRemoveSectionTooltip={
        hasLockedBlocks
          ? 'This section is being edited and cannot be removed'
          : undefined
      }
      caption={caption}
      onHeadingChange={handleHeadingChange}
      onRemoveSection={() => onRemoveSection(sectionId)}
      headerButtons={
        <Tooltip
          text="This section is being edited and cannot be reordered"
          enabled={hasLockedBlocks}
        >
          {({ ref }) => (
            <Button
              ariaDisabled={hasLockedBlocks}
              ref={ref}
              variant={!isReordering ? 'secondary' : undefined}
              onClick={async () => {
                if (isReordering) {
                  await reorderBlocks();
                  toggleIsReordering.off();
                } else {
                  toggleIsReordering.on();
                }
              }}
            >
              {isReordering ? 'Save section order' : 'Reorder this section'}
            </Button>
          )}
        </Tooltip>
      }
    >
      {({ open }) => (
        <>
          <EditableSectionBlocks
            blocks={blocks}
            isReordering={isReordering && editingMode === 'edit'}
            onBlocksChange={setBlocks}
            renderBlock={block => (
              <ReleaseBlock
                block={block}
                releaseVersionId={release.id}
                transformFeaturedTableLinks={transformFeaturedTableLinks}
                visible={open}
              />
            )}
            renderEditableBlock={block => (
              <ReleaseEditableBlock
                allowComments
                allowImages
                block={block}
                sectionId={sectionId}
                sectionKey="content"
                editable={!isReordering}
                publicationId={release.publication.id}
                releaseVersionId={release.id}
                visible={open}
                onAfterDeleteBlock={onAfterDeleteBlock}
              />
            )}
          />

          {editingMode === 'edit' && !isReordering && (
            <>
              {showDataBlockForm && (
                <DataBlockSelectForm
                  id={`dataBlockSelectForm-${sectionId}`}
                  releaseVersionId={release.id}
                  onSelect={async selectedDataBlockId => {
                    await attachDataBlock(selectedDataBlockId);
                    toggleDataBlockForm.off();
                  }}
                  onCancel={toggleDataBlockForm.off}
                />
              )}

              <ButtonGroup className="govuk-!-margin-bottom-8 dfe-justify-content--center">
                <Button
                  variant="secondary"
                  onClick={addBlock}
                  ref={addTextBlockButton}
                >
                  Add text block
                </Button>
                {!showDataBlockForm && (
                  <Button variant="secondary" onClick={toggleDataBlockForm.on}>
                    Add data block
                  </Button>
                )}
                {user?.permissions.isBauUser && (
                  <Modal
                    title="Embed a URL"
                    open={showEmbedDashboardForm}
                    onExit={toggleEmbedDashboardForm.off}
                    triggerButton={
                      <Button
                        variant="secondary"
                        onClick={toggleEmbedDashboardForm.on}
                      >
                        Embed a URL
                      </Button>
                    }
                  >
                    <EditableEmbedForm
                      onCancel={toggleEmbedDashboardForm.off}
                      onSubmit={async embedBlock => {
                        await addEmbedBlock(embedBlock);
                        toggleEmbedDashboardForm.off();
                      }}
                    />
                  </Modal>
                )}
              </ButtonGroup>
            </>
          )}
        </>
      )}
    </EditableAccordionSection>
  );
};

export default memo(ReleaseContentAccordionSection);
