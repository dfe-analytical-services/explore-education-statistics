import DashboardRelease from '@admin/components/DashboardRelease';
import Link from '@admin/components/Link';
import React from 'react';
import { Release } from '@admin/services/api/common/types/types';

export interface DashboardReleaseListProps {
  releases: Release[];
}

const DashboardReleaseList = ({ releases }: DashboardReleaseListProps) => (
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
                <DashboardRelease release={release} showComments />
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
