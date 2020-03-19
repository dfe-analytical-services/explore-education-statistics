import EditableAccordion from '@admin/components/EditableAccordion';
import { MethodologyContent } from '@admin/services/methodology/types';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import orderBy from 'lodash/orderBy';
import React, { useCallback, useContext } from 'react';
import { ContentSectionKeys } from '../context/MethodologyContextActionTypes';
import useMethodologyActions from '../context/useMethodologyActions';
import MethodologyAccordionSection from './MethodologyAccordionSection';

export interface MethodologyAccordionProps {
  sectionKey: ContentSectionKeys;
  methodology: MethodologyContent;
}

const MethodologyAccordion = ({
  sectionKey,
  methodology,
}: MethodologyAccordionProps) => {
  const { isEditing } = useContext(EditingContext);
  const {
    addContentSection,
    updateContentSectionsOrder,
  } = useMethodologyActions();

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

  if (sectionKey === 'annexes' && !isEditing && methodology.annexes.length < 1)
    return null;
  return (
    <EditableAccordion
      id={`methodology-accordion-${sectionKey}`}
      sectionName={sectionKey === 'annexes' ? 'Annexes' : 'Content'}
      onAddSection={onAddSection}
      onSaveOrder={reorderAccordionSections}
    >
      {orderBy(
        sectionKey === 'annexes' ? methodology.annexes : methodology.content,
        'order',
      ).map((accordionSection, index) => (
        <MethodologyAccordionSection
          id={accordionSection.id}
          methodologyId={methodology.id}
          key={accordionSection.id}
          content={accordionSection}
          sectionKey={sectionKey}
          index={index}
        />
      ))}
    </EditableAccordion>
  );
};

export default MethodologyAccordion;
