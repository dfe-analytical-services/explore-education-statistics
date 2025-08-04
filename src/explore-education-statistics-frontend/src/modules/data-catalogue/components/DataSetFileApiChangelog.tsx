import SectionBreak from '@common/components/SectionBreak';
import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';
import sortVersionChanges from '../utils/sortVersionChanges';

interface Props {
  changes: ApiDataSetVersionChanges;
  guidanceNotes?: string;
  version: string;
  patchHistory: ApiDataSetVersionChanges[];
}

export default function DataSetFileApiChangelog({
  changes,
  guidanceNotes,
  version,
  patchHistory,
}: Props) {
  const { majorChanges, minorChanges } = changes;

  if (
    !Object.keys(majorChanges).length &&
    !Object.keys(minorChanges).length &&
    patchHistory.every(
      change => !change || !Object.keys(change.minorChanges).length,
    )
  ) {
    return null;
  }

  return (
    <DataSetFilePageSection
      heading={pageSections.apiChangelog}
      id="apiChangelog"
    >
      {guidanceNotes && (
        <p data-testid="public-guidance-notes">{guidanceNotes}</p>
      )}
      <ApiDataSetChangelog
        majorChanges={changes.majorChanges}
        minorChanges={changes.minorChanges}
        version={version}
      />
      {patchHistory.length > 0 && <SectionBreak size="xl" />}

      {patchHistory &&
        sortVersionChanges(patchHistory).map(
          (patch, idx) =>
            (Object.keys(patch.majorChanges).length ||
              Object.keys(patch.minorChanges).length ||
              (patch.notes?.length ?? 0) > 0) && (
              <React.Fragment key={`${patch.versionNumber}`}>
                {patch.notes && (
                  <p data-testid="public-guidance-notes">{patch.notes}</p>
                )}
                <ApiDataSetChangelog
                  majorChanges={patch.majorChanges}
                  minorChanges={patch.minorChanges}
                  version={patch.versionNumber}
                />
                {patchHistory.length > 0 && idx !== patchHistory.length - 1 && (
                  <SectionBreak size="xl" />
                )}
              </React.Fragment>
            ),
        )}
    </DataSetFilePageSection>
  );
}
