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
  StageComplete,
  StageReranker,
  StageRetrieved,
  TtSearchStreamMessage,
} from '@frontend/services/tableToolSearchService';
import { GetServerSideProps, NextPage } from 'next';
import React, { useRef, useState } from 'react';

export interface TableToolSearchPageProps {
  latestReleaseVersion: ReleaseVersionSummary;
  publicationSummary: PublicationSummary;
}

export interface SearchPipelineData {
  currentStage: TtSearchStreamMessage['stage'] | null;
  retrievedData: StageRetrieved['data'] | null;
  rerankerData: StageReranker['data'] | null;
  finalData: StageComplete['data'] | null;
}

const initialPipelineData: SearchPipelineData = {
  currentStage: null,
  retrievedData: null,
  rerankerData: null,
  finalData: null,
};

const TableToolSearchPage: NextPage<TableToolSearchPageProps> = ({
  latestReleaseVersion,
  publicationSummary,
}) => {
  const [isSearching, setIsSearching] = useState(false);
  const [pipelineData, setPipelineData] =
    useState<SearchPipelineData>(initialPipelineData);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const handleSearchSubmit = async (searchTerm: string) => {
    if (!searchTerm.trim()) return;

    // Abort any existing search stream
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    setPipelineData(initialPipelineData);
    setIsSearching(true);

    try {
      await tableToolSearchService.postSearchStream(
        {
          userQuery: searchTerm.trim(),
          publicationId: publicationSummary.id,
        },
        {
          signal: abortControllerRef.current.signal,
          onMessage: message => {
            setPipelineData(prev => {
              const updatedState = { ...prev, currentStage: message.stage };

              switch (message.stage) {
                case 'retrieved datasets':
                  updatedState.retrievedData = message.data;
                  break;
                case 'reranker complete':
                  updatedState.rerankerData = message.data;
                  break;
                case 'pipeline complete':
                  updatedState.finalData = message.data;
                  setIsSearching(false);
                  break;
                case 'starting pipeline':
                  break;
                default:
                  return updatedState;
              }

              return updatedState;
            });
          },
          onRetriableError: errorMessage => {
            setError(errorMessage);
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

          <div aria-live="polite">
            {isSearching && (
              <>
                <strong>Status:</strong>{' '}
                {pipelineData.currentStage || 'Connecting...'}
              </>
            )}
          </div>

          {error && <ErrorMessage announceError>{error}</ErrorMessage>}

          {pipelineData.rerankerData && (
            <div className="govuk-body">
              <ol>
                {pipelineData.rerankerData.shortlisted_datasets.map(dataset => (
                  <li key={dataset.file_id}>
                    {dataset.title} - {dataset.relevance_reason}
                  </li>
                ))}
              </ol>
            </div>
          )}

          {pipelineData.finalData && (
            // TODO EES-7213 render results
            <div className="results-container">
              <h2 className="govuk-heading-m">Results</h2>
              <ol>
                {pipelineData.finalData.datasets.map(dataset => (
                  <li key={dataset.fileId}>
                    <h3>Dataset: {dataset.fileId}</h3>
                    <div>
                      <p className="govuk-body">{dataset.aiSummary}</p>
                    </div>
                  </li>
                ))}
              </ol>
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
