import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
import ContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';
import React from 'react';
import { AbstractRelease } from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@common/types/util';

interface ReleaseContentAccordionProps {
  releaseId: string;

  release: AbstractRelease<EditableContentBlock>;
  content: AbstractRelease<EditableContentBlock>['content'];
  accordionId: string;
  sectionName: string;
}

const ReleaseContentAccordion = ({
  releaseId,
  release,
  accordionId,
  sectionName,
}: ReleaseContentAccordionProps) => {
  const [content, setContent] = React.useState<
    AbstractRelease<EditableContentBlock>['content']
  >([]);

  const onReorder = async (ids: Dictionary<number>) => {
    return releaseContentService.updateContentSectionsOrder(releaseId, ids);
  };

  React.useEffect(() => {
    releaseContentService
      .getContentSections(releaseId)
      .then(result => setContent(result));
  }, [releaseId]);

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
                  content={contentdata}
                  id={`content_${order}`}
                  publication={release.publication}
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
