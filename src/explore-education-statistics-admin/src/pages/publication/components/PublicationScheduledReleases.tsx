import styles from '@admin/pages/publication//PublicationReleasesPage.module.scss';
import ScheduledReleaseRow from '@admin/pages/publication/components/ScheduledReleaseRow';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseVersionService';
import InsetText from '@common/components/InsetText';
import React from 'react';

interface Props {
  publicationId: string;
  releases: ReleaseVersionSummaryWithPermissions[];
}

const PublicationScheduledReleases = ({ publicationId, releases }: Props) => {
  if (releases.length === 0) {
    return <InsetText>You have no scheduled releases.</InsetText>;
  }

  return (
    <table data-testid="publication-scheduled-releases">
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Release period</th>
          <th className={`${styles.statusColumn} dfe-white-space--nowrap`}>
            Status <ScheduledStatusGuidanceModal />
          </th>
          <th className="govuk-!-width-one-quarter dfe-white-space--nowrap">
            Stages checklist <ScheduledStagesGuidanceModal />
          </th>
          <th>Publish date</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {releases.map(release => (
          <ScheduledReleaseRow
            key={release.id}
            publicationId={publicationId}
            release={release}
          />
        ))}
      </tbody>
    </table>
  );
};

export default PublicationScheduledReleases;
