import Accordion from '@admin/components/EditableAccordion';
import {
  addContentSection,
  removeContentSection,
  updateContentSectionHeading,
  updateContentSectionsOrder,
} from '@admin/pages/release/edit-release/content/helpers';
import { useReleaseDispatch } from '@admin/pages/release/edit-release/content/ReleaseContext';
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
  const dispatch = useReleaseDispatch();

  return (
    <Accordion
      id={accordionId}
      canReorder
      sectionName={sectionName}
      onSaveOrder={async order => {
        updateContentSectionsOrder(
          dispatch,
          release.id,
          order,
          handleApiErrors,
        );
      }}
      onAddSection={() =>
        addContentSection(
          dispatch,
          release.id,
          release.content.length,
          handleApiErrors,
        )
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
            updateContentSectionHeading(
              dispatch,
              release.id,
              accordionSection.id,
              title,
              handleApiErrors,
            )
          }
          onRemoveSection={() =>
            removeContentSection(
              dispatch,
              release.id,
              accordionSection.id,
              handleApiErrors,
            )
          }
        />
      ))}
    </Accordion>
  );
};

export default withErrorControl(ReleaseContentAccordion);
