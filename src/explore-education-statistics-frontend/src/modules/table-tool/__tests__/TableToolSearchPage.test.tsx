import render from '@common-test/render';
import {
  testPublicationSummary,
  testReleaseVersionSummary,
} from '@frontend/modules/find-statistics/__tests__/__data__/testReleaseData';
import TableToolSearchPage from '@frontend/modules/table-tool/TableToolSearchPage';
import tableToolSearchService, {
  FatalError,
  PipelineStage,
  SearchStreamOptions,
} from '@frontend/services/tableToolSearchService';
import { act, screen, within } from '@testing-library/react';

jest.mock('@frontend/services/tableToolSearchService', () => {
  const actual = jest.requireActual(
    '@frontend/services/tableToolSearchService',
  );
  return {
    __esModule: true,
    ...actual,
    default: {
      postSearchStream: jest.fn(),
    },
  };
});

const mockPostSearchStream =
  tableToolSearchService.postSearchStream as jest.Mock;

describe('TableToolSearchPage', () => {
  beforeEach(() => {
    mockPostSearchStream.mockClear();
  });

  test('renders the page correctly with search form', () => {
    render(
      <TableToolSearchPage
        publicationSummary={testPublicationSummary}
        latestReleaseVersion={testReleaseVersionSummary}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Pupil attendance in schools',
        level: 1,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText('Search these statistics'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: /Help and example searches/,
      }),
    ).toBeInTheDocument();

    expect(screen.queryByText('Processing request')).not.toBeInTheDocument();
  });

  test('successfully progresses through all pipeline stages and renders final results', async () => {
    const { user } = render(
      <TableToolSearchPage
        publicationSummary={testPublicationSummary}
        latestReleaseVersion={testReleaseVersionSummary}
      />,
    );

    // Capture the callbacks so we can stream fake data to the component
    let capturedOptions: SearchStreamOptions;
    mockPostSearchStream.mockImplementation((_params, options) => {
      capturedOptions = options;
      return Promise.resolve();
    });

    const input = screen.getByLabelText('Search these statistics');
    const submitBtn = screen.getByRole('button', { name: 'Search' });

    await user.type(input, 'test search term');
    await user.click(submitBtn);

    expect(mockPostSearchStream).toHaveBeenCalledWith(
      {
        userQuery: 'test search term',
        publicationId: '96f418e7-3ddb-4a8c-60dc-08deb7f1c424',
      },
      expect.anything(),
    );

    act(() => {
      capturedOptions.onMessage({ stage: PipelineStage.STARTING });
    });

    expect(
      screen.getByText('Analysing "test search term"'),
    ).toBeInTheDocument();
    expect(screen.getByText('Processing request')).toBeInTheDocument();

    act(() => {
      capturedOptions.onMessage({
        stage: PipelineStage.RETRIEVED,
        data: {
          datasets: [
            { title: 'Dataset A', relevanceScore: 10, rawRelevanceScore: 0.1 },
            {
              title: 'Dataset B',
              relevanceScore: 85.5,
              rawRelevanceScore: 0.5,
            },
          ],
        },
      });
    });

    expect(
      screen.getByText('Identify relevant information from data sets'),
    ).toBeInTheDocument();
    expect(screen.getByText('Dataset A')).toBeInTheDocument();
    expect(screen.getByText('Dataset B')).toBeInTheDocument();

    act(() => {
      capturedOptions.onMessage({
        stage: PipelineStage.RERANKER,
        data: {
          shortlistedDatasets: [
            {
              fileId: 'file-1',
              title: 'Dataset B',
              relevanceScore: 85.5,
              relevanceReason: 'Test relevance reason',
              relevantFilters: [],
            },
          ],
          queryRequirements: { filters: [], geography: [], timePeriod: '' },
          confidence: 'high',
        },
      });
    });

    expect(
      screen.getByText('Choosing the most relevant data sets'),
    ).toBeInTheDocument();
    expect(screen.queryByText('Dataset A')).not.toBeInTheDocument();
    expect(screen.getByText('Dataset B')).toBeInTheDocument();
    expect(screen.getByText('85.5% relevance')).toBeInTheDocument();

    act(() => {
      capturedOptions.onMessage({
        stage: PipelineStage.COMPLETE,
        data: {
          datasets: [
            {
              fileId: 'file-1',
              title: 'Dataset B',
              aiSummary: 'Test AI summary.',
              filters: [],
              indicators: [],
              geographicLevels: {},
            },
          ],
          token_usage: 100,
        },
      });
    });

    expect(
      screen.getByRole('heading', { name: 'Dataset B' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Test AI summary.')).toBeInTheDocument();

    expect(screen.queryByText('Processing request')).not.toBeInTheDocument();
  });

  test('displays no results message', async () => {
    const { user } = render(
      <TableToolSearchPage
        publicationSummary={testPublicationSummary}
        latestReleaseVersion={testReleaseVersionSummary}
      />,
    );

    let capturedOptions: SearchStreamOptions;
    mockPostSearchStream.mockImplementation((_params, options) => {
      capturedOptions = options;
      return Promise.resolve();
    });

    await user.type(
      screen.getByLabelText('Search these statistics'),
      'abcdefg',
    );
    await user.click(screen.getByRole('button', { name: 'Search' }));

    act(() => {
      capturedOptions.onMessage({
        stage: PipelineStage.RETRIEVED,
        data: { datasets: [] },
      });
    });

    const warningMessage = screen.getByRole('alert');
    expect(
      within(warningMessage).getByText(
        /We couldn't find any results for your search./,
      ),
    ).toBeInTheDocument();

    expect(screen.queryByText('Processing request')).not.toBeInTheDocument();
  });

  test('displays an error message when a fatal error is thrown', async () => {
    const { user } = render(
      <TableToolSearchPage
        publicationSummary={testPublicationSummary}
        latestReleaseVersion={testReleaseVersionSummary}
      />,
    );

    mockPostSearchStream.mockRejectedValueOnce(
      new FatalError('Simulated pipeline failure'),
    );

    await user.type(
      screen.getByLabelText('Search these statistics'),
      'bad search',
    );
    await user.click(screen.getByRole('button', { name: 'Search' }));

    expect(
      await screen.findByText(
        'Search failed: Simulated pipeline failure. Please try again later.',
      ),
    ).toBeInTheDocument();
  });

  test('aborts the previous stream if a new search is submitted', async () => {
    const { user } = render(
      <TableToolSearchPage
        publicationSummary={testPublicationSummary}
        latestReleaseVersion={testReleaseVersionSummary}
      />,
    );

    mockPostSearchStream.mockImplementation(() => new Promise(() => {}));

    const input = screen.getByLabelText('Search these statistics');
    const submitBtn = screen.getByRole('button', { name: 'Search' });

    await user.type(input, 'first');
    await user.click(submitBtn);

    // Get the first abort signal passed to the service
    // mockPostSearchStream.mock.calls = [
    //   [ paramsObject1, optionsObject1 ],
    // ]
    const firstSignal = mockPostSearchStream.mock.calls[0][1].signal;
    expect(firstSignal.aborted).toBe(false);

    await user.clear(input);
    await user.type(input, 'second');
    await user.click(submitBtn);

    expect(firstSignal.aborted).toBe(true);

    expect(mockPostSearchStream).toHaveBeenCalledTimes(2);
  });
});
