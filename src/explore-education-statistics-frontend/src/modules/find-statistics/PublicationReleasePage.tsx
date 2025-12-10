/* eslint-disable react/destructuring-assignment */
import { NavItem } from '@common/components/PageNavExpandable';
import getNavItemsFromContentSections from '@common/components/util/getNavItemsFromContentSections';
import { contactUsNavItem } from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import exploreDataPageSections from '@common/modules/release/data/releaseExploreDataPageSections';
import publicationService, {
  PublicationMethodologiesList,
  PublicationSummaryRedesign,
  RelatedInformationItem,
  ReleaseVersion,
  ReleaseVersionDataContent,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import { Dictionary } from '@common/types';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import { TabRouteItem } from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import PublicationReleasePageCurrent from '@frontend/modules/find-statistics/PublicationReleasePageCurrent';
import PublicationReleasePageHome from '@frontend/modules/find-statistics/PublicationReleasePageHome';
import ReleaseExploreDataPage from '@frontend/modules/find-statistics/ReleaseExploreDataPage';
import ReleaseHelpPage from '@frontend/modules/find-statistics/ReleaseHelpPage';
import ReleaseMethodologyPage from '@frontend/modules/find-statistics/ReleaseMethodologyPage';
import publicationQueries from '@frontend/queries/publicationQueries';
import { QueryClient } from '@tanstack/react-query';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

export const releasePageTabRouteItems = {
  home: {
    title: 'Release home',
    slug: '',
  },
  explore: {
    title: 'Explore and download data',
    slug: 'explore',
  },
  methodology: {
    title: 'Methodology',
    slug: 'methodology',
  },
  help: {
    title: 'Help and related information',
    slug: 'help',
  },
} as const satisfies TabRouteItem;

export type ReleasePageTabRouteItems = typeof releasePageTabRouteItems;
export type ReleasePageTabRouteKey = keyof ReleasePageTabRouteItems;

interface BaseReleaseProps {
  inPageNavItems: NavItem[];
  publicationSummary: PublicationSummaryRedesign;
  page: ReleasePageTabRouteKey;
  releaseVersionSummary: ReleaseVersionSummary;
}

interface HomeProps extends BaseReleaseProps {
  page: 'home';
  homeContent: ReleaseVersionHomeContent;
}

interface ExploreDataProps extends BaseReleaseProps {
  page: 'explore';
  dataContent: ReleaseVersionDataContent;
}

interface MethodologyProps extends BaseReleaseProps {
  page: 'methodology';
  methodologiesSummary: PublicationMethodologiesList;
}

interface HelpProps extends BaseReleaseProps {
  page: 'help';
  relatedInformationItems: RelatedInformationItem[];
}

interface CurrentReleaseProps {
  page: 'old';
  releaseVersion: ReleaseVersion;
}

type Props =
  | HomeProps
  | ExploreDataProps
  | MethodologyProps
  | HelpProps
  | CurrentReleaseProps;

const PublicationReleasePage: NextPage<Props> = props => {
  const { page } = props;

  return page === 'old' ? (
    <PublicationReleasePageCurrent releaseVersion={props.releaseVersion} />
  ) : (
    <ReleasePageShell
      activePage={page}
      inPageNavItems={props.inPageNavItems}
      publicationSummary={props.publicationSummary}
      releaseVersionSummary={props.releaseVersionSummary}
      tabNavItems={releasePageTabRouteItems}
    >
      {page === 'home' && (
        <PublicationReleasePageHome
          homeContent={props.homeContent}
          publicationSummary={props.publicationSummary}
          releaseVersionSummary={props.releaseVersionSummary}
        />
      )}
      {page === 'explore' && (
        <ReleaseExploreDataPage
          dataContent={props.dataContent}
          publicationSummary={props.publicationSummary}
          releaseVersionSummary={props.releaseVersionSummary}
        />
      )}
      {page === 'methodology' && (
        <ReleaseMethodologyPage
          publicationSummary={props.publicationSummary}
          methodologiesSummary={props.methodologiesSummary}
          releaseVersionSummary={props.releaseVersionSummary}
        />
      )}
      {page === 'help' && (
        <ReleaseHelpPage
          publicationSummary={props.publicationSummary}
          relatedInformationItems={props.relatedInformationItems}
          releaseVersionSummary={props.releaseVersionSummary}
        />
      )}
    </ReleasePageShell>
  );
};

export const getServerSideProps: GetServerSideProps = withAxiosHandler(
  async ({ query }) => {
    const {
      publication: publicationSlug,
      release: releaseSlug,
      tab,
      redesign,
    } = query as Dictionary<string>;

    const queryClient = new QueryClient();

    if ((redesign === 'true' || tab) && process.env.APP_ENV !== 'Production') {
      try {
        const publicationSummary = await queryClient.fetchQuery(
          publicationQueries.getPublicationSummaryRedesign(publicationSlug),
        );

        if (!releaseSlug) {
          return {
            redirect: {
              destination: `/find-statistics/${publicationSlug}/${publicationSummary.latestRelease.slug}?redesign=true`, // TODO EES-6449 remove redesign query param
              permanent: true,
            },
          };
        }

        const releaseVersionSummary = await queryClient.fetchQuery(
          publicationQueries.getReleaseVersionSummary(
            publicationSlug,
            releaseSlug,
          ),
        );

        const baseProps = {
          publicationSummary,
          releaseVersionSummary,
        };

        switch (tab) {
          // Home tab
          case undefined: {
            const homeContent = await queryClient.fetchQuery(
              publicationQueries.getReleaseVersionHomeContent(
                publicationSlug,
                releaseSlug,
              ),
            );
            const { summarySection, content } = homeContent;

            const hasSummarySection = summarySection.content.length > 0;

            return {
              props: {
                ...baseProps,
                page: 'home',
                homeContent,
                inPageNavItems: [
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
              },
            };
          }

          case 'explore': {
            const dataContent = await queryClient.fetchQuery(
              publicationQueries.getReleaseVersionDataContent(
                publicationSlug,
                releaseSlug,
              ),
            );

            const hasSupportingFiles = dataContent.supportingFiles.length;
            const hasFeaturedTables = dataContent.featuredTables.length;
            const hasDataDashboards =
              dataContent.dataDashboards && dataContent.dataDashboards.length;

            return {
              props: {
                ...baseProps,
                page: 'explore',
                dataContent,
                inPageNavItems: [
                  exploreDataPageSections.explore,
                  hasFeaturedTables && exploreDataPageSections.featuredTables,
                  exploreDataPageSections.datasets,
                  hasSupportingFiles && exploreDataPageSections.supportingFiles,
                  hasDataDashboards && exploreDataPageSections.dataDashboards,
                  exploreDataPageSections.dataGuidance,
                  contactUsNavItem,
                ].filter(item => !!item),
              },
            };
          }

          case 'methodology': {
            const methodologiesSummary = await queryClient.fetchQuery(
              publicationQueries.getPublicationMethodologies(publicationSlug),
            );

            const hasMethodologies =
              methodologiesSummary.methodologies.length > 0 ||
              !!methodologiesSummary.externalMethodology;

            return {
              props: {
                ...baseProps,
                page: 'methodology',
                methodologiesSummary,
                inPageNavItems: [
                  hasMethodologies && {
                    id: 'methodology-section',
                    text: 'Methodology',
                  },
                  contactUsNavItem,
                ].filter(item => !!item),
              },
            };
          }

          case 'help': {
            const relatedInformationItems = await queryClient.fetchQuery(
              publicationQueries.getReleaseVersionRelatedInformation(
                publicationSlug,
                releaseSlug,
              ),
            );
            const hasRelatedInformation = !!relatedInformationItems.length;
            const hasPraSummary = !!releaseVersionSummary.preReleaseAccessList;
            return {
              props: {
                ...baseProps,
                page: 'help',
                inPageNavItems: [
                  { ...contactUsNavItem, text: 'Get help by contacting us' },
                  {
                    id: 'release-type-section',
                    text: releaseTypes[releaseVersionSummary.type],
                  },
                  hasRelatedInformation && {
                    id: 'related-information-section',
                    text: 'Related information',
                  },
                  hasPraSummary && {
                    id: 'pre-release-access-list-section',
                    text: 'Pre-release access list',
                  },
                ].filter(item => !!item),
                relatedInformationItems,
              },
            };
          }

          default:
            return {
              notFound: true,
            };
        }
      } catch (error) {
        return {
          notFound: true,
        };
      }
    }

    // TODO EES-6449 remove the below - it is for the current release page
    const releaseVersion = await (releaseSlug
      ? publicationService.getPublicationRelease(publicationSlug, releaseSlug)
      : publicationService.getLatestPublicationRelease(publicationSlug));

    if (!releaseSlug) {
      return {
        redirect: {
          destination: `/find-statistics/${publicationSlug}/${releaseVersion.slug}`,
          permanent: true,
        },
      };
    }

    return {
      props: {
        page: 'old',
        releaseVersion,
      },
    };
  },
);

export default PublicationReleasePage;
