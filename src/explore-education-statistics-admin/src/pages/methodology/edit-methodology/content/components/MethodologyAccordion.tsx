import EditableAccordion from '@admin/components/EditableAccordion';
import { MethodologyContent } from '@admin/services/methodology/types';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types';
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
    [addContentSection, methodology, sectionKey],
  );

  const reorderAccordionSections = useCallback(
    async (ids: string[]) => {
      const order = ids.reduce<Dictionary<number>>((acc, id, index) => {
        acc[id] = index;
        return acc;
      }, {});

      await updateContentSectionsOrder({
        methodologyId: methodology.id,
        order,
        sectionKey,
      });
    },
    [methodology.id, sectionKey, updateContentSectionsOrder],
  );

  if (sectionKey === 'annexes' && !isEditing && methodology.annexes.length < 1)
    return null;
  return (
    <EditableAccordion
      id={`methodology-accordion-${sectionKey}`}
      sectionName={sectionKey}
      onAddSection={onAddSection}
      onReorder={reorderAccordionSections}
    >
      {orderBy(methodology[sectionKey], 'order').map(section => (
        <MethodologyAccordionSection
          key={section.id}
          id={section.id}
          methodologyId={methodology.id}
          section={section}
          sectionKey={sectionKey}
        />
      ))}
    </EditableAccordion>
  );
};

export default MethodologyAccordion;
