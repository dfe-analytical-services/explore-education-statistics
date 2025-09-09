import publicationService, {
  PublicationSummaryRedesign,
  ReleaseVersion,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import PublicationReleasePageHome from './PublicationReleasePageHome';
import PublicationReleasePageCurrent from './PublicationReleasePageCurrent';

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
    releaseVersion && (
      <PublicationReleasePageCurrent releaseVersion={releaseVersion} />
    )
  );
};

export const getServerSideProps: GetServerSideProps = withAxiosHandler(
  async ({ query }) => {
    const {
      publication: publicationSlug,
      release: releaseSlug,
      redesign,
    } = query as Dictionary<string>;

    if (redesign === 'true' && process.env.APP_ENV !== 'Production') {
      const publicationSummary =
        await publicationService.getPublicationSummaryRedesign(publicationSlug);

      if (!releaseSlug) {
        return {
          redirect: {
            destination: `/find-statistics/${publicationSlug}/${publicationSummary.latestRelease.slug}?redesign=true`, // TODO EES-6449 remove redesign query param
            permanent: true,
          },
        };
      }

      const releaseVersionSummary =
        await publicationService.getReleaseVersionSummary(
          publicationSlug,
          releaseSlug,
        );

      return {
        props: {
          publicationSummary,
          releaseVersionSummary,
          previewRedesign: true,
        },
      };
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
