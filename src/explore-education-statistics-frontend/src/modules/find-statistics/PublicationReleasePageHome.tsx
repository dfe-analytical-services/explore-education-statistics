import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import ReleaseSummaryBlockMobile from '@common/modules/release/components/ReleaseSummaryBlockMobile';
import {
  PublicationSummaryRedesign,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React, { Fragment } from 'react';

interface Props {
  homeContent: ReleaseVersionHomeContent;
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

const PublicationReleasePage = ({
  homeContent,
  publicationSummary,
  releaseVersionSummary,
}: Props) => {
  const {
    content,
    keyStatistics,
    keyStatisticsSecondarySection,
    summarySection,
  } = homeContent;

  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <>
      {isMobileMedia && (
        <section id="publication-release-intro-mobile">
          <VisuallyHidden as="h2">Introduction</VisuallyHidden>

          <p className="govuk-body-l">{publicationSummary.summary}</p>

          <ReleaseSummaryBlockMobile
            lastUpdated={releaseVersionSummary.lastUpdated}
            releaseType={releaseVersionSummary.type}
            renderProducerLink={
              releaseVersionSummary.publishingOrganisations?.length ? (
                <span>
                  {releaseVersionSummary.publishingOrganisations.map(
                    (org, index) => (
                      <Fragment key={org.id}>
                        {index > 0 && ' and '}
                        <Link
                          unvisited
                          to={org.url}
                          className="govuk-link--no-underline"
                        >
                          {org.title}
                        </Link>
                      </Fragment>
                    ),
                  )}
                </span>
              ) : (
                <Link
                  unvisited
                  className="govuk-link--no-underline"
                  to="https://www.gov.uk/government/organisations/department-for-education"
                >
                  Department for Education
                </Link>
              )
            }
            renderSubscribeLink={
              <Link
                to={`/subscriptions/new-subscription/${publicationSummary.slug}`}
                unvisited
              >
                Get email alerts
              </Link>
            }
            renderUpdatesLink={
              releaseVersionSummary.updateCount > 1 ? (
                <Link
                  to={`/find-statistics/${publicationSummary.slug}/${publicationSummary.latestRelease.slug}/updates`}
                >
                  {releaseVersionSummary.updateCount} updates{' '}
                  <VisuallyHidden>
                    for `${releaseVersionSummary.title}`
                  </VisuallyHidden>
                </Link>
              ) : undefined
            }
            onShowReleaseTypeModal={() =>
              logEvent({
                category: `${publicationSummary.title} release page`,
                action: 'Release type clicked',
                label: window.location.pathname,
              })
            }
          />
        </section>
      )}

      <section id="headlines-section" data-page-section>
        <h2>Summary Section</h2>
        {summarySection.content.map(block => (
          <p key={block.id}>{block.type}</p>
        ))}
        <h2>Key Statistics</h2>
        {keyStatistics.map(ks => (
          <p key={ks.id}>{ks.type}</p>
        ))}
        <h2>Key Statistics Secondary</h2>
        {keyStatisticsSecondarySection.content.map(block => (
          <p key={block.id}>{block.type}</p>
        ))}
      </section>
      <SectionBreak size="xl" />
      {content.map(section => (
        <Fragment key={section.id}>
          <section id={section.id} data-page-section>
            <h2>{section.heading}</h2>
            {section.content.map(block => (
              <p key={block.id}>{block.type}</p>
            ))}
          </section>
          <SectionBreak size="xl" />
        </Fragment>
      ))}
      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </>
  );
};

export default PublicationReleasePage;
