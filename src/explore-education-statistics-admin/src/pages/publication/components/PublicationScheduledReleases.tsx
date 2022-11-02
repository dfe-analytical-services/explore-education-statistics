import styles from '@admin/pages/publication//PublicationReleasesPage.module.scss';
import ScheduledReleaseRow from '@admin/pages/publication/components/ScheduledReleaseRow';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { ReleaseSummaryWithPermissions } from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import InsetText from '@common/components/InsetText';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

interface Props {
  publicationId: string;
  releases: ReleaseSummaryWithPermissions[];
}

const PublicationScheduledReleases = ({ publicationId, releases }: Props) => {
  const [
    showScheduledStatusGuidance,
    toggleScheduledStatusGuidance,
  ] = useToggle(false);
  const [
    showScheduledStagesGuidance,
    toggleScheduledStagesGuidance,
  ] = useToggle(false);

  if (releases.length === 0) {
    return <InsetText>You have no scheduled releases.</InsetText>;
  }

  return (
    <>
      <table data-testid="publication-scheduled-releases">
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th className={styles.statusColumn}>
              Status{' '}
              <ButtonText onClick={toggleScheduledStatusGuidance.on}>
                <InfoIcon description="Guidance on scheduled statuses" />
              </ButtonText>
            </th>
            <th className="govuk-!-width-one-quarter">
              Stages checklist{' '}
              <ButtonText onClick={toggleScheduledStagesGuidance.on}>
                <InfoIcon description="Guidance on publication stages" />
              </ButtonText>
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

      <ScheduledStagesGuidanceModal
        open={showScheduledStagesGuidance}
        onClose={toggleScheduledStagesGuidance.off}
      />
      <ScheduledStatusGuidanceModal
        open={showScheduledStatusGuidance}
        onClose={toggleScheduledStatusGuidance.off}
      />
    </>
  );
};

export default PublicationScheduledReleases;
