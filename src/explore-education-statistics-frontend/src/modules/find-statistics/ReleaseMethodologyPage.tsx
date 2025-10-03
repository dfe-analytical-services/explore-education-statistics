import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import {
  PublicationMethodologiesList,
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import getUrlAttributes from '@common/utils/url/getUrlAttributes';
import Link from '@frontend/components/Link';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import publicationQueries from '@frontend/queries/publicationQueries';
import { QueryClient } from '@tanstack/react-query';
import { GetServerSideProps, NextPage } from 'next';
import React, { useState } from 'react';

interface Props {
  methodologiesSummary: PublicationMethodologiesList;
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

const ReleaseMethodologyPage: NextPage<Props> = ({
  methodologiesSummary,
  publicationSummary,
  releaseVersionSummary,
}) => {
  const { methodologies, externalMethodology } = methodologiesSummary;

  const hasMethodologies = methodologies.length > 0 || externalMethodology;

  const navItems = [
    hasMethodologies && { id: 'methodology-section', text: 'Methodology' },
    contactUsNavItem,
  ].filter(item => !!item);

  const [activeSection, setActiveSection] = useState(navItems[0].id);

  const externalMethodologyAttributes = getUrlAttributes(
    externalMethodology?.url || '',
  );

  const setActiveSectionIfValid = (sectionId: string) => {
    if (navItems.some(item => item.id === sectionId)) {
      setActiveSection(sectionId);
    }
  };

  return (
    <ReleasePageShell
      activePage="methodology"
      publicationSummary={publicationSummary}
      releaseVersionSummary={releaseVersionSummary}
    >
      <ReleasePageLayout
        activeSection={activeSection}
        navItems={navItems}
        onClickNavItem={setActiveSectionIfValid}
        onChangeSection={setActiveSectionIfValid}
      >
        {hasMethodologies && (
          <>
            <section id="methodology-section" data-page-section>
              <h2>Methodology</h2>
              <p>
                Find out how and why we collect, process and publish these
                statistics.
              </p>
              <ul
                className="govuk-list govuk-list--spaced"
                data-testid="methodologies-list"
              >
                {methodologiesSummary.methodologies.map(methodology => (
                  <li key={methodology.methodologyId}>
                    <Link to={`/methodology/${methodology.slug}`}>
                      {methodology.title}
                    </Link>
                  </li>
                ))}
                {externalMethodology && (
                  <li>
                    <Link
                      to={externalMethodology.url}
                      rel={`noopener noreferrer nofollow ${
                        !externalMethodologyAttributes?.isTrusted
                          ? 'external'
                          : ''
                      }`}
                      target="_blank"
                    >
                      {externalMethodology.title} (opens in new tab)
                    </Link>
                  </li>
                )}
              </ul>
            </section>
            <SectionBreak size="xl" />
          </>
        )}
        <ContactUsSection
          publicationContact={publicationSummary.contact}
          publicationTitle={publicationSummary.title}
        />
      </ReleasePageLayout>
    </ReleasePageShell>
  );
};

export const getServerSideProps: GetServerSideProps = withAxiosHandler(
  async ({ query }) => {
    const { publication: publicationSlug, release: releaseSlug } =
      query as Dictionary<string>;

    // TODO EES-6449 - remove
    if (process.env.APP_ENV === 'Production') {
      return {
        notFound: true,
      };
    }

    const queryClient = new QueryClient();

    try {
      const [publicationSummary, releaseVersionSummary, methodologiesSummary] =
        await Promise.all([
          queryClient.fetchQuery(
            publicationQueries.getPublicationSummaryRedesign(publicationSlug),
          ),
          queryClient.fetchQuery(
            publicationQueries.getReleaseVersionSummary(
              publicationSlug,
              releaseSlug,
            ),
          ),
          queryClient.fetchQuery(
            publicationQueries.getPublicationMethodologies(publicationSlug),
          ),
        ]);

      return {
        props: {
          publicationSummary,
          releaseVersionSummary,
          methodologiesSummary,
        },
      };
    } catch (error) {
      return {
        notFound: true,
      };
    }
  },
);

export default ReleaseMethodologyPage;
