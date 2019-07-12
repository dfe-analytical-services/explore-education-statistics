import React from 'react';
import Link from '@admin/components/Link';
import { AdminDashboardRelease } from '@admin/services/api/dashboard/types';
import DashboardReleaseSummary from './DashboardReleaseSummary';

export interface Props {
  releases: AdminDashboardRelease[];
}

const DashboardReleaseList = ({ releases }: Props) => (
  <>
    <dl className="govuk-summary-list">
      <div className="govuk-summary-list__row">
        <dt className="govuk-summary-list__key dfe-summary-list__key--small">
          Releases
        </dt>
        <dd className="govuk-summary-list__value">
          <ul className="govuk-list dfe-admin">
            {releases.map(release => (
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

export default DashboardReleaseList;
