import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import releaseService, {
  DeleteReleasePlan,
  MyRelease,
} from '@admin/services/releaseService';
import React, { useState } from 'react';

interface Props {
  releases: MyRelease[];
  onChangeRelease: () => void;
}

const DraftReleasesTab = ({ releases, onChangeRelease }: Props) => {
  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan
  >();

  return (
    <>
      <ReleasesTab
        releases={releases}
        noReleasesMessage="There are currently no draft releases"
        releaseSummaryRenderer={release => (
          <NonScheduledReleaseSummary
            key={release.id}
            onClickCancelAmendment={async releaseId => {
              setDeleteReleasePlan(
                await releaseService.getDeleteReleasePlan(releaseId),
              );
            }}
            release={release}
          />
        )}
      />

      {deleteReleasePlan && (
        <CancelAmendmentModal
          methodologiesScheduledWithRelease={
            deleteReleasePlan.methodologiesScheduledWithRelease
          }
          onConfirm={async () => {
            await releaseService.deleteRelease(deleteReleasePlan.releaseId);
            setDeleteReleasePlan(undefined);
            onChangeRelease();
          }}
          onCancel={() => setDeleteReleasePlan(undefined)}
        />
      )}
    </>
  );
};

export default DraftReleasesTab;
