import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleaseSummaryBlockMobile from '@common/modules/release/components/ReleaseSummaryBlockMobile';
import {
  PublicationSummaryRedesign,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import PublicationReleaseHeadlinesSection from '@frontend/modules/find-statistics/components/PublicationReleaseHeadlinesSectionRedesign';
import PublicationSectionBlocks from '@frontend/modules/find-statistics/components/PublicationSectionBlocks';
import glossaryService from '@frontend/services/glossaryService';
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
    headlinesSection,
    keyStatistics,
    keyStatisticsSecondarySection,
    summarySection,
  } = homeContent;

  const { isMedia: isMobileMedia } = useMobileMedia();

  // Update count includes 'first published' by default, but we only
  // want to show 'actual' update number.
  const updateCountExcludingFirstPublished =
    releaseVersionSummary.updateCount - 1;

  return (
    <>
      {isMobileMedia && (
        <section id="publication-release-intro-mobile">
          <VisuallyHidden as="h2">Introduction</VisuallyHidden>

          <p className="govuk-body-l">{publicationSummary.summary}</p>

          <ReleaseSummaryBlockMobile
            lastUpdated={releaseVersionSummary.lastUpdated}
            publishingOrganisations={
              releaseVersionSummary.publishingOrganisations
            }
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
                onClick={() => {
                  logEvent({
                    category: 'Subscribe',
                    action: 'Email subscription',
                  });
                }}
                unvisited
              >
                Get email alerts
              </Link>
            }
            renderUpdatesLink={
              updateCountExcludingFirstPublished > 0 ? (
                <Link
                  to={`/find-statistics/${publicationSummary.slug}/${publicationSummary.latestRelease.slug}/updates`}
                >
                  {updateCountExcludingFirstPublished} update
                  {updateCountExcludingFirstPublished === 1 ? '' : 's'}
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

      {summarySection.content?.length > 0 && (
        <ReleasePageContentSection
          heading="Background information"
          id="background-information"
        >
          {summarySection.content.map(block => (
            <ContentBlockRenderer
              key={block.id}
              block={block}
              getGlossaryEntry={glossaryService.getEntry}
              trackGlossaryLinks={glossaryEntrySlug =>
                logEvent({
                  category: `Publication Release Summary Glossary Link`,
                  action: `Glossary link clicked`,
                  label: glossaryEntrySlug,
                })
              }
            />
          ))}
        </ReleasePageContentSection>
      )}

      <PublicationReleaseHeadlinesSection
        headlinesSection={headlinesSection}
        keyStatistics={keyStatistics}
        keyStatisticsSecondarySection={keyStatisticsSecondarySection}
        releaseVersionId={releaseVersionSummary.id}
      />

      <div id="content" data-testid="home-content">
        {isMobileMedia ? (
          <Accordion
            className="govuk-!-margin-top-9"
            id="accordion-content"
            onSectionOpen={accordionSection => {
              logEvent({
                category: `${publicationSummary.title} release page`,
                action: `Content accordion opened`,
                label: `${accordionSection.title}`,
              });
            }}
          >
            {content.map(({ heading, id, content: sectionContent }) => {
              return (
                <AccordionSection heading={heading} key={id}>
                  {({ open }) => (
                    <PublicationSectionBlocks
                      blocks={sectionContent}
                      releaseVersionId={releaseVersionSummary.id}
                      visible={open}
                    />
                  )}
                </AccordionSection>
              );
            })}
          </Accordion>
        ) : (
          content.map(({ heading, id, content: sectionContent }) => (
            <ReleasePageContentSection
              heading={heading}
              key={id}
              id={generateIdFromHeading(heading)}
              testId="home-content-section"
            >
              <PublicationSectionBlocks
                blocks={sectionContent}
                releaseVersionId={releaseVersionSummary.id}
                visible
              />
            </ReleasePageContentSection>
          ))
        )}
      </div>

      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
        publishingOrganisations={releaseVersionSummary.publishingOrganisations}
      />
    </>
  );
};

export default PublicationReleasePage;
