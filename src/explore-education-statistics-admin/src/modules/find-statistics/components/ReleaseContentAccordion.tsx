import Accordion from '@admin/components/EditableAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { AbstractRelease } from '@common/services/publicationService';
import { Dictionary } from '@common/types/util';
import React, { useCallback, useEffect, useState } from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

export type ContentType = AbstractRelease<EditableContentBlock>['content'][0];

interface ReleaseContentAccordionProps {
  releaseId: string;
  accordionId: string;
  sectionName: string;
  onContentChange?: (content: ContentType[]) => void;
}

const ReleaseContentAccordion = ({
  releaseId,
  accordionId,
  sectionName,
  onContentChange,
  handleApiErrors,
}: ReleaseContentAccordionProps & ErrorControlProps) => {
  const [content, setContent] = useState<ContentType[]>([]);

  const setContentAndTriggerOnContentChange = useCallback(
    (newContent: ContentType[]) => {
      setContent(newContent);

      if (onContentChange) {
        onContentChange(newContent);
      }
    },
    [onContentChange, setContent],
  );

  const onReorder = useCallback(
    async (ids: Dictionary<number>) => {
      const newContent = await releaseContentService
        .updateContentSectionsOrder(releaseId, ids)
        .catch(handleApiErrors);

      setContentAndTriggerOnContentChange(newContent);
    },
    [releaseId, handleApiErrors, setContentAndTriggerOnContentChange],
  );

  useEffect(
    () => {
      releaseContentService
        .getContentSections(releaseId)
        .then(setContentAndTriggerOnContentChange)
        .catch(handleApiErrors);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  const onAddSection = useCallback(async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService
        .addContentSection(releaseId, content.length)
        .catch(handleApiErrors),
    ];

    setContentAndTriggerOnContentChange(newContent);
  }, [
    content,
    releaseId,
    handleApiErrors,
    setContentAndTriggerOnContentChange,
  ]);

  const onUpdateHeading = useCallback(
    async (block: ContentType, index: number, newTitle: string) => {
      let result;
      if (block.id) {
        result = await releaseContentService
          .updateContentSectionHeading(releaseId, block.id, newTitle)
          .catch(handleApiErrors);

        const newContent = [...content];
        newContent[index].heading = newTitle;

        setContentAndTriggerOnContentChange(newContent);
      }
      return result;
    },
    [content, releaseId, handleApiErrors, setContentAndTriggerOnContentChange],
  );

  const updateContentSection = useCallback(
    (index: number, contentBlock?: EditableContentBlock[]) => {
      const newContent = [...content];
      newContent[index].content = contentBlock;

      setContentAndTriggerOnContentChange(newContent);
    },
    [content, setContentAndTriggerOnContentChange],
  );

  const onRemoveContentSection = useCallback(
    async (block: ContentType, index: number) => {
      if (block.id) {
        await releaseContentService
          .removeContentSection(releaseId, block.id)
          .catch(handleApiErrors);

        const newContent = content.filter(item => item.id !== block.id);

        setContentAndTriggerOnContentChange(newContent);
      }
    },
    [content, releaseId, handleApiErrors, setContentAndTriggerOnContentChange],
  );

  return (
    <>
      <Accordion
        id={accordionId}
        canReorder
        sectionName={sectionName}
        onSaveOrder={onReorder}
        onAddSection={onAddSection}
      >
        {content.map((contentItem, index) => (
          <ReleaseContentAccordionSection
            id={contentItem.id}
            key={contentItem.order}
            contentItem={contentItem}
            index={index}
            onHeadingChange={title =>
              onUpdateHeading(contentItem, index, title)
            }
            onContentChange={newContent =>
              updateContentSection(index, newContent)
            }
            onRemoveSection={() => onRemoveContentSection(contentItem, index)}
          />
        ))}
      </Accordion>
    </>
  );
};

export default withErrorControl(ReleaseContentAccordion);
