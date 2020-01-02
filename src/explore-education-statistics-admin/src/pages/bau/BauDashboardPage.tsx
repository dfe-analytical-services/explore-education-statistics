import React from 'react';
import Page from '@admin/components/Page';
import Link from '@admin/components/Link';

function BauDashboardPage() {
  return (
    <Page wide>
      <h1 className="govuk-heading-xl">Platform administration</h1>
      <ul>
        <li>
          <Link to="/administration/methodology">View methodology status</Link>
        </li>
        <li>
          <Link to="/administration/users">View service users</Link>
        </li>
      </ul>
    </Page>
  );
}

export default BauDashboardPage;
