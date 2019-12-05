import Accordion from '@admin/components/EditableAccordion';
import React from 'react';
import { AbstractRelease } from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@common/types/util';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

export type ContentType = AbstractRelease<EditableContentBlock>['content'][0];

interface ReleaseContentAccordionProps {
  release: AbstractRelease<EditableContentBlock>;
  content: ContentType[];
  accordionId: string;
  sectionName: string;
}

type ContentBlock = AbstractRelease<EditableContentBlock>['content'];

const ReleaseContentAccordion = ({
  release,
  accordionId,
  sectionName,
}: ReleaseContentAccordionProps) => {
  const [content, setContent] = React.useState<ContentType[]>([]);

  const onReorder = React.useCallback(
    async (ids: Dictionary<number>) => {
      const newContent = await releaseContentService.updateContentSectionsOrder(
        release.id,
        ids,
      );
      setContent(newContent);
    },
    [release.id],
  );

  React.useEffect(() => {
    releaseContentService.getContentSections(release.id).then(setContent);
  }, [release.id]);

  const onAddSection = React.useCallback(async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService.addContentSection(release.id, content.length),
    ];

    setContent(newContent);
  }, [content, release.id]);

  const onUpdateHeading = React.useCallback(
    async (block: ContentType, index: number, newTitle: string) => {
      let result;
      if (block.id) {
        result = await releaseContentService.updateContentSectionHeading(
          release.id,
          block.id,
          newTitle,
        );

        const newContent = [...content];
        newContent[index].heading = newTitle;
        setContent(newContent);
      }
      return result;
    },
    [content, release.id],
  );

  const updateContentSection = (
    index: number,
    contentBlock?: EditableContentBlock[],
  ) => {
    const newContent = [...content];
    newContent[index].content = contentBlock;
    setContent(newContent);
  };

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
            release={release}
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
