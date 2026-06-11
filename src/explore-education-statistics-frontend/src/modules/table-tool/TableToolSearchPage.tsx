import Details from '@common/components/Details';
import ErrorMessage from '@common/components/ErrorMessage';
import logger from '@common/services/logger';
import publicationService, {
  PublicationSummary,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import SearchForm from '@frontend/components/SearchForm';
import ReleasePageTitle from '@frontend/modules/find-statistics/components/ReleasePageTitle';
import tableToolSearchService, {
  FatalError,
  FinalDataset,
} from '@frontend/services/tableToolSearchService';
import { GetServerSideProps, NextPage } from 'next';
import React, { useRef, useState } from 'react';

export interface TableToolSearchPageProps {
  latestReleaseVersion: ReleaseVersionSummary;
  publicationSummary: PublicationSummary;
}

const TableToolSearchPage: NextPage<TableToolSearchPageProps> = ({
  latestReleaseVersion,
  publicationSummary,
}) => {
  const [isSearching, setIsSearching] = useState(false);
  const [currentStage, setCurrentStage] = useState<string | null>(null);
  const [finalResults, setFinalResults] = useState<FinalDataset[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const handleSearchSubmit = async (searchTerm: string) => {
    if (!searchTerm.trim()) return;

    // Abort any existing search stream
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    setIsSearching(true);
    setCurrentStage('Connecting...');
    setFinalResults(null);
    setError(null);

    try {
      await tableToolSearchService.postSearchStream(
        {
          userQuery: searchTerm.trim(),
          publicationId: publicationSummary.id,
        },
        {
          signal: abortControllerRef.current.signal,
          onMessage: parsed => {
            setCurrentStage(parsed.stage);
            if (parsed.stage === 'pipeline complete') {
              setFinalResults(parsed.data.datasets);
              setIsSearching(false);
            }
          },
          onClose: () => {
            setIsSearching(false);
          },
          onError: errorMessage => {
            setCurrentStage(errorMessage); // This will be a connection lost error, fetch event source will retry automatically
          },
        },
      );
    } catch (err) {
      // Ignore user-triggered aborts
      if (err instanceof Error && err.name === 'AbortError') return;

      logger.error(err);
      setIsSearching(false);

      if (err instanceof FatalError) {
        setError(`Search failed: ${err.message}. Please try again later.`);
      } else if (err instanceof Error) {
        setError(err.message);
      } else {
        setError(
          'An unexpected system error occurred. Please try again later.',
        );
      }
    }
  };

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
        />
      }
      width="wide"
    >
      <div className="govuk-grid-row govuk-!-margin-bottom-4">
        <div className="govuk-grid-column-two-thirds">
          <SearchForm
            label="Search these statistics"
            hint={`Use natural language to search for statistics relating to ${publicationSummary.title}.`}
            onSubmit={handleSearchSubmit}
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

          {/* TODO render statuses and data as it's being returned.
           Could be that we have state vars for each stage, which we fill,
           then interrogate actual currentStage to determine what to render  */}
          {isSearching && (
            <div className="govuk-inset-text">
              <strong>Status:</strong> {currentStage}
            </div>
          )}

          {error && <ErrorMessage announceError>{error}</ErrorMessage>}

          {finalResults && (
            // TODO EES-7213 render results
            <div className="results-container">
              <h2 className="govuk-heading-m">Results</h2>
              <ul>
                {finalResults.map(dataset => (
                  <li key={dataset.fileId}>
                    <h3>Dataset: {dataset.fileId}</h3>
                    <div>
                      <p className="govuk-body">{dataset.aiSummary}</p>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
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
