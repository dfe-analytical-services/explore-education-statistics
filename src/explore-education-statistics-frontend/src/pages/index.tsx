import HomepageCard from '@frontend/components/HomepageCard';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import TryNewSearchBanner from '@frontend/components/TryNewSearchBanner';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

function HomePage() {
  const logLinkClick = (label: string) =>
    logEvent({
      category: 'Homepage',
      action: 'Homepage link clicked',
      label,
    });

  return (
    <Page
      title="Explore our statistics and data"
      isHomepage
      customBannerContent={<TryNewSearchBanner />}
    >
      <div className="govuk-grid-row dfe-card__container">
        <HomepageCard
          title="Find statistics and data"
          text="Find statistical summaries to help you understand and analyse our
              range of statistics."
          destination="/find-statistics"
          buttonText="Explore"
        />
        <HomepageCard
          title="Browse data catalogue"
          text="Browse all of the available open data and choose files to explore
          or download."
          destination="/data-catalogue"
          buttonText="Browse"
        />
        <HomepageCard
          title="Create your own tables"
          text="Explore our range of data and build your own tables from it."
          destination="/data-tables"
          buttonText="Create"
        />
      </div>

      <h2 className="govuk-!-margin-top-6">Supporting information</h2>

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h3 className="govuk-!-margin-bottom-1">
            <Link
              to="https://www.gov.uk/search/research-and-statistics?content_store_document_type=upcoming_statistics&organisations%5B%5D=department-for-education&order=updated-newest"
              onClick={() => logLinkClick('Statistics release calendar')}
            >
              Statistics release calendar
            </Link>
          </h3>
          <p className="govuk-caption-m">
            Browse our upcoming official statistics releases and their expected
            publication dates.
          </p>

          <h3 className="govuk-!-margin-bottom-1">
            <Link to="/methodology" onClick={() => logLinkClick('Methodology')}>
              Methodology
            </Link>
          </h3>
          <p className="govuk-caption-m">
            Browse to find out more about the methodology behind our statistics
            and how and why they&apos;re collected and published.
          </p>

          <h3 className="govuk-!-margin-bottom-1">
            <Link to="/glossary" onClick={() => logLinkClick('Glossary')}>
              Glossary
            </Link>
          </h3>
          <p className="govuk-caption-m">
            Browse our A to Z list of definitions for terms used across our
            statistics.
          </p>

          <h3 className="govuk-!-margin-bottom-1">
            <Link
              to="https://api.education.gov.uk/statistics/docs"
              onClick={() => logLinkClick('API documentation')}
            >
              API documentation
            </Link>
          </h3>
          <p className="govuk-caption-m">
            Find out how to automate access to statistics and data through our
            API (application programming interface).
          </p>

        </div>
      </div>

      <hr />

      <h2 className="govuk-!-margin-top-9">Related services</h2>

      <div className="govuk-grid-row govuk-!-margin-bottom-3">
        <div className="govuk-grid-column-two-thirds">
          <p>
            Use these services to find related information and other statistical
            services provided by the Department for Education (DfE):
          </p>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <a
              href="https://www.gov.uk/government/organisations/department-for-education/about/statistics"
              onClick={() => logLinkClick('Statistics at DfE')}
            >
              Statistics at DfE
            </a>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Find out more about latest news, announcements, forthcoming releases
            and ad hoc publications, as well as related education statistics.
          </p>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <a
              href="https://www.gov.uk/school-performance-tables"
              onClick={() =>
                logLinkClick('Compare school and college performance')
              }
            >
              Compare school and college performance
            </a>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Search for and check the performance of primary, secondary and
            special needs schools and colleges.
          </p>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <a
              href="https://www.get-information-schools.service.gov.uk/"
              onClick={() => logLinkClick('Get information about schools')}
            >
              Get information about schools
            </a>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Search to find and download information about schools, colleges,
            educational organisations and governors in England.
          </p>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <a
              href="https://financial-benchmarking-and-insights-tool.education.gov.uk/"
              onClick={() =>
                logLinkClick('Financial Benchmarking and Insights Tool')
              }
            >
              Financial Benchmarking and Insights Tool
            </a>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-1">
            Compare your school&apos;s expenditure with other schools in
            England.
          </p>
        </div>
      </div>

      <hr />

      <h2 className="govuk-!-margin-top-9">Contact us</h2>

      <p className="govuk-!-margin-top-1">
        The Explore education statistics service is operated by the Department
        for Education (DfE).
      </p>

      <p className="govuk-!-margin-top-1">
        If you need help and support or have a question about Explore education
        statistics contact:
      </p>

      <p className="govuk-!-margin-top-1">
        <strong>Explore education statistics team</strong>
      </p>

      <p className="govuk-caption-m govuk-!-margin-top-1">
        Email
        <br />
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
      </p>
    </Page>
  );
}

export default HomePage;
