import { useTenant } from '@common/contexts/TenantContext';
import FeedbackSection from '@common/components/FeedbackSection';
import InsetText from '@common/components/InsetText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import CookieBanner from '@frontend/components/CookieBanner';
import Link from '@frontend/components/Link';
import PageFeedback from '@frontend/components/PageFeedback';
import PageFooter from '@frontend/components/PageFooter';
import PageMeta from '@frontend/components/PageMeta';
import HomePageHeader from '@frontend/modules/home/components/HomePageHeader';
import HomePageMasthead from '@frontend/modules/home/components/HomePageMasthead';
import HomeReleaseLink from '@frontend/modules/home/components/HomeReleaseLink';
import {
  latestReleases,
  popularReleases,
} from '@frontend/modules/home/data/prototypeHomeReleasesData';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

const HomePageRedesign = () => {
  const {
    service: { nameMidSentence },
    organisation,
    contactEmail,
    relatedServices,
  } = useTenant();

  const logLinkClick = (label: string) =>
    logEvent({
      category: 'Homepage',
      action: 'Homepage link clicked',
      label,
    });

  // N.b not using the generic Page wrapper component as the 'masthead'
  // needs to go in <main> and be full width
  return (
    <>
      {/* eslint-disable-next-line jsx-a11y/anchor-has-content */}
      <a id="top" />
      <CookieBanner width="wide" />
      <PageMeta
        title="Explore our statistics and data"
        additionalMeta={[
          {
            type: 'meta',
            attributes: { name: 'robots', content: 'noindex,nofollow' },
          },
        ]}
      />
      <HomePageHeader />
      <main
        className="govuk-main-wrapper govuk-!-padding-top-0 app-main-class"
        id="main-content"
        role="main"
      >
        <HomePageMasthead />

        <div className="govuk-width-container dfe-width-container--wide">
          <section className="govuk-!-padding-bottom-9">
            <h2 className="govuk-!-margin-top-9">
              Latest statistical releases
            </h2>

            <ol className="govuk-grid-row govuk-list">
              {latestReleases.map(release => (
                <li
                  className="govuk-grid-column-one-third"
                  key={`${release.publicationSlug}-${release.releaseSlug}`}
                >
                  <HomeReleaseLink release={release} onClick={logLinkClick} />
                </li>
              ))}
            </ol>
            <Link
              to="/find-statistics"
              onClick={() => logLinkClick('Find statistics')}
            >
              Explore all of our statistical releases
            </Link>

            <h2 className="govuk-!-margin-top-9">
              Most popular statistical releases
            </h2>

            <ol className="govuk-grid-row govuk-list">
              {popularReleases.map(release => (
                <li
                  className="govuk-grid-column-one-third"
                  key={`${release.publicationSlug}-${release.releaseSlug}`}
                >
                  <HomeReleaseLink release={release} onClick={logLinkClick} />
                </li>
              ))}
            </ol>
            <Link
              to="/find-statistics"
              onClick={() => logLinkClick('Find statistics')}
            >
              Explore all of our statistical releases
            </Link>
          </section>

          <hr />

          <h2 className="govuk-!-margin-top-9">Supporting information</h2>

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <h3 className="govuk-heading-s govuk-!-margin-bottom-1">
                <Link
                  to="https://www.gov.uk/search/research-and-statistics?content_store_document_type=upcoming_statistics&organisations%5B%5D=department-for-education&order=updated-newest"
                  rel="noopener noreferrer nofollow"
                  target="_blank"
                  onClick={() => logLinkClick('Statistics release calendar')}
                >
                  Statistics release calendar{' '}
                  <VisuallyHidden>(opens in new tab)</VisuallyHidden>
                </Link>
              </h3>
              <p className="govuk-caption-m">
                Browse our upcoming official statistics releases and their
                expected publication dates.
              </p>

              <h3 className="govuk-heading-s govuk-!-margin-bottom-1">
                <Link
                  to="/methodology"
                  onClick={() => logLinkClick('Methodology')}
                >
                  Methodology
                </Link>
              </h3>
              <p className="govuk-caption-m">
                Browse to find out more about the methodology behind our
                statistics and how and why they&apos;re collected and published.
              </p>

              <h3 className="govuk-heading-s govuk-!-margin-bottom-1">
                <Link to="/glossary" onClick={() => logLinkClick('Glossary')}>
                  Glossary
                </Link>
              </h3>
              <p className="govuk-caption-m">
                Browse our A to Z list of definitions for terms used across our
                statistics.
              </p>

              <h3 className="govuk-heading-s govuk-!-margin-bottom-1">
                <Link
                  to="https://api.education.gov.uk/statistics/docs"
                  rel="noopener noreferrer nofollow"
                  target="_blank"
                  onClick={() => logLinkClick('API documentation')}
                >
                  API documentation{' '}
                  <VisuallyHidden>(opens in new tab)</VisuallyHidden>
                </Link>
              </h3>
              <p className="govuk-caption-m">
                Find out how to automate access to statistics and data through
                our API (application programming interface).
              </p>
            </div>
          </div>

          <hr />

          <h2 className="govuk-!-margin-top-9">
            Contact us for help and support
          </h2>
          <div className="govuk-grid-row govuk-!-margin-bottom-3">
            <div className="govuk-grid-column-two-thirds">
              <p className="govuk-!-margin-top-1">
                The {nameMidSentence} service is operated by{' '}
                {organisation.nameWithArticle}
                {organisation.acronym && ` (${organisation.acronym})`}.
              </p>

              <h3>
                Need help using this service or have a question about the data?
              </h3>
              <p>
                If you have any specific statistical or subject-related queries,
                contact the team or named statistician listed in the 'Contact
                us' sections of the pages found under{' '}
                <Link to="/find-statistics">Find statistics and data</Link>.
              </p>
              <p>
                For any further queries, contact the {nameMidSentence} team:
              </p>
              <InsetText>
                Email: <a href={`mailto:${contactEmail}`}>{contactEmail}</a>
              </InsetText>

              {organisation.publicEnquiries && (
                <>
                  <h3>Have a general enquiry?</h3>
                  <p>{organisation.publicEnquiries.description}</p>
                  <p>{organisation.publicEnquiries.telephoneCallToAction}</p>
                  <InsetText>
                    {organisation.publicEnquiries.telephone}
                    <br />
                    Opening hours: {organisation.publicEnquiries.openingHours}
                  </InsetText>
                </>
              )}

              <h3 className="govuk-heading-m">Provide us with feedback</h3>
              <FeedbackSection />
            </div>
          </div>

          {relatedServices && relatedServices.length > 0 && (
            <>
              <hr />

              <h2 className="govuk-!-margin-top-9">
                Other services provided by {organisation.nameWithArticle}
                {organisation.acronym && ` (${organisation.acronym})`}
              </h2>
              <div className="govuk-grid-row govuk-!-margin-bottom-3">
                <div className="govuk-grid-column-two-thirds">
                  {relatedServices.map(relatedService => (
                    <div key={relatedService.url}>
                      <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                        <a
                          href={relatedService.url}
                          rel="noopener noreferrer nofollow"
                          target="_blank"
                          onClick={() => logLinkClick(relatedService.title)}
                        >
                          {relatedService.title}{' '}
                          <VisuallyHidden>(opens in new tab)</VisuallyHidden>
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        {relatedService.description}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            </>
          )}
        </div>
      </main>
      <PageFeedback width="wide" />
      <PageFooter width="wide" />
    </>
  );
};

export default HomePageRedesign;
