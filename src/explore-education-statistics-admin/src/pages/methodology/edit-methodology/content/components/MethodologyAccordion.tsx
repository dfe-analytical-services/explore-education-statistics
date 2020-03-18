import EditableAccordion from '@admin/components/EditableAccordion';
import { MethodologyContent } from '@admin/services/methodology/types';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import orderBy from 'lodash/orderBy';
import React, { useCallback, useContext } from 'react';
import useMethodologyActions from '../context/useMethodologyActions';
import MethodologyAccordionSection from './MethodologyAccordionSection';

export interface MethodologyAccordionProps {
  isAnnex?: boolean;
  methodology: MethodologyContent;
}

const MethodologyAccordion = ({
  isAnnex = false,
  methodology,
}: MethodologyAccordionProps) => {
  const { isEditing } = useContext(EditingContext);
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

  if (isAnnex && !isEditing && methodology.annexes.length < 1) return null;
  return (
    <EditableAccordion
      id={`methodology-accordion-${sectionKey}`}
      sectionName={isAnnex ? 'Annexes' : 'Content'}
      onAddSection={onAddSection}
      onSaveOrder={reorderAccordionSections}
    >
      {orderBy(
        isAnnex ? methodology.annexes : methodology.content,
        'order',
      ).map((accordionSection, index) => (
        <MethodologyAccordionSection
          id={accordionSection.id}
          methodologyId={methodology.id}
          key={accordionSection.id}
          content={accordionSection}
          isAnnex={isAnnex}
          index={index}
        />
      ))}
    </EditableAccordion>
  );
};

export default MethodologyAccordion;
