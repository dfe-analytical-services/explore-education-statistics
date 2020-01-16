import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { getReleaseSummaryLabel } from '@admin/pages/release/util/releaseSummaryUtil';
import releaseRoutes, { summaryRoute } from '@admin/routes/edit-release/routes';
import { AdminDashboardPublication } from '@admin/services/dashboard/types';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { formatTestId } from '@common/util/test-utils';
import React from 'react';

export interface Props {
  publication: AdminDashboardPublication;
}

const PublicationSummary = ({ publication }: Props) => {
  return (
    <>
      <SummaryList>
        <SummaryListItem term="Methodology" smallKey>
          {publication.methodology && (
            <Link to={`/methodology/${publication.methodology.id}`}>
              {publication.methodology.title}
            </Link>
          )}
          {!publication.methodology && <>No methodology available</>}
        </SummaryListItem>
        <SummaryListItem term="Releases" smallKey>
          <ul className="govuk-list dfe-admin">
            {publication.releases.map(release => (
              <li key={release.id}>
                <ReleaseSummary
                  release={release}
                  actions={
                    <ButtonLink
                      to={summaryRoute.generateLink(publication.id, release.id)}
                      testId={formatTestId(
                        `Edit release link for ${
                          publication.title
                        }, ${getReleaseSummaryLabel(release)}`,
                      )}
                    >
                      {release.permissions.canUpdateRelease
                        ? 'Edit this release'
                        : 'View this release'}
                    </ButtonLink>
                  }
                />
              </li>
            ))}
            {publication.releases.length < 1 && <>No releases created</>}
          </ul>
        </SummaryListItem>
      </SummaryList>
      <SummaryList>
        <SummaryListItem term="" smallKey>
          {publication.permissions.canCreateReleases && (
            <ButtonLink
              to={releaseRoutes.createReleaseRoute.generateLink(publication.id)}
              className="govuk-!-margin-right-6"
              testId={`Create new release link for ${publication.title}`}
            >
              Create new release
            </ButtonLink>
          )}

          {publication.methodology && (
            <ButtonLink
              to="/prototypes/publication-assign-methodology"
              className="govuk-button--secondary"
            >
              Manage methodology
            </ButtonLink>
          )}

          {!publication.methodology && (
            <ButtonLink
              to="/prototypes/publication-assign-methodology"
              className="govuk-button--secondary"
            >
              Add methodology
            </ButtonLink>
          )}
        </SummaryListItem>
      </SummaryList>
    </>
  );
};

export default PublicationSummary;
