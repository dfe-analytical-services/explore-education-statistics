import ApiDataSetCreateModal from '@admin/pages/release/api-data-sets/components/ApiDataSetCreateModal';
import _apiDataSetCandidateService, {
  ApiDataSetCandidate,
} from '@admin/services/apiDataSetCandidateService';
import _apiDataSetService from '@admin/services/apiDataSetService';
import baseRender from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import { createMemoryHistory, History } from 'history';
import { ReactElement } from 'react';
import { Router } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetCandidateService');

const apiDataSetCandidateService = jest.mocked(_apiDataSetCandidateService);
const apiDataSetService = jest.mocked(_apiDataSetService);

describe('ApiDataSetCreateModal', () => {
  const testCandidates: ApiDataSetCandidate[] = [
    {
      releaseFileId: 'release-file-1',
      title: 'Test data set 1',
    },
    {
      releaseFileId: 'release-file-2',
      title: 'Test data set 2',
    },
  ];

  test('renders warning message in modal when no candidates', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue([]);

    const { user } = render(
      <ApiDataSetCreateModal
        publicationId="publication-id"
        releaseId="release-id"
      />,
    );

    await user.click(
      await screen.findByRole('button', { name: 'Create API data set' }),
    );

    expect(
      await screen.findByText(
        /No API data sets can be created as there are no candidates data files available/,
      ),
    ).toBeInTheDocument();

    expect(screen.getByRole('button', { name: 'Close' })).toBeInTheDocument();
  });

  test('renders form in modal when there are candidates', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);

    const { user } = render(
      <ApiDataSetCreateModal
        publicationId="publication-id"
        releaseId="release-id"
      />,
    );

    expect(await screen.findByText('Create API data set')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Create API data set' }),
    );

    expect(
      await screen.findByText('Create a new API data set'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Create a new API data set' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Data set')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Close' }),
    ).not.toBeInTheDocument();
  });

  test('submitting the form calls correct service and redirects to next page', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);
    apiDataSetService.createDataSet.mockResolvedValue({
      id: 'data-set-id',
      title: 'Test title',
      summary: 'Test summary',
      status: 'Draft',
    });

    const history = createMemoryHistory();

    const { user } = render(
      <ApiDataSetCreateModal
        publicationId="publication-id"
        releaseId="release-id"
      />,
      { history },
    );

    expect(await screen.findByText('Create API data set')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Create API data set' }),
    );

    await user.selectOptions(
      await screen.findByLabelText('Data set'),
      testCandidates[0].releaseFileId,
    );

    expect(apiDataSetService.createDataSet).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    );

    await waitFor(() => {
      expect(apiDataSetService.createDataSet).toHaveBeenCalledTimes(1);
      expect(apiDataSetService.createDataSet).toHaveBeenCalledWith<
        Parameters<typeof apiDataSetService.createDataSet>
      >({
        releaseFileId: testCandidates[0].releaseFileId,
      });
    });

    expect(history.location.pathname).toBe(
      '/publication/publication-id/release/release-id/api-data-sets/data-set-id',
    );
  });

  function render(
    ui: ReactElement,
    options?: {
      history: History;
    },
  ) {
    const { history = createMemoryHistory() } = options ?? {};

    return baseRender(<Router history={history}>{ui}</Router>);
  }
});
