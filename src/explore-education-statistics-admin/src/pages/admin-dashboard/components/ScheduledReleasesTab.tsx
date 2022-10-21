import ScheduledReleasesTable from '@admin/pages/admin-dashboard/components/ScheduledReleasesTable';
import { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';

interface Props {
  isLoading: boolean;
  releases: Release[];
}

const ScheduledReleasesTab = ({ isLoading, releases }: Props) => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h2>Approved scheduled releases</h2>
          <p>
            Here you can view releases that have been approved and are now
            scheduled for publication. You can also check the progress of any
            releases that are currently being published to live.
          </p>
        </div>
      </div>
      <LoadingSpinner
        hideText
        loading={isLoading}
        text="Loading scheduled releases"
      >
        <ScheduledReleasesTable releases={releases} />
      </LoadingSpinner>
    </>
  );
};

export default ScheduledReleasesTab;
