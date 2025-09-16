import InsetText from '@common/components/InsetText';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import React from 'react';

function HelpSupportPage() {
  return (
    <Page title="Help and support">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p>
            Find out more about Explore education statistics - including how to
            use it services and get further help and support.
          </p>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">About the service</h2>
            <p>
              Explore education statistics has been built by{' '}
              <a
                href="https://www.gov.uk/government/organisations/department-for-education"
                rel="noopener noreferrer nofollow"
                target="_blank"
              >
                the Department for Education (DfE) (opens in new tab)
              </a>{' '}
              to make it easier for you to find, access, navigate and understand
              the range of education-related statistics and data it provides.
            </p>
            <p>
              All the statistics and data published this service are produced in
              line with the UK Statistical Authority's{' '}
              <a
                href="https://www.statisticsauthority.gov.uk/code-of-practice/"
                rel="noopener noreferrer nofollow"
                target="_blank"
              >
                Code of Practice for Official Statistics (opens in new tab)
              </a>{' '}
              and DfE's{' '}
              <a
                href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education"
                rel="noopener noreferrer nofollow"
                target="_blank"
              >
                Standards for official statistics (opens in new tab)
              </a>
              .
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Find statistics and data</h2>
            The service contains all of DfE's published official statistics.
            <p>
              To find specific sets of statistics currently published via the
              service, browse our{' '}
              <Link to="/find-statistics">Find statistics and data</Link>{' '}
              section.
            </p>
            <p>
              To find out about the methodology behind the specific statistics
              and data set currently published via the service and how and why
              they're collected and published browse our{' '}
              <Link to="/methodology">Education statistics: methodology</Link>{' '}
              section.
            </p>
            <p>
              For a list of definitions and terms used across education
              statistics and data visit our{' '}
              <Link to="/glossary">Education statistics: glossary</Link>{' '}
              section.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">
              Creating and downloading data tables
            </h2>
            <p>
              To create your own tables and explore the data we have available
              via the service use the table tool available in our{' '}
              <Link to="/data-tables">Create your own tables</Link> section.
            </p>
            <p>
              You can use our table tool to choose the data and area of
              statistical interest you want to explore and then use filters to
              create your table.
            </p>
            <p>
              Once you've created your table, you can download the data it
              contains for your own offline analysis, or share a permanent
              webpage of the created table.
            </p>
            <p>
              You can also download full data files of those statistics
              currently published via the service through our{' '}
              <Link to="/data-catalogue">Data catalogue</Link>.
            </p>
            <p>
              These files are currently only available in CSV format but other
              formats will be made available in the future.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Sign up for email alerts</h2>
            <p>
              You can sign up to receive emails when new statistics and data are
              published through our service.
            </p>
            <p>
              Sign up by selecting the 'Sign up for email alerts' link found at
              the top of the pages found under{' '}
              <Link to="/find-statistics">Find statistics and data</Link>.
            </p>
            <p>
              You'll then be sent an email alert with a link to the latest
              statistical and data release which you have chosen to be updated
              about.
            </p>
            <p>
              You can unsubscribe at any time by selecting the ‘unsubscribe’
              links in the email alerts you receive or in the original
              confirmation email which you were sent.
            </p>
          </section>
          <section className="govuk-section-break--xl">
            <h2 className="govuk-heading-m">Contact us for help and support</h2>
            <p className="govuk-!-margin-top-1">
              The Explore education statistics service is operated by the
              Department for Education (DfE).
            </p>

            <h3 className="govuk-!-margin-top-8 govuk-heading-s">
              Need help using this service or have a question about the data?
            </h3>
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

            <h3 className="govuk-!-margin-top-8 govuk-heading-s">
              Have a general question about education or the Department for
              Education?
            </h3>
            <p>Call the DfE public enquiries line:</p>
            <InsetText>
              Phone: 0370 000 2288
              <br />
              Opening hours: Monday to Friday, 9.30am to 5pm (closed on bank
              holidays)
            </InsetText>
          </section>
        </div>
      </div>
    </Page>
  );
}

export default HelpSupportPage;
