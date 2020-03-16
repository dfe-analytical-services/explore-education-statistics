import EditableAccordion from '@admin/components/EditableAccordion';
import { MethodologyContent } from '@admin/services/methodology/types';
import React, { useCallback } from 'react';
import useMethodologyActions from '../context/useMethodologyActions';
import MethodologyAccordionSection from './MethodologyAccordionSection';

interface MethodologyAccordionProps {
  isAnnex?: boolean;
  methodology: MethodologyContent;
}

const MethodologyAccordion = ({
  isAnnex = false,
  methodology,
}: MethodologyAccordionProps) => {
  const {
    addContentSection,
    updateContentSectionsOrder,
  } = useMethodologyActions();
  const sectionKey = isAnnex ? 'annexes' : 'content';

  const onAddSection = useCallback(
    () =>
      addContentSection({
        methodologyId: methodology.id,
        order: methodology[sectionKey].length,
        sectionKey,
      }),
    [methodology.id, addContentSection],
  );

  const reorderAccordionSections = useCallback(
    async order => {
      updateContentSectionsOrder({
        methodologyId: methodology.id,
        order,
        sectionKey,
      });
    },
    [methodology.id, updateContentSectionsOrder],
  );

  return (
    <EditableAccordion
      id={`methodology-accordion-${sectionKey}`}
      sectionName={isAnnex ? 'Annexes' : 'Content'}
      onAddSection={onAddSection}
      onSaveOrder={reorderAccordionSections}
    >
      {(isAnnex ? methodology.annexes : methodology.content).map(
        accordionSection => (
          <MethodologyAccordionSection
            key={accordionSection.id}
            content={accordionSection}
            isAnnex={isAnnex}
          />
        ),
      )}
    </EditableAccordion>
  );
};

export default MethodologyAccordion;
