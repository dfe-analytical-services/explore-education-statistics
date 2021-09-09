import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import { MyRelease } from '@admin/services/releaseService';
import React from 'react';

interface Props {
  releases: MyRelease[];
  onChangeRelease: () => void;
}

const DraftReleasesTab = ({ releases, onChangeRelease }: Props) => {
  return (
    <ReleasesTab
      releases={releases}
      noReleasesMessage="There are currently no draft releases"
      releaseSummaryRenderer={release => (
        <NonScheduledReleaseSummary
          key={release.id}
          release={release}
          onAmendmentCancelled={onChangeRelease}
        />
      )}
    />
  );
};

export default DraftReleasesTab;
