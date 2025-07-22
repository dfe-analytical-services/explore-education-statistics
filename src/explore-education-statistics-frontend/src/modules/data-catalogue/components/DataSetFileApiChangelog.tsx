import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

interface Props {
  changes: ApiDataSetVersionChanges;
  guidanceNotes: string;
  version: string;
  renderHeading?: boolean;
}

export default function DataSetFileApiChangelog({
  changes,
  guidanceNotes,
  version,
  renderHeading = true,
}: Props) {
  const { majorChanges, minorChanges } = changes;

  if (!Object.keys(majorChanges).length && !Object.keys(minorChanges).length) {
    return null;
  }

  return (
    <DataSetFilePageSection
      heading={renderHeading ?? true ? pageSections.apiChangelog : ''}
      id={renderHeading ?? true ? 'apiChangelog' : undefined}
    >
      {(guidanceNotes ?? '').length > 0 && (
        <p data-testid="public-guidance-notes">{guidanceNotes}</p>
      )}
      <ApiDataSetChangelog
        majorChanges={changes.majorChanges}
        minorChanges={changes.minorChanges}
        version={version}
      />
    </DataSetFilePageSection>
  );
}
