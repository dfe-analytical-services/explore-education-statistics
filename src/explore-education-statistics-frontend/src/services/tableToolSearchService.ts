// eslint-disable-next-line max-classes-per-file
import logger from '@common/services/logger';
import { Dictionary } from '@common/types';
import {
  EventStreamContentType,
  fetchEventSource,
} from '@microsoft/fetch-event-source';

export const PipelineStage = {
  STARTING: 'starting pipeline',
  RETRIEVED: 'retrieved datasets',
  RERANKER: 'reranker complete',
  COMPLETE: 'pipeline complete',
} as const;

export type PipelineStageType =
  (typeof PipelineStage)[keyof typeof PipelineStage];

export const PipelineStageLabels: Record<PipelineStageType, string> = {
  [PipelineStage.STARTING]: 'Understanding your question',
  [PipelineStage.RETRIEVED]: 'Identify relevant information from data sets',
  [PipelineStage.RERANKER]: 'Choosing the most relevant data sets',
  [PipelineStage.COMPLETE]: 'Results',
};

export interface RetrievedDataset {
  title: string;
  relevanceScore: number;
  rawRelevanceScore: number;
}

export interface ShortlistedDataset {
  fileId: string;
  title: string;
  relevanceReason: string;
  relevantFilters: string[];
  relevanceScore: number;
}

export interface GeographicLevelItem {
  property: string;
  name: string;
  score: number;
}

export interface FinalDataset {
  fileId: string;
  filters: string[];
  indicators: string[];
  geographicLevels: Dictionary<GeographicLevelItem[]>;
  aiSummary: string;
}

export interface StageStarting {
  stage: typeof PipelineStage.STARTING;
}

export interface StageRetrieved {
  stage: typeof PipelineStage.RETRIEVED;
  data: { datasets: RetrievedDataset[] };
}

export interface StageReranker {
  stage: typeof PipelineStage.RERANKER;
  data: {
    queryRequirements: {
      filters: string[];
      geography: string[];
      timePeriod: string;
    };
    shortlistedDatasets: ShortlistedDataset[];
    confidence: string;
  };
}

export interface StageComplete {
  stage: typeof PipelineStage.COMPLETE;
  data: {
    datasets: FinalDataset[];
    token_usage: number;
  };
}

export type TtSearchStreamMessage =
  | StageStarting
  | StageRetrieved
  | StageReranker
  | StageComplete;

export interface TableToolSearchListRequest {
  user_query: string;
  publication: string;
}

export interface SearchStreamOptions {
  maxRetries?: number;
  signal?: AbortSignal;
  onRetriableError?: (errorMessage: string) => void;
  onMessage: (message: TtSearchStreamMessage) => void;
}

export class FatalError extends Error {
  constructor(message?: string) {
    super(message);
    this.name = 'FatalError';
  }
}

export class RetriableError extends Error {
  constructor(message?: string) {
    super(message);
    this.name = 'RetriableError';
  }
}

export class StreamCompleteError extends Error {
  constructor(message?: string) {
    super(message);
    this.name = 'StreamCompleteError';
  }
}

const tableToolSearchService = {
  async postSearchStream(
    params: TableToolSearchListRequest,
    options: SearchStreamOptions,
  ): Promise<void> {
    let retryCount = 0;
    const maxRetries = options.maxRetries ?? 3;
    let isComplete = false;

    try {
      await fetchEventSource('/api/search-table-tool', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'text/event-stream',
        },
        body: JSON.stringify(params),
        signal: options.signal,

        async onopen(response) {
          if (
            response.ok &&
            response.headers.get('content-type') === EventStreamContentType
          ) {
            retryCount = 0;
            return;
          }
          if (
            response.status >= 400 &&
            response.status < 500 &&
            response.status !== 429
          ) {
            throw new FatalError(
              `Server responded with ${response.status} - ${response.statusText}`,
            );
          }
          throw new RetriableError(
            `Server responded with ${response.status}. Retrying.`,
          );
        },

        onmessage(msg) {
          if (msg.event === 'FatalError') {
            throw new FatalError(msg.data);
          }

          if (!msg.data) return;

          try {
            const parsed: TtSearchStreamMessage = JSON.parse(msg.data);
            if (parsed.stage === 'pipeline complete') {
              isComplete = true;
            }
            options.onMessage(parsed);
          } catch (err) {
            logger.error(err);
          }
        },

        onclose() {
          if (isComplete) {
            throw new StreamCompleteError();
          }
          throw new RetriableError();
        },

        onerror(err) {
          if (err instanceof FatalError || err instanceof StreamCompleteError) {
            throw err; // Rethrowing aborts the stream
          }

          retryCount += 1;

          if (retryCount > maxRetries) {
            throw new FatalError(
              `Maximum retry limit (${maxRetries}) reached. Last error: ${
                err instanceof Error ? err.message : 'Unknown'
              }`,
            );
          }

          options.onRetriableError?.(
            err.message
              ? err.message
              : `Connection unstable. Retrying (${retryCount}/${maxRetries})...`,
          );

          return 3000; // Retry after 3 seconds
        },
      });
    } catch (err) {
      if (err instanceof StreamCompleteError) {
        return;
      }

      throw err;
    }
  },
};

export default tableToolSearchService;
