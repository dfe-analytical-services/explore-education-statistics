/* eslint-disable react/destructuring-assignment */
import { NavItem } from '@common/components/PageNavExpandable';
import { contactUsNavItem } from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import publicationService, {
  PublicationMethodologiesList,
  PublicationSummaryRedesign,
  ReleaseVersion,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import ReleasePageShell from '@frontend/modules/find-statistics/components/ReleasePageShell';
import { TabRouteItem } from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import PublicationReleasePageCurrent from '@frontend/modules/find-statistics/PublicationReleasePageCurrent';
import PublicationReleasePageHome from '@frontend/modules/find-statistics/PublicationReleasePageHome';
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
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
  page: ReleasePageTabRouteKey;
}

interface HomeProps extends BaseReleaseProps {
  page: 'home';
  homeContent: ReleaseVersionHomeContent;
}

interface ExploreDataProps extends BaseReleaseProps {
  page: 'explore';
}

interface MethodologyProps extends BaseReleaseProps {
  page: 'methodology';
  methodologiesSummary: PublicationMethodologiesList;
}

interface HelpProps extends BaseReleaseProps {
  page: 'help';
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

  const generateNavItems = (): NavItem[] => {
    switch (page) {
      case 'home':
        return [
          {
            id: 'headlines-section',
            text: 'Headlines facts and figures',
          },
          contactUsNavItem,
        ];

      case 'methodology': {
        const hasMethodologies =
          props.methodologiesSummary.methodologies.length > 0 ||
          props.methodologiesSummary.externalMethodology;

        return [
          hasMethodologies && {
            id: 'methodology-section',
            text: 'Methodology',
          },
          contactUsNavItem,
        ].filter(item => !!item);
      }

      default:
        return [contactUsNavItem];
    }
  };

  return page === 'old' ? (
    <PublicationReleasePageCurrent releaseVersion={props.releaseVersion} />
  ) : (
    <ReleasePageShell
      activePage={page}
      inPageNavItems={generateNavItems()}
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
      {page === 'explore' && <p>TODO EES-6444 Explore Data page</p>}
      {page === 'methodology' && (
        <ReleaseMethodologyPage
          publicationSummary={props.publicationSummary}
          methodologiesSummary={props.methodologiesSummary}
        />
      )}
      {page === 'help' && <p>TODO EES-6446 Help and information page</p>}
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
          case undefined:
            return {
              props: {
                ...baseProps,
                page: 'home',
                homeContent: await queryClient.fetchQuery(
                  publicationQueries.getReleaseVersionHomeContent(
                    publicationSlug,
                    releaseSlug,
                  ),
                ),
              },
            };

          case 'explore':
            return {
              props: {
                ...baseProps,
                page: 'explore',
              },
            };

          case 'methodology':
            return {
              props: {
                ...baseProps,
                page: 'methodology',
                methodologiesSummary: await queryClient.fetchQuery(
                  publicationQueries.getPublicationMethodologies(
                    publicationSlug,
                  ),
                ),
              },
            };

          case 'help':
            return {
              props: {
                ...baseProps,
                page: 'help',
              },
            };

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
