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

  React.useEffect(() => {
    releaseContentService
      .getContentSections(release.id)
      .then(result => setContent(result));
  }, [release.id]);

  const onAddContent = async (
    sectionId: string | undefined,
    type: string,
    order: number | undefined,
  ) => {
    if (sectionId) {
      await releaseContentService.addContentSectionBlock(
        release.id,
        sectionId,
        {
          body: 'Click to edit',
          type,
          order,
        },
      );

      const result = await releaseContentService.getContentSections(release.id);

      setContent(result);
    }
  };

  return (
    <>
      {content && content.length > 0 && (
        <Accordion
          id={accordionId}
          canReorder
          sectionName={sectionName}
          onSaveOrder={onReorder}
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
                  onAddContent={(type, position) =>
                    onAddContent(id, type, position)
                  }
                />
              </AccordionSection>
            ),
          )}
        </Accordion>
      )}
    </>
  );
};

export default ReleaseContentAccordion;
