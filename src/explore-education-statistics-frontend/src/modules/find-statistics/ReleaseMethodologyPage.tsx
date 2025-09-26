import SectionBreak from '@common/components/SectionBreak';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import publicationService, {
  PublicationMethodologiesList,
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import getUrlAttributes from '@common/utils/url/getUrlAttributes';
import Link from '@frontend/components/Link';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import ReleasePageTabNav from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

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

  const externalMethodologyAttributes = getUrlAttributes(
    externalMethodology?.url || '',
  );

  const hasMethodologies = methodologies.length > 0 || externalMethodology;

  return (
    <ReleasePageShell
      publicationSummary={publicationSummary}
      releaseVersionSummary={releaseVersionSummary}
    >
      <ReleasePageTabNav
        activePage="methodology"
        releaseUrlBase={`/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}`}
      />
      <ReleasePageLayout>
        {hasMethodologies && (
          <>
            <section id="methodology-section">
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
    const [publicationSummary, releaseVersionSummary, methodologiesSummary] =
      await Promise.all([
        publicationService.getPublicationSummaryRedesign(publicationSlug),
        publicationService.getReleaseVersionSummary(
          publicationSlug,
          releaseSlug,
        ),
        publicationService.getPublicationMethodologies(publicationSlug),
      ]);

    return {
      props: {
        publicationSummary,
        releaseVersionSummary,
        methodologiesSummary,
      },
    };
  },
);

export default ReleaseMethodologyPage;
