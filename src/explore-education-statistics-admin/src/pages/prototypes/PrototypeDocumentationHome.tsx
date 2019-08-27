import React from 'react';
import Link from '@admin/components/Link';
import PrototypePage from './components/PrototypePage';

const BrowseReleasesPage = () => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { text: 'Documentation', link: '/prototypes/documentation' },
      ]}
    >
      <h1>Documentation</h1>

      <h3 className="govuk-heading-m govuk-!-margin-top-9">Style guide</h3>

      <div className="govuk-grid-row govuk-!-margin-bottom-9">
        <div className="govuk-grid-column-one-third">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/prototypes/documentation/style-guide">Style guide</Link>
          </h4>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Browse our A to Z list of style, spelling and grammar conventions
            for all content published on the explore education statistics
            service{' '}
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/prototypes/documentation/glossary">A-Z Glossary</Link>
          </h4>
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
      <div className="govuk-grid-row govuk-!-margin-bottom-9">
        <div className="govuk-grid-column-one-third">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/prototypes/documentation/using-dashboard">
              Using your administration dashboard
            </Link>
          </h4>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            How to use your administration dashboard to manage publications,
            releases and methodology.{' '}
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/prototypes/documentation/create-new-publication">
              Creating a new publication
            </Link>
          </h4>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            How to create a new publication - including adding a methodology and
            contact details.{' '}
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="/prototypes/documentation/create-new-release">
              Creating a new release
            </Link>
          </h4>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            How to create a new release - including uploading data and files and
            creating data blocks (ie tables and charts) and content.{' '}
          </p>
        </div>
      </div>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
