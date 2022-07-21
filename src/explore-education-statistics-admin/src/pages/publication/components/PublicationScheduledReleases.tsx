import styles from '@admin/pages/publication//PublicationReleasesPage.module.scss';
import ScheduledReleaseRow from '@admin/pages/publication/components/ScheduledReleaseRow';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { MyRelease } from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

interface Props {
  releases: MyRelease[];
}

const PublicationScheduledReleases = ({ releases }: Props) => {
  const [
    showScheduledStatusGuidance,
    toggleScheduledStatusGuidance,
  ] = useToggle(false);
  const [
    showScheduledStagesGuidance,
    toggleScheduledStagesGuidance,
  ] = useToggle(false);

  return (
    <>
      <table
        className="govuk-!-margin-bottom-9"
        data-testid="publication-scheduled-releases"
      >
        <caption className="govuk-table__caption--m">
          Scheduled releases
        </caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th className={styles.statusColumn}>
              State{' '}
              <ButtonText onClick={toggleScheduledStatusGuidance.on}>
                <InfoIcon description="Guidance on scheduled states" />
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
          {releases.map(release => {
            return <ScheduledReleaseRow key={release.id} release={release} />;
          })}
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
