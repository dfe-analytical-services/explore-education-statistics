import DashboardReleaseSummary from '@admin/pages/admin-dashboard/components/DashboardReleaseSummary';
import { AdminDashboardPublication } from '@admin/services/api/dashboard/types';
import React from 'react';
import Link from '@admin/components/Link';

export interface Props {
  publication: AdminDashboardPublication;
}

const AdminDashboardPublicationSummary = ({ publication }: Props) => {
  return (
    <>
      <dl className="govuk-summary-list govuk-!-margin-bottom-0">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key  dfe-summary-list__key--small">
            Methodology
          </dt>
          <dd className="govuk-summary-list__value">
            <Link to={`/methodology/${publication.methodology.id}`}>
              {publication.methodology.label}
            </Link>
          </dd>
          <dd className="govuk-summary-list__actions">
            <Link to="/prototypes/publication-assign-methodology">
              Edit methodology
            </Link>
          </dd>
        </div>
      </dl>
      <dl className="govuk-summary-list">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key dfe-summary-list__key--small">
            Releases
          </dt>
          <dd className="govuk-summary-list__value">
            <ul className="govuk-list dfe-admin">
              {publication.releases.map(release => (
                <li key={release.id}>
                  <DashboardReleaseSummary release={release} />
                </li>
              ))}
            </ul>
          </dd>
        </div>
      </dl>
      <Link to="/prototypes/release-create-new" className="govuk-button">
        Create a new release
      </Link>
    </>
  );
};

export default AdminDashboardPublicationSummary;
