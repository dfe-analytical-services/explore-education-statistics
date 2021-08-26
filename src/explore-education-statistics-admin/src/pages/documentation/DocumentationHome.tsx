import React from 'react';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';

const BrowseReleasesPage = () => {
  return (
    <Page
      wide
      breadcrumbs={[
        { name: "Administrator's guide", link: '/prototypes/documentation' },
      ]}
      title="Documentation"
    >
      <h1>Documentation</h1>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">Style guide</h2>

      <div className="govuk-grid-row govuk-!-margin-bottom-9">
        <div className="govuk-grid-column-one-third">
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/documentation/content-design-standards-guide">
              Content design standards guide
            </Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            How to create clear and consistent content to tell people a clear
            story so they can understand our statistics and data.
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/documentation/style-guide">Style guide</Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Browse our A to Z list of style, spelling and grammar conventions
            for all content published on the Explore education statistics
            service{' '}
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/documentation/glossary">A-Z Glossary</Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Browse our A to Z list of definitions for terms used across
            education statistics and data{' '}
          </p>
        </div>
      </div>

      <hr className="govuk-!-margin-top-9" />

      <h3 className="govuk-heading-m govuk-!-margin-top-9">
        Training and documentation
      </h3>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/using-dashboard">
          Using your administration dashboard
        </Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to use your administration dashboard to manage publications,
        releases and methodology.{' '}
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/manage-content">Managing content</Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to manage content within a release - including adding new and
        editing existing content.{' '}
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/manage-data">Managing data</Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to manage data for and within a release - including preparing data
        and adding data, other files and footnotes.{' '}
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/create-new-publication">
          Creating a new publication
        </Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to create a new publication - including adding a methodology and
        contact details.
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/create-new-release">
          Creating a new release
        </Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to create a new release - including uploading data and files and
        creating data blocks (i.e. tables and charts) and content.{' '}
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/edit-release">
          Editing a release and updating release status
        </Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to edit a release and update a release’s status - including
        approving a release for sign-off.{' '}
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/manage-data-block">
          Managing data blocks and creating tables and charts
        </Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to manage data blocks within a release - including creating tables
        and charts.{' '}
      </p>

      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
        <Link to="/documentation/configure-charts">Creating charts</Link>
      </h3>
      <p className="govuk-caption-m govuk-!-margin-top-1">
        How to create charts based on a saved data block.{' '}
      </p>
    </Page>
  );
};

export default BrowseReleasesPage;
