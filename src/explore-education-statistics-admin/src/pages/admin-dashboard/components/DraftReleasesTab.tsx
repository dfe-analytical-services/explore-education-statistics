import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import service from '@admin/services/release/create-release/service';
import React, { useState } from 'react';

interface Props {
  releases: AdminDashboardRelease[];
  onChangeRelease: () => void;
}

const DraftReleasesTab = ({ releases, onChangeRelease }: Props) => {
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();

  return (
    <>
      <ReleasesTab
        releases={releases}
        noReleasesMessage="There are currently no draft releases"
        releaseSummaryRenderer={release => (
          <NonScheduledReleaseSummary
            key={release.id}
            onClickCancelAmendment={setCancelAmendmentReleaseId}
            release={release}
          />
        )}
      />

      {cancelAmendmentReleaseId && (
        <CancelAmendmentModal
          onConfirm={async () => {
            await service.deleteRelease(cancelAmendmentReleaseId);
            setCancelAmendmentReleaseId(undefined);
            onChangeRelease();
          }}
          onCancel={() => setCancelAmendmentReleaseId(undefined)}
        />
      )}
    </>
  );
};

export default DraftReleasesTab;
