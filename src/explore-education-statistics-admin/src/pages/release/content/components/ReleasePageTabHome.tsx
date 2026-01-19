import Link from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseHeadlinesRedesign from '@admin/pages/release/content/components/ReleaseHeadlinesRedesign';
import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import getNavItemsFromContentSections from '@common/components/util/getNavItemsFromContentSections';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { useMobileMedia } from '@common/hooks/useMedia';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import ReleaseSummaryBlockMobile from '@common/modules/release/components/ReleaseSummaryBlockMobile';
import React, { Fragment, useEffect, useMemo } from 'react';

interface Props {
  hidden: boolean;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}
const ReleasePageTabHome = ({ hidden, transformFeaturedTableLinks }: Props) => {
  const { setActiveSection } = useEditingContext();

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

  const [handleScroll] = useDebouncedCallback(() => {
    const sections = document.querySelectorAll('[data-scroll]');

    // Set a section as active when it's in the top third of the page.
    const buffer = window.innerHeight / 3;
    const scrollPosition = window.scrollY + buffer;

    sections.forEach(section => {
      if (section) {
        const { height } = section.getBoundingClientRect();
        const { offsetTop } = section as HTMLElement;
        const offsetBottom = offsetTop + height;

        if (
          scrollPosition > offsetTop &&
          scrollPosition < offsetBottom &&
          section instanceof HTMLElement &&
          section.dataset.scroll
        ) {
          setActiveSection(section.dataset.scroll);
        }
      }
    });
  }, 100);

  useEffect(() => {
    window.addEventListener('scroll', handleScroll);

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll]);

  const hasSummarySection = summarySection.content.length > 0;

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
        ...getNavItemsFromContentSections(content),
        contactUsNavItem,
      ].filter(item => !!item),
    [content, hasSummarySection],
  );

  // On mobile we want to show published date if there are no updates,
  // otherwise, we want to show updated at and a (dummy) link to updates
  const showUpdatesInfo = updates.length > 0;

  return (
    <ReleasePageTabPanel tabKey="home" hidden={hidden}>
      <ReleasePageLayout navItems={navItems}>
        {isMobileMedia && (
          <section
            id="publication-release-intro-mobile"
            data-scroll="summary-section"
          >
            <VisuallyHidden as="h2">Introduction</VisuallyHidden>

            <p className="govuk-body-l">{publication.summary}</p>

            <ReleaseSummaryBlockMobile
              lastUpdated={showUpdatesInfo ? updates[0].on : undefined}
              publishingOrganisations={publishingOrganisations}
              releaseDate={
                !showUpdatesInfo
                  ? release.published ?? release.publishScheduled
                  : undefined
              }
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
                showUpdatesInfo ? (
                  <span data-testid="summary-mobile-updates-link">
                    {updates.length} update{updates.length === 1 ? '' : 's'}
                    <VisuallyHidden>for {release.title}</VisuallyHidden>
                  </span>
                ) : undefined
              }
              renderSubscribeLink={<span>Get email alerts</span>}
            />
          </section>
        )}

        {hasSummarySection && (
          <ReleasePageContentSection
            heading="Background information"
            id="background-information"
          >
            {summarySection.content.map(block => (
              <div
                key={block.id}
                data-scroll={`editableSectionBlocks-${block.id}`}
              >
                <ReleaseBlock
                  block={block}
                  releaseVersionId={release.id}
                  transformFeaturedTableLinks={transformFeaturedTableLinks}
                  visible
                />
              </div>
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
                dataScrollId={`releaseMainContent-${id}`}
              >
                {sectionContent.map(block => (
                  <div
                    key={block.id}
                    data-scroll={`editableSectionBlocks-${block.id}`}
                  >
                    <ReleaseBlock
                      block={block}
                      releaseVersionId={release.id}
                      transformFeaturedTableLinks={transformFeaturedTableLinks}
                      visible
                    />
                  </div>
                ))}
              </ReleasePageContentSection>
            ))
          )}
        </div>

        <ContactUsSection
          publicationContact={publication.contact}
          publicationTitle={publication.title}
          publishingOrganisations={publishingOrganisations}
        />
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabHome;
