import Accordion from '@admin/components/EditableAccordion';
import {
  EditableContentBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { AbstractRelease } from '@common/services/publicationService';
import { Dictionary } from '@common/types/util';
import React, { useCallback } from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

export type ContentType = AbstractRelease<EditableContentBlock>['content'][0];

interface ReleaseContentAccordionProps {
  accordionId: string;
  sectionName: string;
  release: EditableRelease;
}

const ReleaseContentAccordion = ({
  release,
  accordionId,
  sectionName,
  handleApiErrors,
}: ReleaseContentAccordionProps & ErrorControlProps) => {
  const { content } = release;

  const onReorder = useCallback(
    async (ids: Dictionary<number>) => {
      const newContent = await releaseContentService
        .updateContentSectionsOrder(release.id, ids)
        .catch(handleApiErrors);
    },
    [release.id, handleApiErrors],
  );

  const onAddSection = useCallback(async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService
        .addContentSection(release.id, content.length)
        .catch(handleApiErrors),
    ];
  }, [content, release.id, handleApiErrors]);

  const onUpdateHeading = useCallback(
    async (block: ContentType, index: number, newTitle: string) => {
      let result;
      if (block.id) {
        result = await releaseContentService
          .updateContentSectionHeading(release.id, block.id, newTitle)
          .catch(handleApiErrors);

        const newContent = [...content];
        newContent[index].heading = newTitle;
      }
      return result;
    },
    [content, release.id, handleApiErrors],
  );

  const updateContentSection = useCallback(
    (index: number, contentBlock?: EditableContentBlock[]) => {
      const newContent = [...content];
      newContent[index].content = contentBlock;
    },
    [content],
  );

  const onRemoveContentSection = useCallback(
    async (block: ContentType) => {
      if (block.id) {
        await releaseContentService
          .removeContentSection(release.id, block.id)
          .catch(handleApiErrors);

        const newContent = content.filter(item => item.id !== block.id);
      }
    },
    [content, release.id, handleApiErrors],
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
            release={release}
            id={contentItem.id as string}
            key={contentItem.order}
            contentItem={contentItem}
            index={index}
            onHeadingChange={title =>
              onUpdateHeading(contentItem, index, title)
            }
            onContentChange={newContent =>
              updateContentSection(index, newContent)
            }
            onRemoveSection={() => onRemoveContentSection(contentItem)}
          />
        ))}
      </Accordion>
    </>
  );
};

export default withErrorControl(ReleaseContentAccordion);
