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
import LazyLoad from 'react-lazyload';
import LoadingSpinner from '@common/components/LoadingSpinner';

export interface Props {
  publication: AdminDashboardPublication;
}

const PublicationSummary = ({ publication }: Props) => {
  return (
    <>
      <SummaryList>
        <SummaryListItem term="Methodology" smallKey>
          {publication.methodology && (
            <Link to={`/methodologies/${publication.methodology.id}`}>
              {publication.methodology.title}
            </Link>
          )}
          {publication.externalMethodology && (
            <a
              href={publication.externalMethodology.url}
              target="_blank"
              rel="noopener noreferrer"
            >
              {publication.externalMethodology.url}
            </a>
          )}
          {!publication.methodology && !publication.externalMethodology && (
            <>No methodology assigned</>
          )}
        </SummaryListItem>
        <SummaryListItem term="Releases" smallKey>
          <LazyLoad placeholder={<LoadingSpinner />} once>
            <ul className="govuk-list dfe-admin">
              {publication.releases.map(release => (
                <li key={release.id}>
                  <ReleaseSummary
                    release={release}
                    actions={
                      <ButtonLink
                        to={summaryRoute.generateLink(
                          publication.id,
                          release.id,
                        )}
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
          </LazyLoad>
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
              to={`/methodologies/${publication.methodology.id}`}
              className="govuk-button--secondary"
            >
              Edit methodology
            </ButtonLink>
          )}

          {!publication.methodology && (
            <ButtonLink
              to={`/methodologies/create?publicationId=${publication.id}`}
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
