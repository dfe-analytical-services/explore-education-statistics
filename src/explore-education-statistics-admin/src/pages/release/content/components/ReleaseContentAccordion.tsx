import EditableAccordion from '@admin/components/editable/EditableAccordion';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableReleaseVersion } from '@admin/services/releaseContentService';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useCallback } from 'react';
import ReleaseContentAccordionSection from './ReleaseContentAccordionSection';

interface ReleaseContentAccordionProps {
  id?: string;
  release: EditableReleaseVersion;
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
  const { addContentSection, updateContentSectionsOrder } =
    useReleaseContentActions();

  const addAccordionSection = useCallback(async () => {
    await addContentSection({
      releaseVersionId: release.id,
      order: release.content.length,
    });
  }, [release.id, release.content.length, addContentSection]);

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
        />
      ))}
    </EditableAccordion>
  );
};

export default ReleaseContentAccordion;
