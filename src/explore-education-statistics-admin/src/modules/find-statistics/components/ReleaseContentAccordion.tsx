import Accordion from '@admin/components/EditableAccordion';
import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ContentSectionViewModel } from '@admin/services/release/edit-release/content/types';
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
  publication: AbstractRelease<EditableContentBlock>['publication'];
  accordionId: string;
  sectionName: string;
  onContentChange?: (content: ContentType[]) => void;
}

const ReleaseContentAccordion = ({
  releaseId,
  publication,
  accordionId,
  sectionName,
  onContentChange,
  handleApiErrors,
}: ReleaseContentAccordionProps & ErrorControlProps) => {
  const [content, setContent] = useState<ContentType[]>([]);

  const setContentAndTriggerOnContentChange = useCallback(
    (newContent: AbstractRelease<EditableContentBlock>['content']) => {
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
    [releaseId, setContent, handleApiErrors],
  );

  useEffect(() => {
    releaseContentService
      .getContentSections(releaseId)
      .then(newContent => {
        setContent(newContent);

        if (onContentChange) {
          onContentChange(newContent);
        }
      })
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors, setContent]);

  const onAddSection = useCallback(async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService
        .addContentSection(releaseId, content.length)
        .catch(handleApiErrors),
    ];

    setContentAndTriggerOnContentChange(newContent);
  }, [content, releaseId, setContent, handleApiErrors]);

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
    [content, releaseId, setContent, handleApiErrors],
  );

  const updateContentSection = useCallback(
    (index: number, contentBlock?: EditableContentBlock[]) => {
      const newContent = [...content];
      newContent[index].content = contentBlock;

      setContentAndTriggerOnContentChange(newContent);
    },
    [content, setContent],
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
            publication={publication}
            onHeadingChange={title =>
              onUpdateHeading(contentItem, index, title)
            }
            onContentChange={newContent =>
              updateContentSection(index, newContent)
            }
          />
        ))}
      </Accordion>
    </>
  );
};

export default withErrorControl(ReleaseContentAccordion);
