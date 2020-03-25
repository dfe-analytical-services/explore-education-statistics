import Accordion from '@admin/components/EditableAccordion';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import {
  EditableContentBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import { AbstractRelease } from '@common/services/publicationService';
import orderBy from 'lodash/orderBy';
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
}: ReleaseContentAccordionProps) => {
  const {
    addContentSection,
    removeContentSection,
    updateContentSectionHeading,
    updateContentSectionsOrder,
  } = useReleaseActions();

  const addAccordionSection = useCallback(
    () =>
      addContentSection({
        releaseId: release.id,
        order: release.content.length,
      }),
    [release.id, release.content.length, addContentSection],
  );

  const reorderAccordionSections = useCallback(
    async order => {
      updateContentSectionsOrder({ releaseId: release.id, order });
    },
    [release.id, updateContentSectionsOrder],
  );
  return (
    <Accordion
      id={accordionId}
      sectionName={sectionName}
      onSaveOrder={reorderAccordionSections}
      onAddSection={addAccordionSection}
    >
      {orderBy(release.content, 'order').map((accordionSection, index) => (
        <ReleaseContentAccordionSection
          release={release}
          id={accordionSection.id}
          key={accordionSection.id}
          contentItem={accordionSection}
          index={index}
          onHeadingChange={title =>
            updateContentSectionHeading({
              releaseId: release.id,
              sectionId: accordionSection.id,
              title,
            })
          }
          onRemoveSection={() =>
            removeContentSection({
              releaseId: release.id,
              sectionId: accordionSection.id,
            })
          }
        />
      ))}
    </Accordion>
  );
};

export default ReleaseContentAccordion;
