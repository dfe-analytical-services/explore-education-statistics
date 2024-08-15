import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

interface Props {
  changes: ApiDataSetVersionChanges;
  version: string;
}

export default function DataSetFileApiChangelog({ changes, version }: Props) {
  const { majorChanges, minorChanges } = changes;

  if (!Object.keys(majorChanges).length && !Object.keys(minorChanges).length) {
    return null;
  }

  return (
    <DataSetFilePageSection
      heading={pageSections.apiChangelog}
      id="apiChangelog"
    >
      <ApiDataSetChangelog
        majorChanges={changes.majorChanges}
        minorChanges={changes.minorChanges}
        version={version}
      />
    </DataSetFilePageSection>
  );
}
