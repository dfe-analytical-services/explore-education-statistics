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

  const onReorder = async (ids: Dictionary<number>) => {
    return releaseContentService.updateContentSectionsOrder(release.id, ids);
  };

  React.useEffect(() => {
    releaseContentService.getContentSections(release.id).then(setContent);
  }, [release.id]);

  const onAddSection = async () => {
    const newContent: AbstractRelease<EditableContentBlock>['content'] = [
      ...content,
      await releaseContentService.addContentSection(release.id, content.length),
    ];

    setContent(newContent);
  };

  return (
    <>
      {content && content.length > 0 && (
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
            />
          ))}
        </Accordion>
      )}
    </>
  );
};

export default ReleaseContentAccordion;
