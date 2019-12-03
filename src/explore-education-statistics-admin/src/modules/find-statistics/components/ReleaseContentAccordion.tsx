import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
import ContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';
import React from 'react';
import { AbstractRelease } from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@common/types/util';

interface ReleaseContentAccordionProps {
  release: AbstractRelease<EditableContentBlock>;
  content: AbstractRelease<EditableContentBlock>['content'];
  accordionId: string;
  sectionName: string;
}

type ContentBlock = AbstractRelease<EditableContentBlock>['content'];

const ReleaseContentAccordion = ({
  release,
  accordionId,
  sectionName,
}: ReleaseContentAccordionProps) => {
  const [content, setContent] = React.useState<
    AbstractRelease<EditableContentBlock>['content']
  >([]);

  const onReorder = async (ids: Dictionary<number>) => {
    return releaseContentService.updateContentSectionsOrder(release.id, ids);
  };

  const updateContent = async (id: string) => {
    setContent(await releaseContentService.getContentSections(id));
  };

  React.useEffect(() => {
    updateContent(release.id);
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
      <Accordion
        id={accordionId}
        canReorder
        sectionName={sectionName}
        onSaveOrder={onReorder}
        onAddSection={onAddSection}
      >
        {content.map(
          ({ id, heading, caption, order, content: contentdata }, index) => (
            <AccordionSection
              id={id}
              index={index}
              heading={heading || ''}
              caption={caption}
              key={order}
            >
              <ContentBlock
                canAddBlocks
                sectionId={id}
                content={contentdata}
                id={`content_${order}`}
                publication={release.publication}
              />
            </AccordionSection>
          ),
        )}
      </Accordion>
    </>
  );
};

export default ReleaseContentAccordion;
