import EditableAccordion from '@admin/components/editable/EditableAccordion';
import { useEditingContext } from '@admin/contexts/EditingContext';
import { MethodologyContent } from '@admin/services/methodologyContentService';
import { Dictionary } from '@common/types';
import { ContentSectionKeys } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContextActionTypes';
import useMethodologyContentActions from '@admin/pages/methodology/edit-methodology/content/context/useMethodologyContentActions';
import MethodologyAccordionSection from '@admin/pages/methodology/edit-methodology/content/components/MethodologyAccordionSection';
import React, { useCallback } from 'react';

export interface MethodologyAccordionProps {
  id?: string;
  sectionKey: ContentSectionKeys;
  title: string;
  methodology: MethodologyContent;
  onSectionOpen: ({ id, title }: { id: string; title: string }) => void;
}

const MethodologyAccordion = ({
  sectionKey,
  methodology,
  id = `methodologyAccordion-${sectionKey}`,
  title,
  onSectionOpen,
}: MethodologyAccordionProps) => {
  const { editingMode } = useEditingContext();
  const {
    addContentSection,
    removeContentSection,
    updateContentSectionsOrder,
  } = useMethodologyContentActions();

  const onAddSection = useCallback(async () => {
    const newSection = await addContentSection({
      methodologyId: methodology.id,
      order: methodology[sectionKey].length,
      sectionKey,
    });

    setTimeout(() => {
      const newSectionButton = document.querySelector(
        `#${id}-${newSection.id}-heading`,
      ) as HTMLButtonElement;
      if (newSectionButton) {
        newSectionButton.focus();
      }
    }, 100);
  }, [addContentSection, methodology, sectionKey, id]);

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

  const handleRemoveSection = useCallback(
    async (sectionId: string) => {
      // Get position of section to remove, for focus handling after deletion
      const removedSectionIndex = methodology.content.findIndex(
        section => section.id === sectionId,
      );

      const updatedContent = await removeContentSection({
        methodologyId: methodology.id,
        sectionId,
        sectionKey,
      });

      // Section has been removed, now move focus to:
      // the previous section if exists
      // otherwise to the first section if exists,
      // otherwise to the 'add section' button
      setTimeout(() => {
        let buttonToFocus = document.querySelector(
          `#add-section-button-${id}`,
        ) as HTMLButtonElement;

        if (updatedContent.length > 0) {
          const sectionToFocus = updatedContent.at(
            Math.max(removedSectionIndex - 1, 0),
          );

          if (sectionToFocus) {
            buttonToFocus = document.querySelector(
              `#${id}-${sectionToFocus.id}-heading`,
            ) as HTMLButtonElement;
          }
        }
        buttonToFocus?.focus();
      }, 100);
    },
    [removeContentSection, id, methodology.id, methodology.content],
  );

  if (
    sectionKey === 'annexes' &&
    editingMode !== 'edit' &&
    methodology.annexes.length < 1
  )
    return null;
  return (
    <EditableAccordion
      id={id}
      sectionName={title}
      onAddSection={onAddSection}
      onReorder={reorderAccordionSections}
      onSectionOpen={onSectionOpen}
    >
      {methodology[sectionKey].map(section => (
        <MethodologyAccordionSection
          key={section.id}
          id={`${id}-${section.id}`}
          methodologyId={methodology.id}
          methodologySlug={methodology.slug}
          section={section}
          sectionKey={sectionKey}
          onRemoveSection={handleRemoveSection}
        />
      ))}
    </EditableAccordion>
  );
};

export default MethodologyAccordion;
