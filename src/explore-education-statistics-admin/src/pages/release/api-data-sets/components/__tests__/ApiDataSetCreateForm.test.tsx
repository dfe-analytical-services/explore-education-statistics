import ApiDataSetCreateForm, {
  ApiDataSetCreateFormProps,
} from '@admin/pages/release/api-data-sets/components/ApiDataSetCreateForm';
import { ApiDataSetCandidate } from '@admin/services/apiDataSetCandidateService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';

describe('ApiDataSetCreateForm', () => {
  const testCandidates: ApiDataSetCandidate[] = [
    {
      releaseFileId: 'release-file-id-1',
      title: 'Test data set 1',
    },
    {
      releaseFileId: 'release-file-id-2',
      title: 'Test data set 2',
    },
    {
      releaseFileId: 'release-file-id-3',
      title: 'Test data set 2',
    },
  ];

  test('renders form correctly', () => {
    render(
      <ApiDataSetCreateForm
        dataSetCandidates={testCandidates}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const options = within(screen.getByLabelText('Data set')).getAllByRole(
      'option',
    );

    expect(options).toHaveLength(4);

    expect(options[0]).toHaveValue('');
    expect(options[0]).toHaveTextContent('Choose a data set');

    expect(options[1]).toHaveValue(testCandidates[0].releaseFileId);
    expect(options[1]).toHaveTextContent(testCandidates[0].title);

    expect(options[2]).toHaveValue(testCandidates[1].releaseFileId);
    expect(options[2]).toHaveTextContent(testCandidates[1].title);

    expect(options[3]).toHaveValue(testCandidates[2].releaseFileId);
    expect(options[3]).toHaveTextContent(testCandidates[2].title);
  });

  test('shows validation error when no data set selected', async () => {
    const { user } = render(
      <ApiDataSetCreateForm
        dataSetCandidates={testCandidates}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('Data set'));
    await user.tab();

    expect(
      await screen.findByText('Choose a data set', {
        selector: '#apiDataSetCreateForm-releaseFileId-error',
      }),
    ).toBeInTheDocument();
  });

  test('shows validation error if submitted without selecting data set', async () => {
    const { user } = render(
      <ApiDataSetCreateForm
        dataSetCandidates={testCandidates}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    );

    expect(
      await screen.findByText('Choose a data set', {
        selector: '#apiDataSetCreateForm-releaseFileId-error',
      }),
    ).toBeInTheDocument();
  });

  test('submitting form successfully calls `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetCreateForm
        dataSetCandidates={testCandidates}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.selectOptions(
      screen.getByLabelText('Data set'),
      'Test data set 2',
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith<
        Parameters<ApiDataSetCreateFormProps['onSubmit']>
      >({
        releaseFileId: testCandidates[1].releaseFileId,
      });
    });
  });

  test('submitting form with validation error does not call `onSubmit` handler', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ApiDataSetCreateForm
        dataSetCandidates={testCandidates}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    );

    expect(
      await screen.findByText('Choose a data set', {
        selector: '#apiDataSetCreateForm-releaseFileId-error',
      }),
    ).toBeInTheDocument();

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });
});
