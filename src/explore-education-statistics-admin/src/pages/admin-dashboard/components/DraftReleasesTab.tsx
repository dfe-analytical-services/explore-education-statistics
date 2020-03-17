import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import service from '@admin/services/release/create-release/service';
import React, { useEffect, useState } from 'react';

interface Props {
  initialReleases: AdminDashboardRelease[];
}

const DraftReleasesTab = ({ initialReleases }: Props) => {
  const [releases, setReleases] = useState<AdminDashboardRelease[]>();
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();

  useEffect(() => {
    setReleases(initialReleases);
  }, [initialReleases, setReleases]);

  return (
    <>
      {releases && (
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
              onConfirm={async () =>
                service.deleteRelease(cancelAmendmentReleaseId).then(() => {
                  setReleases(
                    releases.filter(r => r.id !== cancelAmendmentReleaseId),
                  );
                  setCancelAmendmentReleaseId(undefined);
                })
              }
              onCancel={() => setCancelAmendmentReleaseId(undefined)}
            />
          )}
        </>
      )}
    </>
  );
};

export default DraftReleasesTab;
