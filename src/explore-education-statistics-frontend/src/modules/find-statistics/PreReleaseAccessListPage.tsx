import FormattedDate from '@common/components/FormattedDate';
import SanitizeHtml from '@common/components/SanitizeHtml';
import publicationService, {
  Release,
} from '@common/services/publicationService';
import Page from '@frontend/components/Page';
import { GetServerSideProps } from 'next';
import React from 'react';

interface Props {
  release: Release;
}

const PreReleaseAccessListPage = ({ release }: Props) => {
  return (
    <Page
      title={release.publication.title}
      caption={release.title}
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: release.publication.title,
          link: release.latestRelease
            ? `/find-statistics/${release.publication.slug}`
            : `/find-statistics/${release.publication.slug}/${release.slug}`,
        },
      ]}
      breadcrumbLabel="Pre-release access list"
    >
      <h2>Pre-release access list</h2>

      {release.published && (
        <p>
          <strong>
            Published <FormattedDate>{release.published}</FormattedDate>
          </strong>
        </p>
      )}

      {release.preReleaseAccessList && (
        <SanitizeHtml dirtyHtml={release.preReleaseAccessList} />
      )}
    </Page>
  );
};

export default PreReleaseAccessListPage;

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { publication, release } = query as {
    publication: string;
    release: string;
  };

  const data = await (release
    ? publicationService.getPublicationRelease(publication, release)
    : publicationService.getLatestPublicationRelease(publication));

  return {
    props: {
      release: data,
    },
  };
};
