import styles from '@admin/pages/publication//PublicationReleasesPage.module.scss';
import ScheduledReleaseRow from '@admin/pages/publication/components/ScheduledReleaseRow';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseVersionService';
import InsetText from '@common/components/InsetText';
import ButtonGroup from '@common/components/ButtonGroup';
import BackToTopLink from '@common/components/BackToTopLink';
import React from 'react';

interface Props {
  publicationId: string;
  releases: ReleaseVersionSummaryWithPermissions[];
  showBackToTopLink?: boolean;
}

const PublicationScheduledReleases = ({
  publicationId,
  releases,
  showBackToTopLink,
}: Props) => {
  if (releases.length === 0) {
    return <InsetText>You have no scheduled releases.</InsetText>;
  }

  return (
    <>
      <table data-testid="publication-scheduled-releases">
        <caption className="govuk-visually-hidden">
          Table showing the scheduled releases for this publication.
        </caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th className={`${styles.statusColumn} dfe-white-space--nowrap`}>
              Status
            </th>
            <th className="govuk-!-width-one-quarter dfe-white-space--nowrap">
              Stages checklist
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
      <ButtonGroup
        className={showBackToTopLink ? 'govuk-!-margin-bottom-2' : undefined}
      >
        <ScheduledStatusGuidanceModal />
        <ScheduledStagesGuidanceModal />
      </ButtonGroup>
      {showBackToTopLink && <BackToTopLink />}
    </>
  );
};

export default PublicationScheduledReleases;
