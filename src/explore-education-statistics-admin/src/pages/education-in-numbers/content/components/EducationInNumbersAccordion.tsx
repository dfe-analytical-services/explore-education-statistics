import EditableAccordion from '@admin/components/editable/EditableAccordion';
import React, { useCallback } from 'react';
import { EinContent } from '@admin/services/educationInNumbersContentService';
import useEducationInNumbersPageContentActions from '../context/useEducationInNumbersPageContentActions';
import EducationInNumbersAccordionSection from './EducationInNumbersAccordionSection';

export interface EducationInNumbersAccordionProps {
  id?: string;
  title: string;
  pageContent: EinContent;
  onSectionOpen: ({ id, title }: { id: string; title: string }) => void;
}

const EducationInNumbersAccordion = ({
  pageContent,
  id = 'educationInNumbersAccordion',
  title,
  onSectionOpen,
}: EducationInNumbersAccordionProps) => {
  const {
    addContentSection,
    removeContentSection,
    updateContentSectionsOrder,
  } = useEducationInNumbersPageContentActions();

  const onAddSection = useCallback(async () => {
    const newSection = await addContentSection({
      educationInNumbersPageId: pageContent.id,
      order: pageContent.content.length,
    });

    setTimeout(() => {
      const newSectionButton = document.querySelector(
        `#${id}-${newSection.id}-heading`,
      ) as HTMLButtonElement;
      if (newSectionButton) {
        newSectionButton.focus();
      }
    }, 100);
  }, [addContentSection, id, pageContent.content.length, pageContent.id]);

  const reorderAccordionSections = useCallback(
    async (ids: string[]) => {
      const order = ids
        // Strip out the accordion id prefix
        .map(sectionId => sectionId.replace(`${id}-`, ''));

      await updateContentSectionsOrder({
        educationInNumbersPageId: pageContent.id,
        order,
      });
    },
    [id, pageContent.id, updateContentSectionsOrder],
  );

  const handleRemoveSection = useCallback(
    async (sectionId: string) => {
      // Get position of section to remove, for focus handling after deletion
      const removedSectionIndex = pageContent.content.findIndex(
        section => section.id === sectionId,
      );

      const updatedContent = await removeContentSection({
        educationInNumbersPageId: pageContent.id,
        sectionId,
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
    [removeContentSection, id, pageContent.id, pageContent.content],
  );

  return (
    <EditableAccordion
      id={id}
      sectionName={title}
      onAddSection={onAddSection}
      onReorder={reorderAccordionSections}
      onSectionOpen={onSectionOpen}
    >
      {pageContent.content.map(section => (
        <EducationInNumbersAccordionSection
          key={section.id}
          id={`${id}-${section.id}`}
          educationInNumbersPageId={pageContent.id}
          educationInNumbersPageSlug={pageContent.slug}
          section={section}
          onRemoveSection={handleRemoveSection}
        />
      ))}
    </EditableAccordion>
  );
};

export default EducationInNumbersAccordion;
