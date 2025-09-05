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

interface Props {
  publicationSummary?: PublicationSummaryRedesign;
  releaseVersionSummary?: ReleaseVersionSummary;
  releaseVersion?: ReleaseVersion;
  previewRedesign?: boolean;
}

const PublicationReleasePage: NextPage<Props> = ({
  publicationSummary,
  releaseVersion,
  releaseVersionSummary,
  previewRedesign,
}) => {
  return previewRedesign && releaseVersionSummary && publicationSummary ? (
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

    if (
      redesign &&
      redesign === 'true' &&
      process.env.APP_ENV !== 'Production'
    ) {
      const publicationSummary = await publicationService.getPublicationSummary(
        publicationSlug,
      );

      const releaseVersionSummary = await (releaseSlug
        ? publicationService.getReleaseVersionSummary(
            publicationSlug,
            releaseSlug,
          )
        : publicationService.getPublicationLatestReleaseVersionSummary(
            publicationSlug,
          ));

      if (!releaseSlug) {
        return {
          redirect: {
            destination: `/find-statistics/${publicationSlug}/${releaseVersionSummary.slug}`,
            permanent: true,
          },
        };
      }

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
