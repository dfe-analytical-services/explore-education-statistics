import Accordion from '@admin/components/EditableAccordion';
import React from 'react';
import { AbstractRelease } from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@common/types/util';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

export type ContentType = AbstractRelease<EditableContentBlock>['content'][0];

interface ReleaseContentAccordionProps {
  releaseId: string;
  publication: AbstractRelease<EditableContentBlock>['publication'];
  accordionId: string;
  sectionName: string;
  onContentChange?: (content: ContentType[]) => void;
}

type ContentBlock = AbstractRelease<EditableContentBlock>['content'];

const ReleaseContentAccordion = ({
  releaseId,
  publication,
  accordionId,
  sectionName,
  onContentChange,
}: ReleaseContentAccordionProps) => {
  const [content, _setContent] = React.useState<ContentType[]>([]);

  const setContent = React.useCallback(
    (newContent: ContentType[]) => {
      if (onContentChange) onContentChange(newContent);
      _setContent(newContent);
    },
    [onContentChange],
  );

  const onReorder = React.useCallback(
    async (ids: Dictionary<number>) => {
      const newContent = await releaseContentService.updateContentSectionsOrder(
        releaseId,
        ids,
      );
      setContent(newContent);
    },
    [releaseId, setContent],
  );

  React.useEffect(() => {
    const f = async (rid: string) => {
      const newContent = await releaseContentService.getContentSections(rid);
      if (releaseId === rid) setContent(newContent);
    };
    f(releaseId);
  }, [releaseId]);

  const onAddSection = React.useCallback(async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService.addContentSection(releaseId, content.length),
    ];

    setContent(newContent);
  }, [content, releaseId, setContent]);

  const onUpdateHeading = React.useCallback(
    async (block: ContentType, index: number, newTitle: string) => {
      let result;
      if (block.id) {
        result = await releaseContentService.updateContentSectionHeading(
          releaseId,
          block.id,
          newTitle,
        );

        const newContent = [...content];
        newContent[index].heading = newTitle;
        setContent(newContent);
      }
      return result;
    },
    [content, releaseId, setContent],
  );

  const updateContentSection = React.useCallback(
    (index: number, contentBlock?: EditableContentBlock[]) => {
      const newContent = [...content];
      newContent[index].content = contentBlock;
      setContent(newContent);
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

export default ReleaseContentAccordion;
