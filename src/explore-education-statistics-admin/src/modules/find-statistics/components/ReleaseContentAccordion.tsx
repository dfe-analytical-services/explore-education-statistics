import EditableAccordion from '@admin/components/editable/EditableAccordion';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useCallback } from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

interface ReleaseContentAccordionProps {
  id?: string;
  sectionName: string;
  release: EditableRelease;
}

const ReleaseContentAccordion = ({
  release,
  id = 'releaseContentAccordion',
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
      const order = ids
        // Strip out the accordion id prefix
        .map(sectionId => sectionId.replace(`${id}-`, ''))
        .reduce<Dictionary<number>>((acc, sectionId, index) => {
          acc[sectionId] = index;
          return acc;
        }, {});

      await updateContentSectionsOrder({ releaseId: release.id, order });
    },
    [id, release.id, updateContentSectionsOrder],
  );

  return (
    <EditableAccordion
      id={id}
      sectionName={sectionName}
      onReorder={reorderAccordionSections}
      onAddSection={addAccordionSection}
    >
      {orderBy(release.content, 'order').map(section => (
        <ReleaseContentAccordionSection
          key={section.id}
          id={`${id}-${section.id}`}
          release={release}
          section={section}
        />
      ))}
    </EditableAccordion>
  );
};

export default ReleaseContentAccordion;
