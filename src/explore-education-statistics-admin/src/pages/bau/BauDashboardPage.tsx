import React from 'react';
import Page from '@admin/components/Page';
import Link from '@admin/components/Link';

function BauDashboardPage() {
  return (
    <Page wide>
      <h1 className="govuk-heading-xl">Platform administration</h1>

      <Link to="/administration/methodology">View methodology status</Link>
    </Page>
  );
}

export default BauDashboardPage;
