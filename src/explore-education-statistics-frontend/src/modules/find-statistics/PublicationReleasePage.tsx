import publicationService, {
  ReleaseVersion,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import PublicationReleasePageHome from './PublicationReleasePageHome';
import PublicationReleasePageCurrent from './PublicationReleasePageCurrent';

interface Props {
  releaseVersion: ReleaseVersion;
  previewRedesign?: boolean;
}

const PublicationReleasePage: NextPage<Props> = ({
  releaseVersion,
  previewRedesign,
}) => {
  return previewRedesign ? (
    <PublicationReleasePageHome releaseVersion={releaseVersion} />
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
        previewRedesign: !!redesign,
      },
    };
  },
);

export default PublicationReleasePage;
