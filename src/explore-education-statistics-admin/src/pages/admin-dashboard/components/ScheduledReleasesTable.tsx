import ScheduledReleaseRow from '@admin/pages/publication/components/ScheduledReleaseRow';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { DashboardReleaseSummary } from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';

interface PublicationRowProps {
  publication: string;
  releases: DashboardReleaseSummary[];
}

const PublicationRow = ({ publication, releases }: PublicationRowProps) => {
  return (
    <>
      <tr key={publication}>
        <th className="govuk-!-padding-top-6" colSpan={5}>
          {publication}
        </th>
      </tr>
      {releases.map(release => (
        <ScheduledReleaseRow
          key={release.id}
          publicationId={release.publication.id}
          release={release}
        />
      ))}
    </>
  );
};

interface ScheduledReleasesTableProps {
  releases: DashboardReleaseSummary[];
}

const ScheduledReleasesTable = ({ releases }: ScheduledReleasesTableProps) => {
  const [showScheduledStatusGuidance, toggleScheduledStatusGuidance] =
    useToggle(false);
  const [showScheduledStagesGuidance, toggleScheduledStagesGuidance] =
    useToggle(false);

  const releasesByPublication: Dictionary<DashboardReleaseSummary[]> =
    useMemo(() => {
      return releases.reduce<Dictionary<DashboardReleaseSummary[]>>(
        (acc, release) => {
          if (acc[release.publication.title]) {
            acc[release.publication.title].push(release);
          } else {
            acc[release.publication.title] = [release];
          }

          return acc;
        },
        {},
      );
    }, [releases]);
  return (
    <>
      {releases.length === 0 ? (
        <p>There are currently no scheduled releases</p>
      ) : (
        <>
          {releasesByPublication &&
            Object.keys(releasesByPublication).length > 0 && (
              <table>
                <thead>
                  <tr>
                    <th>Publication / Release period</th>
                    <th>
                      Status{' '}
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
                    <th>Scheduled publish date</th>
                    <th>Actions</th>
                  </tr>
                  {orderBy(Object.keys(releasesByPublication)).map(
                    publication => (
                      <PublicationRow
                        key={publication}
                        publication={publication}
                        releases={releasesByPublication[publication]}
                      />
                    ),
                  )}
                </thead>
              </table>
            )}
        </>
      )}
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

export default ScheduledReleasesTable;
