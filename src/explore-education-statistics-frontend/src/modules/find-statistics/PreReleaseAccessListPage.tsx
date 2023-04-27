import FormattedDate from '@common/components/FormattedDate';
import ContentHtml from '@common/components/ContentHtml';
import publicationService, {
  PreReleaseAccessListSummary,
} from '@common/services/publicationService';
import glossaryService from '@frontend/services/glossaryService';
import Page from '@frontend/components/Page';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

interface Props {
  release: PreReleaseAccessListSummary;
}

const PreReleaseAccessListPage: NextPage<Props> = ({ release }) => {
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
        <p className="govuk-!-margin-bottom-8">
          <strong>
            Published <FormattedDate>{release.published}</FormattedDate>
          </strong>
        </p>
      )}

      {release.preReleaseAccessList && (
        <ContentHtml
          html={release.preReleaseAccessList}
          getGlossaryEntry={glossaryService.getEntry}
        />
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
    ? publicationService.getPreReleaseAccessList(publication, release)
    : publicationService.getLatestPreReleaseAccessList(publication));

  return {
    props: {
      release: data,
    },
  };
};
