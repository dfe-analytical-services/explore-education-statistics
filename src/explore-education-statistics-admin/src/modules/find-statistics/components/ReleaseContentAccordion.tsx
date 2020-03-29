import EditableAccordion from '@admin/components/EditableAccordion';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/publicationService';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useCallback } from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

interface ReleaseContentAccordionProps {
  accordionId: string;
  sectionName: string;
  release: EditableRelease;
}

const ReleaseContentAccordion = ({
  release,
  accordionId,
  sectionName,
}: ReleaseContentAccordionProps) => {
  const { addContentSection, updateContentSectionsOrder } = useReleaseActions();

  const addAccordionSection = useCallback(
    () =>
      addContentSection({
        releaseId: release.id,
        order: release.content.length,
      }),
    [release.id, release.content.length, addContentSection],
  );

  const reorderAccordionSections = useCallback(
    async (ids: string[]) => {
      const order = ids.reduce<Dictionary<number>>((acc, id, index) => {
        acc[id] = index;
        return acc;
      }, {});

      await updateContentSectionsOrder({ releaseId: release.id, order });
    },
    [release.id, updateContentSectionsOrder],
  );

  return (
    <EditableAccordion
      id={accordionId}
      sectionName={sectionName}
      onReorder={reorderAccordionSections}
      onAddSection={addAccordionSection}
    >
      {orderBy(release.content, 'order').map(section => (
        <ReleaseContentAccordionSection
          key={section.id}
          id={section.id}
          release={release}
          section={section}
        />
      ))}
    </EditableAccordion>
  );
};

export default ReleaseContentAccordion;
