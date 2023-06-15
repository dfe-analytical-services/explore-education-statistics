import React from 'react';
import DraftReleasesTable from '@admin/pages/admin-dashboard/components/DraftReleasesTable';
import { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';

interface Props {
  isBauUser: boolean;
  isLoading: boolean;
  releases: Release[];
  onChangeRelease: () => void;
}

const DraftReleasesTab = ({
  isBauUser,
  isLoading,
  releases,
  onChangeRelease,
}: Props) => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h2>Draft releases</h2>
          <p>
            Here you can view and edit any of your releases that are currently
            in 'Draft' or 'In review' and also 'Amendments' that are being made
            to a published release. You can also view a summary of any
            outstanding issues that may need to be resolved.
          </p>
        </div>
      </div>
      <LoadingSpinner
        hideText
        loading={isLoading}
        text="Loading draft releases"
      >
        <DraftReleasesTable
          isBauUser={isBauUser}
          releases={releases}
          onChangeRelease={onChangeRelease}
        />
      </LoadingSpinner>
    </>
  );
};

export default DraftReleasesTab;
