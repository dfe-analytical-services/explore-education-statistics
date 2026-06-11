// eslint-disable-next-line max-classes-per-file
import logger from '@common/services/logger';
import {
  EventStreamContentType,
  fetchEventSource,
} from '@microsoft/fetch-event-source';

export type AiPipelineStage =
  | 'starting pipeline'
  | 'retrieved datasets'
  | 'reranker complete'
  | 'pipeline complete';

export interface RetrievedDataset {
  title: string;
  relevanceScore: number;
  rawRelevanceScore: number;
}

export interface ShortlistedDataset {
  file_id: string;
  title: string;
  relevance_reason: string;
  relevant_filters: string[];
  relevanceScore: number;
}

export interface FinalDataset {
  fileId: string;
  filters: string[];
  indicators: string[];
  aiSummary: string;
}

export interface StageStarting {
  stage: 'starting pipeline';
}

export interface StageRetrieved {
  stage: 'retrieved datasets';
  data: { datasets: RetrievedDataset[] };
}

export interface StageReranker {
  stage: 'reranker complete';
  data: {
    query_requirements: {
      filters: string[];
      geography: string[];
      time_period: string;
    };
    shortlisted_datasets: ShortlistedDataset[];
    confidence: string;
  };
}

export interface StageComplete {
  stage: 'pipeline complete';
  data: {
    datasets: FinalDataset[];
    token_usage: number;
  };
}

export type AiSseMessage =
  | StageStarting
  | StageRetrieved
  | StageReranker
  | StageComplete;

export interface TableToolSearchListRequest {
  userQuery: string;
  publicationId: string;
}

export interface SearchStreamOptions {
  maxRetries?: number;
  signal?: AbortSignal;
  onClose?: () => void;
  onError?: (errorMessage: string) => void;
  onMessage: (message: AiSseMessage) => void;
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

const tableToolSearchService = {
  async postSearchStream(
    params: TableToolSearchListRequest,
    options: SearchStreamOptions,
  ): Promise<void> {
    let retryCount = 0;
    const maxRetries = options.maxRetries ?? 3;

    return fetchEventSource('/api/search-table-tool', {
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
          throw new FatalError(`Server responded with ${response.status}`);
        }
        throw new RetriableError(`Server responded with ${response.status}`);
      },

      onmessage(msg) {
        if (msg.event === 'FatalError') {
          throw new FatalError(msg.data);
        }

        if (!msg.data) return;

        try {
          const parsed: AiSseMessage = JSON.parse(msg.data);
          options.onMessage(parsed);
        } catch (err) {
          logger.error(err);
        }
      },

      onclose() {
        throw new RetriableError('Connection closed by server prematurely');
      },

      onerror(err) {
        if (err instanceof FatalError) {
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

        if (options.onError) {
          options.onError(
            err.message
              ? err.message
              : `Connection unstable. Retrying (${retryCount}/${maxRetries})...`,
          );
        }

        return 3000; // Retry after 3 seconds
      },
    });
  },
};

export default tableToolSearchService;
