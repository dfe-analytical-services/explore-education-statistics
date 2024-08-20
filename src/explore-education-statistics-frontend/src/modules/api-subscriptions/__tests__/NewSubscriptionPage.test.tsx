import render from '@common-test/render';
import _dataSetFileService from '@frontend/services/dataSetFileService';
import _apiNotificationService from '@frontend/services/apiNotificationService';
import { testDataSetWithApi } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import NewSubscriptionPage from '@frontend/modules/api-subscriptions/NewSubscriptionPage';
import { screen, waitFor } from '@testing-library/react';

jest.mock('@frontend/services/dataSetFileService');
const dataSetFileService = _dataSetFileService as jest.Mocked<
  typeof _dataSetFileService
>;

jest.mock('@frontend/services/apiNotificationService');
const apiNotificationService = _apiNotificationService as jest.Mocked<
  typeof _apiNotificationService
>;

describe('NewSubscriptionPage', () => {
  beforeEach(() => {
    dataSetFileService.getDataSetFile.mockResolvedValue(testDataSetWithApi);
  });

  test('renders correctly', () => {
    render(<NewSubscriptionPage dataSetFile={testDataSetWithApi} />);

    expect(
      screen.getByRole('heading', { name: 'Data set 1' }),
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText('Enter your email address'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Subscribe' }),
    ).toBeInTheDocument();
  });

  test('shows validation error if submit without email', async () => {
    const { user } = render(
      <NewSubscriptionPage dataSetFile={testDataSetWithApi} />,
    );
    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    expect(
      await screen.findByText('Email is required', {
        selector: '#subscriptionForm-email-error',
      }),
    ).toBeInTheDocument();
  });

  test('submits successfully', async () => {
    const { user } = render(
      <NewSubscriptionPage dataSetFile={testDataSetWithApi} />,
    );

    await user.type(
      screen.getByLabelText('Enter your email address'),
      'user@test.com',
    );

    expect(
      apiNotificationService.requestPendingSubscription,
    ).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    await waitFor(() =>
      expect(
        apiNotificationService.requestPendingSubscription,
      ).toHaveBeenCalledTimes(1),
    );

    expect(
      apiNotificationService.requestPendingSubscription,
    ).toHaveBeenLastCalledWith({
      email: 'user@test.com',
      dataSetId: 'data-set-file-id',
      dataSetTitle: 'Data set 1',
    });

    expect(
      screen.getByRole('heading', { name: 'Subscribed' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByLabelText('Enter your email address'),
    ).not.toBeInTheDocument();
  });
});
