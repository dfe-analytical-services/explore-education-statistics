import ButtonLink from '@admin/components/ButtonLink';
import ScheduledReleasesTable from '@admin/pages/admin-dashboard/components/ScheduledReleasesTable';
import ReleasesTab from '@admin/pages/admin-dashboard/components/ReleasesByStatusTab';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { MyRelease } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  isLoading: boolean;
  releases: MyRelease[];
  showNewDashboard?: boolean; // TODO EES-3217 remove when ready to go live
}

const ScheduledReleasesTab = ({
  isLoading,
  releases,
  showNewDashboard = false,
}: Props) => {
  if (showNewDashboard) {
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
  }
  return (
    <LoadingSpinner
      hideText
      loading={isLoading}
      text="Loading scheduled releases"
    >
      <ReleasesTab
        releases={releases}
        noReleasesMessage="There are currently no scheduled releases"
        renderReleaseSummary={release => (
          <ReleaseSummary
            key={release.id}
            release={release}
            actions={
              <ButtonLink
                to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                  publicationId: release.publicationId,
                  releaseId: release.id,
                })}
                variant="secondary"
              >
                View release
              </ButtonLink>
            }
          />
        )}
      />
    </LoadingSpinner>
  );
};

export default ScheduledReleasesTab;
