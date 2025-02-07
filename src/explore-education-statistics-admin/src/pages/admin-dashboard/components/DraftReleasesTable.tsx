import DraftReleaseRow from '@admin/pages/admin-dashboard/components/DraftReleaseRow';
import styles from '@admin/pages/admin-dashboard/components/DraftReleasesTable.module.scss';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { DashboardReleaseVersionSummary } from '@admin/services/releaseVersionService';
import { Dictionary } from '@common/types';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';

interface PublicationRowProps {
  isBauUser: boolean;
  publication: string;
  releases: DashboardReleaseVersionSummary[];
  onChangeRelease: () => void;
}

const PublicationRow = ({
  isBauUser,
  publication,
  releases,
  onChangeRelease,
}: PublicationRowProps) => {
  return (
    <>
      <tr key={publication}>
        <th className="govuk-!-padding-top-6" colSpan={isBauUser ? 3 : 4}>
          {publication}
        </th>
      </tr>
      {releases.map(release => (
        <DraftReleaseRow
          key={release.id}
          isBauUser={isBauUser}
          release={release}
          onChangeRelease={onChangeRelease}
        />
      ))}
    </>
  );
};

interface DraftReleasesTableProps {
  isBauUser: boolean;
  releases: DashboardReleaseVersionSummary[];
  onChangeRelease: () => void;
}

const DraftReleasesTable = ({
  isBauUser,
  releases,
  onChangeRelease,
}: DraftReleasesTableProps) => {
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
        <p>There are currently no draft releases</p>
      ) : (
        <>
          {releasesByPublication &&
            Object.keys(releasesByPublication).length > 0 && (
              <table>
                <thead>
                  <tr>
                    <th
                      className={
                        isBauUser ? 'govuk-!-width-one-half' : undefined
                      }
                    >
                      Publication / Release period
                    </th>
                    <th className="dfe-white-space--nowrap">
                      Status <DraftStatusGuidanceModal />
                    </th>
                    {/* Don't render the issues for BAU users to prevent performance problems. */}
                    {!isBauUser && (
                      <th
                        className={`${styles.issuesColumn} dfe-white-space--nowrap`}
                      >
                        Issues <IssuesGuidanceModal />
                      </th>
                    )}
                    <th>Actions</th>
                  </tr>
                  {orderBy(Object.keys(releasesByPublication)).map(
                    publication => (
                      <PublicationRow
                        key={publication}
                        isBauUser={isBauUser}
                        publication={publication}
                        releases={releasesByPublication[publication]}
                        onChangeRelease={onChangeRelease}
                      />
                    ),
                  )}
                </thead>
              </table>
            )}
        </>
      )}
    </>
  );
};

export default DraftReleasesTable;
