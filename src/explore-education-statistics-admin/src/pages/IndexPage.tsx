import React from 'react';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';

function IndexPage() {
  return (
    <Page>
      <h1>Index page for administrative application</h1>

      <h3>Dashboards</h3>
      <ul>
        <li>
          <Link to="/admin-dashboard">Administrators' dashboard page</Link>
        </li>
      </ul>

      <h3>Tools</h3>
      <ul>
        <li>
          <a href="/tools">Administrative tools</a>
        </li>
        <li>
          <a href="/tools/release/notify">Send a release notification</a>
        </li>
      </ul>

      <h3>Prototypes</h3>
      <ul>
        <li>
          <Link to="/prototypes/">
            Index page for administrative application (prototypes)
          </Link>
        </li>
      </ul>
    </Page>
  );
}

export default IndexPage;
