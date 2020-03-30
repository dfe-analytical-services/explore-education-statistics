import EditableAccordion from '@admin/components/EditableAccordion';
import { MethodologyContent } from '@admin/services/methodology/types';
import { useEditingContext } from '@common/contexts/EditingContext';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useCallback } from 'react';
import { ContentSectionKeys } from '../context/MethodologyContextActionTypes';
import useMethodologyActions from '../context/useMethodologyActions';
import MethodologyAccordionSection from './MethodologyAccordionSection';

export interface MethodologyAccordionProps {
  id?: string;
  sectionKey: ContentSectionKeys;
  methodology: MethodologyContent;
}

const MethodologyAccordion = ({
  sectionKey,
  methodology,
  id = `methodologyAccordion-${sectionKey}`,
}: MethodologyAccordionProps) => {
  const { isEditing } = useEditingContext();
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
      const order = ids
        // Strip out the accordion id prefix
        .map(sectionId => sectionId.replace(`${id}-`, ''))
        .reduce<Dictionary<number>>((acc, sectionId, index) => {
          acc[sectionId] = index;
          return acc;
        }, {});

      await updateContentSectionsOrder({
        methodologyId: methodology.id,
        order,
        sectionKey,
      });
    },
    [id, methodology.id, sectionKey, updateContentSectionsOrder],
  );

  if (sectionKey === 'annexes' && !isEditing && methodology.annexes.length < 1)
    return null;
  return (
    <EditableAccordion
      id={id}
      sectionName={sectionKey}
      onAddSection={onAddSection}
      onReorder={reorderAccordionSections}
    >
      {orderBy(methodology[sectionKey], 'order').map(section => (
        <MethodologyAccordionSection
          key={section.id}
          id={`${id}-${section.id}`}
          methodologyId={methodology.id}
          section={section}
          sectionKey={sectionKey}
        />
      ))}
    </EditableAccordion>
  );
};

export default MethodologyAccordion;
