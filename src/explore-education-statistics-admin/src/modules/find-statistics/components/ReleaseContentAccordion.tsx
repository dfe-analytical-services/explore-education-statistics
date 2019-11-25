import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
import ContentBlock, {
  EditingContentBlockContext,
} from '@admin/modules/find-statistics/components/EditableContentBlock';
import React from 'react';
import { AbstractRelease } from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import { Dictionary } from '@common/types/util';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';

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

  const releaseContext = React.useContext(EditingContext);

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
              <EditingContentBlockContext.Provider
                key={order}
                value={{
                  ...releaseContext,
                  sectionId: id,
                }}
              >
                <AccordionSection
                  id={id}
                  index={index}
                  heading={heading || ''}
                  caption={caption}
                >
                  <ContentBlock
                    content={contentdata}
                    id={`content_${order}`}
                    publication={release.publication}
                  />
                </AccordionSection>
              </EditingContentBlockContext.Provider>
            ),
          )}
        </Accordion>
      )}
    </>
  );
};

export default ReleaseContentAccordion;
