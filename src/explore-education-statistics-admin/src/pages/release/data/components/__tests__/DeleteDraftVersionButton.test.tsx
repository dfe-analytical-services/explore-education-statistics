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
        Remove draft version
      </DeleteDraftVersionButton>,
    );

    expect(
      screen.getByRole('button', { name: 'Remove draft version' }),
    ).toBeInTheDocument();
  });

  test('clicking the `Remove` button opens a confirmation modal', async () => {
    const { user } = render(
      <DeleteDraftVersionButton
        dataSet={testDataSet}
        dataSetVersion={testDataSetVersion}
      >
        Remove draft version
      </DeleteDraftVersionButton>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Remove draft version' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', {
        name: 'Remove this draft API data set version',
      }),
    ).toBeInTheDocument();

    expect(
      modal.getByText(
        'Are you sure you want to remove the selected API data set version?',
      ),
    ).toBeInTheDocument();
    expect(modal.getByText('Data set title')).toBeInTheDocument();
  });

  test('confirming the deletion calls the correct service', async () => {
    const { user } = render(
      <DeleteDraftVersionButton
        dataSet={testDataSet}
        dataSetVersion={testDataSetVersion}
      >
        Remove draft version
      </DeleteDraftVersionButton>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Remove draft version' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(apiDataSetVersionService.deleteVersion).not.toHaveBeenCalled();

    await user.click(
      modal.getByRole('button', { name: 'Remove this API data set version' }),
    );

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
        Remove draft version
      </DeleteDraftVersionButton>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Remove draft version' }),
    );

    const modal = within(screen.getByRole('dialog'));

    await user.click(
      modal.getByRole('button', { name: 'Remove this API data set version' }),
    );

    await waitFor(() => {
      expect(
        screen.queryByText('Remove this API data set version'),
      ).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
