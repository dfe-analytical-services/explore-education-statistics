import DeleteDraftVersionButton, {
  DeleteDraftVersionButtonProps,
} from '@admin/pages/release/data/components/DeleteDraftVersionButton';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import render from '@common-test/render';
import { screen, within, waitFor } from '@testing-library/react';

jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('DeleteDraftVersionButton', () => {
  const testDataSet: DeleteDraftVersionButtonProps['dataSet'] = {
    id: 'data-set-id',
    title: 'Data set title',
  };

  const testDataSetVersion: DeleteDraftVersionButtonProps['dataSetVersion'] = {
    id: 'data-set-version-id',
    version: '1.0',
  };

  test('renders correctly with data set version details', () => {
    render(
      <DeleteDraftVersionButton
        dataSet={testDataSet}
        dataSetVersion={testDataSetVersion}
      >
        Delete draft version
      </DeleteDraftVersionButton>,
    );

    expect(
      screen.getByRole('button', { name: 'Delete draft version' }),
    ).toBeInTheDocument();
  });

  test('clicking the `Confirm` button opens a confirmation modal', async () => {
    const { user } = render(
      <DeleteDraftVersionButton
        dataSet={testDataSet}
        dataSetVersion={testDataSetVersion}
      >
        Delete draft version
      </DeleteDraftVersionButton>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Delete draft version' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'Remove draft version' }),
    ).toBeInTheDocument();

    expect(modal.getByTestId('confirm-text')).toHaveTextContent(
      'Confirm that you want to delete the draft version 1.0 for API data set: Data set title',
    );
  });

  test('confirming the deletion calls the correct service', async () => {
    const { user } = render(
      <DeleteDraftVersionButton
        dataSet={testDataSet}
        dataSetVersion={testDataSetVersion}
      >
        Delete draft version
      </DeleteDraftVersionButton>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Delete draft version' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(apiDataSetVersionService.deleteVersion).not.toHaveBeenCalled();

    await user.click(modal.getByRole('button', { name: 'Confirm' }));

    expect(apiDataSetVersionService.deleteVersion).toHaveBeenCalledWith(
      testDataSetVersion.id,
    );
  });

  test('confirming the deletion closes the modal', async () => {
    const { user } = render(
      <DeleteDraftVersionButton
        dataSet={testDataSet}
        dataSetVersion={testDataSetVersion}
      >
        Delete draft version
      </DeleteDraftVersionButton>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Delete draft version' }),
    );

    const modal = within(screen.getByRole('dialog'));

    await user.click(modal.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
