import DraftReleasesTable from '@admin/pages/admin-dashboard/components/DraftReleasesTable';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import { MyRelease } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';

interface Props {
  isBauUser: boolean;
  isLoading: boolean;
  releases: MyRelease[];
  showNewDashboard?: boolean; // TODO EES-3217 remove when ready to go live
  onChangeRelease: () => void;
}

const DraftReleasesTab = ({
  isBauUser,
  isLoading,
  releases,
  showNewDashboard = false,
  onChangeRelease,
}: Props) => {
  if (showNewDashboard) {
    return (
      <>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <h2>Draft releases</h2>
            <p>
              Here you can view and edit any of your releases that are currently
              in 'Draft' or 'In review' and also 'Amendments' that are being
              made to a published release. You can also view a summary of any
              outstanding issues that may need to be resolved.
            </p>
          </div>
        </div>

        <DraftReleasesTable
          isBauUser={isBauUser}
          isLoading={isLoading}
          releases={releases}
          onChangeRelease={onChangeRelease}
        />
      </>
    );
  }
  return (
    <LoadingSpinner loading={isLoading}>
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
    </LoadingSpinner>
  );
};

export default DraftReleasesTab;
