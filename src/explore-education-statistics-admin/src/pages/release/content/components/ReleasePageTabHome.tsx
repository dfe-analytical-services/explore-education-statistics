import Link from '@admin/components/Link';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseHeadlinesRedesign from '@admin/pages/release/content/components/ReleaseHeadlinesRedesign';
import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import getNavItemsFromHtml from '@common/components/util/getNavItemsFromHtml';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import ReleaseSummaryBlockMobile from '@common/modules/release/components/ReleaseSummaryBlockMobile';
import React, { Fragment, useMemo } from 'react';

interface Props {
  hidden: boolean;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}
const ReleasePageTabHome = ({ hidden, transformFeaturedTableLinks }: Props) => {
  const { release } = useReleaseContentState();
  const {
    content,
    publication,
    publishingOrganisations,
    summarySection,
    type,
    updates,
  } = release;

  const { isMedia: isMobileMedia } = useMobileMedia();

  const hasSummarySection = summarySection.content.length > 0;

  // Get nav items for section headings (h2) and parse the section's
  // HtmlBlocks for h3 headings to generate subNavItems
  const contentSectionsItems = useMemo(
    () =>
      content.map(section => {
        const { heading, content: sectionContent } = section;
        const subNavItems = sectionContent
          .flatMap(block => {
            if (block.type === 'HtmlBlock') {
              return getNavItemsFromHtml({
                html: block.body,
                blockId: block.id,
              });
            }
            return null;
          })
          .filter(item => !!item);

        return {
          id: generateIdFromHeading(heading),
          text: heading,
          subNavItems,
        };
      }),
    [content],
  );

  const navItems = useMemo(
    () =>
      [
        hasSummarySection && {
          id: 'background-information',
          text: 'Background information',
        },
        {
          id: 'headlines-section',
          text: 'Headline facts and figures',
        },
        ...contentSectionsItems,
        contactUsNavItem,
      ].filter(item => !!item),
    [contentSectionsItems, hasSummarySection],
  );

  return (
    <ReleasePageTabPanel tabKey="home" hidden={hidden}>
      <ReleasePageLayout navItems={navItems}>
        {isMobileMedia && (
          <section id="publication-release-intro-mobile">
            <VisuallyHidden as="h2">Introduction</VisuallyHidden>

            <p className="govuk-body-l">{publication.summary}</p>

            <ReleaseSummaryBlockMobile
              lastUpdated={updates[0]?.on}
              releaseType={type}
              renderProducerLink={
                publishingOrganisations?.length ? (
                  <span>
                    {publishingOrganisations.map((org, index) => (
                      <Fragment key={org.id}>
                        {index > 0 && ' and '}
                        <Link unvisited to={org.url}>
                          {org.title}
                        </Link>
                      </Fragment>
                    ))}
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
              renderUpdatesLink={
                updates.length > 1 ? (
                  <Link to="#">
                    {updates.length} updates{' '}
                    <VisuallyHidden>for {release.title}</VisuallyHidden>
                  </Link>
                ) : undefined
              }
              renderSubscribeLink={
                <Link to="#" unvisited>
                  Get email alerts
                </Link>
              }
            />
          </section>
        )}

        {hasSummarySection && (
          <ReleasePageContentSection
            heading="Background information"
            id="background-information"
          >
            {summarySection.content.map(block => (
              <ReleaseBlock
                key={block.id}
                block={block}
                releaseVersionId={release.id}
                transformFeaturedTableLinks={transformFeaturedTableLinks}
              />
            ))}
          </ReleasePageContentSection>
        )}

        <ReleaseHeadlinesRedesign
          release={release}
          transformFeaturedTableLinks={transformFeaturedTableLinks}
        />

        <div id="content" data-testid="home-content">
          {isMobileMedia ? (
            <Accordion className="govuk-!-margin-top-9" id="accordion-content">
              {content.map(({ heading, id, content: sectionContent }) => {
                return (
                  <AccordionSection heading={heading} key={id}>
                    {({ open }) =>
                      sectionContent.map(block => (
                        <ReleaseBlock
                          key={block.id}
                          block={block}
                          releaseVersionId={release.id}
                          transformFeaturedTableLinks={
                            transformFeaturedTableLinks
                          }
                          visible={open}
                        />
                      ))
                    }
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
                {sectionContent.map(block => (
                  <ReleaseBlock
                    key={block.id}
                    block={block}
                    releaseVersionId={release.id}
                    transformFeaturedTableLinks={transformFeaturedTableLinks}
                  />
                ))}
              </ReleasePageContentSection>
            ))
          )}
        </div>

        <ContactUsSection
          publicationContact={publication.contact}
          publicationTitle={publication.title}
        />
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabHome;
