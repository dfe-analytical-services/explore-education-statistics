import ApiDataSetCreateModal from '@admin/pages/release/data/components/ApiDataSetCreateModal';
import _apiDataSetCandidateService, {
  ApiDataSetCandidate,
} from '@admin/services/apiDataSetCandidateService';
import baseRender from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import { createMemoryHistory, History } from 'history';
import { ReactNode } from 'react';
import { Router } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetCandidateService');

const apiDataSetCandidateService = jest.mocked(_apiDataSetCandidateService);

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
        releaseVersionId="release-id"
        onSubmit={Promise.resolve}
      />,
    );

    expect(await screen.findByText('Create API data set')).toBeInTheDocument();

    await user.click(
      await screen.findByRole('button', { name: 'Create API data set' }),
    );

    expect(screen.getByRole('button', { name: 'Close' })).toBeInTheDocument();

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByText(
        /No API data sets can be created as there are no candidate data files available/,
      ),
    ).toBeInTheDocument();

    expect(modal.getByRole('link', { name: 'Data and files' })).toHaveAttribute(
      'href',
      '/publication/publication-id/release/release-id/data#data-uploads',
    );

    await user.click(screen.getByRole('link', { name: 'Data and files' }));

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test('renders form in modal when there are candidates', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);

    const { user } = render(
      <ApiDataSetCreateModal
        publicationId="publication-id"
        releaseVersionId="release-id"
        onSubmit={Promise.resolve}
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

  test('submitting the form closes the modal and refreshes the candidates', async () => {
    apiDataSetCandidateService.listCandidates
      .mockResolvedValueOnce(testCandidates)
      .mockResolvedValue([testCandidates[1]]);
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetCreateModal
        publicationId="publication-id"
        releaseVersionId="release-id"
        onSubmit={handleSubmit}
      />,
    );

    expect(await screen.findByText('Create API data set')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Create API data set' }),
    );

    await user.selectOptions(
      await screen.findByLabelText('Data set'),
      testCandidates[0].releaseFileId,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith({
        releaseFileId: testCandidates[0].releaseFileId,
      });
    });

    await waitFor(() => {
      expect(apiDataSetCandidateService.listCandidates).toHaveBeenCalledTimes(
        2,
      );
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    await user.click(
      screen.getByRole('button', { name: 'Create API data set' }),
    );

    const modal = within(await screen.findByRole('dialog'));
    await user.selectOptions(
      await screen.findByLabelText('Data set'),
      testCandidates[1].releaseFileId,
    );
    expect(
      modal.queryByRole('heading', { name: 'Creating API data set' }),
    ).not.toBeInTheDocument();
  });

  function render(
    ui: ReactNode,
    options?: {
      history: History;
    },
  ) {
    const { history = createMemoryHistory() } = options ?? {};

    return baseRender(<Router history={history}>{ui}</Router>);
  }
});
