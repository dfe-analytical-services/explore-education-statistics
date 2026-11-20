import DraftReleasesTable from '@admin/pages/admin-dashboard/components/DraftReleasesTable';
import { DashboardReleaseVersionSummary } from '@admin/services/releaseVersionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';

interface Props {
  isBauUser: boolean;
  isLoading: boolean;
  releases: DashboardReleaseVersionSummary[];
  onChangeRelease: () => void;
}

const DraftReleasesTab = ({
  isBauUser,
  isLoading,
  releases,
  onChangeRelease,
}: Props) => {
  const showIssues = !isBauUser;
  const showMethodology = !isBauUser;

  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h2>Draft releases</h2>
          <p>
            Here you can view and edit any of your releases that are currently
            in 'Draft' or 'In review' and also 'Amendments' that are being made
            to a published release.
          </p>
          {showIssues && (
            <p>
              You can also view a summary of any outstanding issues that may
              need to be resolved.
            </p>
          )}
        </div>
      </div>
      <LoadingSpinner
        hideText
        loading={isLoading}
        text="Loading draft releases"
      >
        <DraftReleasesTable
          showIssues={showIssues}
          showMethodology={showMethodology}
          releases={releases}
          onChangeRelease={onChangeRelease}
        />
      </LoadingSpinner>
    </>
  );
};

export default DraftReleasesTab;
