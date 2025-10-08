import publicationService, {
  PublicationSummaryRedesign,
  ReleaseVersion,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import publicationQueries from '@frontend/queries/publicationQueries';
import { QueryClient } from '@tanstack/react-query';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import PublicationReleasePageCurrent from './PublicationReleasePageCurrent';
import PublicationReleasePageHome from './PublicationReleasePageHome';

interface PreviewRedesignProps {
  previewRedesign: true;
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
  releaseVersion?: never;
}

interface CurrentReleaseProps {
  previewRedesign?: never;
  releaseVersion: ReleaseVersion;
  publicationSummary?: never;
  releaseVersionSummary?: never;
}

type Props = PreviewRedesignProps | CurrentReleaseProps;

const PublicationReleasePage: NextPage<Props> = ({
  publicationSummary,
  releaseVersion,
  releaseVersionSummary,
  previewRedesign,
}) => {
  return previewRedesign ? (
    <PublicationReleasePageHome
      publicationSummary={publicationSummary}
      releaseVersionSummary={releaseVersionSummary}
    />
  ) : (
    <PublicationReleasePageCurrent releaseVersion={releaseVersion} />
  );
};

export const getServerSideProps: GetServerSideProps = withAxiosHandler(
  async ({ query }) => {
    const {
      publication: publicationSlug,
      release: releaseSlug,
      redesign,
    } = query as Dictionary<string>;

    const queryClient = new QueryClient();

    if (redesign === 'true' && process.env.APP_ENV !== 'Production') {
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

        return {
          props: {
            publicationSummary,
            releaseVersionSummary,
            previewRedesign: true,
          },
        };
      } catch (error) {
        return {
          notFound: true,
        };
      }
    }

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
        releaseVersion,
      },
    };
  },
);

export default PublicationReleasePage;
