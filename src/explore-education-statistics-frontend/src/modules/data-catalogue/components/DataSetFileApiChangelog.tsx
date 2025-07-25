import SectionBreak from '@common/components/SectionBreak';
import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

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
      <SectionBreak size="xl" />

      {patchHistory &&
        patchHistory.map(
          (patch, idx) =>
            (Object.keys(majorChanges).length ||
              Object.keys(minorChanges).length) && (
              <React.Fragment
                key={
                  patch.versionNumber
                    ? `${patch.versionNumber.major}.${patch.versionNumber.minor}.${patch.versionNumber.patch}`
                    : idx
                }
              >
                {patch.notes && (
                  <p data-testid="public-guidance-notes">{patch.notes}</p>
                )}
                <ApiDataSetChangelog
                  majorChanges={patch.majorChanges}
                  minorChanges={patch.minorChanges}
                  version={
                    patch.versionNumber.patch > 0
                      ? `${patch.versionNumber.major}.${patch.versionNumber.minor}.${patch.versionNumber.patch}`
                      : `${patch.versionNumber.major}.${patch.versionNumber.minor}`
                  }
                />
                {idx !== patchHistory.length - 1 && <SectionBreak size="xl" />}
              </React.Fragment>
            ),
        )}
    </DataSetFilePageSection>
  );
}
