import DraftReleaseRow from '@admin/pages/admin-dashboard/components/DraftReleaseRow';
import styles from '@admin/pages/admin-dashboard/components/DraftReleasesTable.module.scss';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { DashboardReleaseVersionSummary } from '@admin/services/releaseVersionService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Pagination from '@common/components/Pagination';
import VisuallyHidden from '@common/components/VisuallyHidden';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';

const releasesPerPage = 25;

interface PublicationGroup {
  id: string;
  releases: DashboardReleaseVersionSummary[];
}

interface DraftReleasesTableProps {
  showIssues: boolean;
  showMethodology: boolean;
  releases: DashboardReleaseVersionSummary[];
  onChangeRelease: () => void;
}

const DraftReleasesTable = ({
  showIssues,
  showMethodology,
  releases,
  onChangeRelease,
}: DraftReleasesTableProps) => {
  const [currentPage, setCurrentPage] = useState(1);

  const orderedReleases = useMemo(
    () =>
      orderBy(
        releases,
        [
          release => release.publication.title.toLowerCase(),
          release => release.publication.id,
          release => release.year,
          release => release.title.toLowerCase(),
          release => release.id,
        ],
        ['asc', 'asc', 'desc', 'asc', 'asc'],
      ),
    [releases],
  );

  const totalPages = Math.ceil(orderedReleases.length / releasesPerPage);
  const page = Math.min(currentPage, totalPages);
  const pageReleases = orderedReleases.slice(
    (page - 1) * releasesPerPage,
    page * releasesPerPage,
  );

  const publicationGroups = useMemo(() => {
    return pageReleases.reduce<PublicationGroup[]>((groups, release) => {
      const currentGroup = groups[groups.length - 1];

      if (currentGroup?.id === release.publication.id) {
        currentGroup.releases.push(release);
      } else {
        groups.push({
          id: release.publication.id,
          releases: [release],
        });
      }

      return groups;
    }, []);
  }, [pageReleases]);

  return (
    <>
      <VisuallyHidden>
        <p aria-atomic aria-live="polite">
          {orderedReleases.length === 0
            ? '0 results'
            : `${orderedReleases.length} results, showing page ${page} of ${totalPages}`}
        </p>
      </VisuallyHidden>
      {releases.length === 0 ? (
        <p>There are currently no draft releases</p>
      ) : (
        <>
          <div className="table-container">
            <table>
              <caption className="govuk-visually-hidden">
                Draft releases grouped by publication
              </caption>
              <thead>
                <tr>
                  <th scope="col">Publication title</th>
                  <th scope="col">Release</th>
                  <th scope="col" className="dfe-white-space--nowrap">
                    Draft status
                  </th>
                  {showIssues && (
                    <th
                      scope="col"
                      className={`${styles.issuesColumn} dfe-white-space--nowrap`}
                    >
                      Issues
                    </th>
                  )}
                  {showMethodology && (
                    <th scope="col" className="dfe-white-space--nowrap">
                      Methodology status
                    </th>
                  )}
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              {publicationGroups.map(group => (
                <tbody key={group.id}>
                  {group.releases.map((release, index) => (
                    <DraftReleaseRow
                      key={release.id}
                      publicationRowSpan={
                        index === 0 ? group.releases.length : undefined
                      }
                      release={release}
                      showIssues={showIssues}
                      showMethodology={showMethodology}
                      onChangeRelease={onChangeRelease}
                    />
                  ))}
                </tbody>
              ))}
            </table>
          </div>
          <Pagination
            currentPage={page}
            totalPages={totalPages}
            renderLink={({
              'aria-current': ariaCurrent,
              'aria-label': ariaLabel,
              'data-testid': testId,
              children,
              className,
              onClick,
            }) => (
              <ButtonText
                ariaCurrent={ariaCurrent}
                ariaLabel={ariaLabel}
                className={`govuk-link ${className}`}
                testId={testId}
                onClick={onClick}
              >
                {children}
              </ButtonText>
            )}
            onClick={setCurrentPage}
          />
          <ButtonGroup>
            {showIssues && <IssuesGuidanceModal />}
            <DraftStatusGuidanceModal />
          </ButtonGroup>
        </>
      )}
    </>
  );
};

export default DraftReleasesTable;
