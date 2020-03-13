import Accordion from '@admin/components/EditableAccordion';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import {
  EditableContentBlock,
  EditableRelease,
} from '@admin/services/publicationService';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { AbstractRelease } from '@common/services/publicationService';
import React from 'react';
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
  const {
    addContentSection,
    removeContentSection,
    updateContentSectionHeading,
    updateContentSectionsOrder,
  } = useReleaseActions();

  return (
    <Accordion
      id={accordionId}
      canReorder
      sectionName={sectionName}
      onSaveOrder={async order => {
        updateContentSectionsOrder({ releaseId: release.id, order }).catch(
          handleApiErrors,
        );
      }}
      onAddSection={() =>
        addContentSection({
          releaseId: release.id,
          order: release.content.length,
        }).catch(handleApiErrors)
      }
    >
      {release.content.map((accordionSection, index) => (
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
            }).catch(handleApiErrors)
          }
          onRemoveSection={() =>
            removeContentSection({
              releaseId: release.id,
              sectionId: accordionSection.id,
            }).catch(handleApiErrors)
          }
        />
      ))}
    </Accordion>
  );
};

export default withErrorControl(ReleaseContentAccordion);
