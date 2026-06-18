import Details from '@common/components/Details';
import ErrorMessage from '@common/components/ErrorMessage';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
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
  PipelineStage,
  PipelineStageLabels,
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
  const [searchedTerm, setSearchedTerm] = useState<string | null>(null);
  const [pipelineData, setPipelineData] =
    useState<SearchPipelineData>(initialPipelineData);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const handleSearchSubmit = async (searchTerm: string) => {
    if (!searchTerm.trim()) return;

    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    setPipelineData(initialPipelineData);
    setSearchedTerm(searchTerm.trim());
    setError(null);

    try {
      await tableToolSearchService.postSearchStream(
        {
          user_query: searchTerm.trim(),
          publication:
            'Pupil attendance in schools in England' || publicationSummary.id, // TODO change to publication ID
        },
        {
          signal: abortControllerRef.current.signal,
          onMessage: message => {
            setPipelineData(prev => {
              const updatedState = { ...prev, currentStage: message.stage };

              switch (message.stage) {
                case PipelineStage.RETRIEVED:
                  updatedState.retrievedData = message.data;
                  break;
                case PipelineStage.RERANKER:
                  updatedState.rerankerData = message.data;
                  break;
                case PipelineStage.COMPLETE:
                  updatedState.finalData = message.data;
                  break;
                case PipelineStage.STARTING:
                  break;
                default:
                  return updatedState;
              }

              return updatedState;
            });

            // Abort early if there are no results.
            if (
              (message.stage === PipelineStage.RETRIEVED &&
                message.data.datasets.length === 0) ||
              (message.stage === PipelineStage.RERANKER &&
                message.data.shortlistedDatasets.length === 0)
            ) {
              abortControllerRef.current?.abort();
            }
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

  const hasNoResults =
    (pipelineData.retrievedData &&
      pipelineData.retrievedData.datasets.length === 0) ||
    (pipelineData.rerankerData &&
      pipelineData.rerankerData.shortlistedDatasets.length === 0) ||
    (pipelineData.finalData && pipelineData.finalData.datasets.length === 0);

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

          {error && <ErrorMessage announceError>{error}</ErrorMessage>}

          {hasNoResults && (
            <WarningMessage className="govuk-!-margin-top-4">
              We couldn't find any results for your search. <br />
              Please make sure your query is relevant to{' '}
              {publicationSummary.title}
            </WarningMessage>
          )}
        </div>
      </div>

      {pipelineData.currentStage && !hasNoResults && (
        <div className="govuk-grid-row govuk-!-margin-bottom-4">
          <div className="govuk-grid-column-two-thirds">
            <h2 className="govuk-heading-m">
              {PipelineStageLabels[pipelineData.currentStage]}
            </h2>

            {pipelineData.currentStage === PipelineStage.STARTING && (
              <InsetText>
                <p>Analysing "{searchedTerm}"</p>
              </InsetText>
            )}

            {pipelineData.currentStage === PipelineStage.RETRIEVED &&
              pipelineData.retrievedData?.datasets && (
                <div className="govuk-body">
                  <ul>
                    {pipelineData.retrievedData.datasets.map(dataset => (
                      <li key={dataset.title} className="dfe-flex">
                        <p>{dataset.title}</p>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

            {pipelineData.currentStage === PipelineStage.RERANKER &&
              pipelineData.rerankerData?.shortlistedDatasets && (
                <div className="govuk-body">
                  <ul>
                    {pipelineData.rerankerData.shortlistedDatasets.map(
                      dataset => (
                        <li
                          key={dataset.fileId}
                          className="dfe-flex dfe-justify-between dfe-justify-content--space-between"
                        >
                          <p>{dataset.title}</p>
                          <p>{dataset.relevanceScore} relevance</p>
                        </li>
                      ),
                    )}
                  </ul>
                </div>
              )}

            {pipelineData.currentStage === PipelineStage.COMPLETE &&
            pipelineData.finalData &&
            pipelineData.finalData.datasets.length > 0 ? (
              // TODO EES-7213 render results
              <div className="results-container">
                <ol>
                  {pipelineData.finalData.datasets.map(dataset => (
                    <li key={dataset.fileId}>
                      <p>
                        Dataset: <strong>{dataset.fileId}</strong>
                      </p>
                      <div>
                        <p className="govuk-body">{dataset.aiSummary}</p>
                      </div>
                    </li>
                  ))}
                </ol>
              </div>
            ) : (
              <LoadingSpinner />
            )}
          </div>
        </div>
      )}
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
