import EditableAccordion from '@admin/components/editable/EditableAccordion';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useCallback } from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

interface ReleaseContentAccordionProps {
  id?: string;
  release: EditableRelease;
  sectionName: string;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
  onSectionOpen: ({ id, title }: { id: string; title: string }) => void;
}

const ReleaseContentAccordion = ({
  release,
  id = 'releaseMainContent',
  sectionName,
  transformFeaturedTableLinks,
  onSectionOpen,
}: ReleaseContentAccordionProps) => {
  const {
    addContentSection,
    removeContentSection,
    updateContentSectionsOrder,
  } = useReleaseContentActions();

  const addAccordionSection = useCallback(async () => {
    const newSection = await addContentSection({
      releaseVersionId: release.id,
      order: release.content.length,
    });

    setTimeout(() => {
      const newSectionButton = document.querySelector(
        `#${id}-${newSection.id}-heading`,
      ) as HTMLButtonElement;
      if (newSectionButton) {
        newSectionButton.focus();
      }
    }, 100);
  }, [release.id, release.content.length, addContentSection, id]);

  const handleRemoveSection = useCallback(
    async (sectionId: string) => {
      // Get position of section to remove, for focus handling after deletion
      const removedSectionIndex = release.content.findIndex(
        section => section.id === sectionId,
      );

      const updatedContent = await removeContentSection({
        sectionId,
        releaseVersionId: release.id,
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
    [removeContentSection, release.id, release.content, id],
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

      await updateContentSectionsOrder({ releaseVersionId: release.id, order });
    },
    [id, release.id, updateContentSectionsOrder],
  );

  return (
    <EditableAccordion
      id={id}
      sectionName={sectionName}
      onReorder={reorderAccordionSections}
      onAddSection={addAccordionSection}
      onSectionOpen={onSectionOpen}
    >
      {orderBy(release.content, 'order').map(section => (
        <ReleaseContentAccordionSection
          key={section.id}
          id={`${id}-${section.id}`}
          section={section}
          transformFeaturedTableLinks={transformFeaturedTableLinks}
          onRemoveSection={handleRemoveSection}
        />
      ))}
    </EditableAccordion>
  );
};

export default ReleaseContentAccordion;
