import Details from '@common/components/Details';
import publicationService, {
  PublicationSummary,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import SearchForm from '@frontend/components/SearchForm';
import ReleasePageTitle from '@frontend/modules/find-statistics/components/ReleasePageTitle';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

export interface TableToolSearchPageProps {
  latestReleaseVersion: ReleaseVersionSummary;
  publicationSummary: PublicationSummary;
}

const TableToolSearchPage: NextPage<TableToolSearchPageProps> = ({
  latestReleaseVersion,
  publicationSummary,
}) => {
  return (
    <Page
      title={`Search statistics - ${publicationSummary.title}`}
      description={`Search statistics in ${publicationSummary.title.toLocaleLowerCase()} using natural language`}
      breadcrumbs={[
        { name: 'Table tool', link: `/data-tables/${publicationSummary.slug}` },
      ]}
      pageTitleComponent={
        <ReleasePageTitle
          publicationSummary={publicationSummary}
          releaseTitle={latestReleaseVersion.title}
          publishingOrganisations={latestReleaseVersion.publishingOrganisations}
        />
      }
      width="wide"
    >
      <div className="govuk-grid-row govuk-!-margin-bottom-4">
        <div className="govuk-grid-column-two-thirds">
          <SearchForm
            label="Search these statistics"
            hint={`Use natural language to search for statistics relating to ${publicationSummary.title}.`}
            onSubmit={() => {}}
          />
          <Details
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-4"
            summary="Help and example searches"
          >
            <p>
              Use phrases to find the statistics you are looking for such as:
            </p>
            <ul>
              <li>Show results for science</li>
              <li>Compare subjects</li>
              <li>Filter by North West</li>
              <li>Show me English results from 2024</li>
            </ul>
            <p>
              You can preview results and also filter results further to help
              you find something specific
            </p>
          </Details>
        </div>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<
  TableToolSearchPageProps
> = async ({ query }) => {
  if (process.env.APP_ENV === 'Production') {
    return {
      notFound: true,
    };
  }

  const { publicationSlug = '' } = query as Dictionary<string>;

  if (!publicationSlug) {
    return {
      notFound: true,
    };
  }

  const publicationSummary = await publicationService.getPublicationSummary(
    publicationSlug,
  );

  const latestReleaseVersion =
    await publicationService.getReleaseVersionSummary(
      publicationSlug,
      publicationSummary.latestRelease.slug,
    );
  return {
    props: {
      publicationSummary,
      latestReleaseVersion,
    },
  };
};

export default TableToolSearchPage;
