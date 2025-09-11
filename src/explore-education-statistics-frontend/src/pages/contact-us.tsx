import InsetText from '@common/components/InsetText';
import Link from '@frontend/components/Link';
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
            <h2 className="govuk-heading-m">
              Need help using this service or have a question about the data?
            </h2>
            <p>
              If you have any specific statistical or subject-related queries,
              contact the team or named statistician listed in the 'Contact us'
              sections of the pages found under{' '}
              <Link to="/find-statistics">Find statistics and data</Link>.
            </p>
            <p>
              For any further queries, contact the Explore education statistics
              team:
            </p>
            <InsetText>
              Email:{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>
            </InsetText>

            <h2 className="govuk-heading-m">
              Have a general question about education or the Department for
              Education?
            </h2>
            <p>Call the DfE public enquiries line:</p>
            <InsetText>
              Phone: <a href="tel:03700002288">0370 000 2288</a>
              <br />
              Opening hours: Monday to Friday, 9.30am to 5pm (closed on bank
              holidays)
            </InsetText>
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
              <a
                href="https://form.education.gov.uk/en/AchieveForms/?form_uri=sandbox-publish://AF-Process-f1453496-7d8a-463f-9f33-1da2ac47ed76/AF-Stage-1e64d4cc-25fb-499a-a8d7-74e98203ac00/definition.json&redirectlink=%2Fen&cancelRedirectLink=%2Fen"
                rel="noopener noreferrer nofollow"
                target="_blank"
              >
                freedom of information (FOI) request (opens in new tab)
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
