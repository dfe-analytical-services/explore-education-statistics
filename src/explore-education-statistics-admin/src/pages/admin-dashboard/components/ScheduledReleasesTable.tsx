import ScheduledReleaseRow from '@admin/pages/publication/components/ScheduledReleaseRow';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { DashboardReleaseVersionSummary } from '@admin/services/releaseVersionService';
import { Dictionary } from '@common/types';
import ButtonGroup from '@common/components/ButtonGroup';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';

interface PublicationRowProps {
  publication: string;
  releases: DashboardReleaseVersionSummary[];
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
  releases: DashboardReleaseVersionSummary[];
}

const ScheduledReleasesTable = ({ releases }: ScheduledReleasesTableProps) => {
  const releasesByPublication: Dictionary<DashboardReleaseVersionSummary[]> =
    useMemo(() => {
      return releases.reduce<Dictionary<DashboardReleaseVersionSummary[]>>(
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
              <>
                <table>
                  <thead>
                    <tr>
                      <th>Publication / Release period</th>
                      <th className="dfe-white-space--nowrap">Status</th>
                      <th className="govuk-!-width-one-quarter dfe-white-space--nowrap">
                        Stages checklist
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
                <ButtonGroup>
                  <ScheduledStatusGuidanceModal />
                  <ScheduledStagesGuidanceModal />
                </ButtonGroup>
              </>
            )}
        </>
      )}
    </>
  );
};

export default ScheduledReleasesTable;
