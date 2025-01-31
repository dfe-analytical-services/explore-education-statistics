import Page from '@frontend/components/Page';
import React from 'react';

function ContactPage() {
  return (
    <Page
      title="Contact Explore education statistics"
      breadcrumbLabel="Contact"
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <section>
            <h2 className="govuk-heading-m">General enquiries</h2>

            <p>
              The Explore education statistics service is operated by the
              Department for Education (DfE).
            </p>

            <p>
              If you need help and support using the service or have any general
              questions about education statistics and data contact:
            </p>

            <h3 className="govuk-heading-s">
              Explore education statistics team
            </h3>

            <p>
              Email:{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>
            </p>
          </section>

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Data and methodology enquiries</h2>
            <p>
              If you have a question about the data or methods used in a
              specific set of our statistics contact the team or statistician
              listed within the ‘Contact us’ section of the relevant page.
            </p>
          </section>

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Statistical policy enquiries</h2>
            <p>
              If you have a question about statistical policies such as about
              the Code of Practice, revisions and confidentiality contact:
            </p>
            <h3 className="govuk-heading-s">
              DfE Head of Profession for Statistics
            </h3>
            <p>
              Email:{' '}
              <a href="mailto:hop.statistics@education.gov.uk">
                hop.statistics@education.gov.uk
              </a>
            </p>
          </section>

          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              Freedom of information (FOI) requests
            </h2>
            <p>
              If you want access to data or statistics which are not available
              within this service submit a{' '}
              <a href="https://form.education.gov.uk/en/AchieveForms/?form_uri=sandbox-publish://AF-Process-f1453496-7d8a-463f-9f33-1da2ac47ed76/AF-Stage-1e64d4cc-25fb-499a-a8d7-74e98203ac00/definition.json&redirectlink=%2Fen&cancelRedirectLink=%2Fen">
                freedom of information (FOI) request
              </a>
              .
            </p>
          </section>
        </div>
      </div>
    </Page>
  );
}

export default ContactPage;
