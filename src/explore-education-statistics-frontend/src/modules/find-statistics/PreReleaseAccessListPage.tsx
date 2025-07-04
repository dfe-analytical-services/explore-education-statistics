import FormattedDate from '@common/components/FormattedDate';
import ContentHtml from '@common/components/ContentHtml';
import publicationService, {
  PreReleaseAccessListSummary,
} from '@common/services/publicationService';
import glossaryService from '@frontend/services/glossaryService';
import Page from '@frontend/components/Page';
import { GetServerSideProps } from 'next';
import React from 'react';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';

interface Props {
  release: PreReleaseAccessListSummary;
}

const PreReleaseAccessListPage = ({ release }: Props) => {
  return (
    <Page
      title={release.publication.title}
      caption={release.title}
      metaTitle={`${
        release.publication.title
      } pre-release access list ${release.title.toLocaleLowerCase()}`}
      description={`Pre-release access list for statistics on ${release.publication.title.toLocaleLowerCase()} ${release.title.toLocaleLowerCase()}.`}
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
        {
          name: release.publication.title,
          link: `/find-statistics/${release.publication.slug}/${release.slug}`,
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

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
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
  },
);
