import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import DashboardReleaseSummary from '@admin/pages/admin-dashboard/components/DashboardReleaseSummary';
import releaseRoutes from '@admin/routes/edit-release/routes';
import { AdminDashboardPublication } from '@admin/services/dashboard/types';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

export interface Props {
  publication: AdminDashboardPublication;
}

const AdminDashboardPublicationSummary = ({ publication }: Props) => {
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
      </SummaryList>
      <SummaryList>
        <SummaryListItem term="Releases" smallKey>
          <ul className="govuk-list dfe-admin">
            {publication.releases.map(release => (
              <li key={release.id}>
                <DashboardReleaseSummary
                  publicationId={publication.id}
                  release={release}
                />
              </li>
            ))}
          </ul>
        </SummaryListItem>
      </SummaryList>
      <ButtonLink
        to={releaseRoutes.createReleaseRoute.generateLink(publication.id)}
        className="govuk-!-margin-right-6"
      >
        Create new release
      </ButtonLink>

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
    </>
  );
};

export default AdminDashboardPublicationSummary;
