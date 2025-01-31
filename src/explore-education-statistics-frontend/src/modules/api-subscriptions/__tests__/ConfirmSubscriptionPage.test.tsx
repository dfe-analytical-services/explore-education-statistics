import render from '@common-test/render';
import createAxiosErrorMock from '@common-test/createAxiosErrorMock';
import _apiDataSetService from '@frontend/services/apiDataSetService';
import _apiNotificationService from '@frontend/services/apiNotificationService';
import { testApiDataSet } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import ConfirmSubscriptionPage from '@frontend/modules/api-subscriptions/ConfirmSubscriptionPage';
import { screen, waitFor } from '@testing-library/react';

jest.mock('@frontend/services/apiDataSetService');
const apiDataSetService = _apiDataSetService as jest.Mocked<
  typeof _apiDataSetService
>;

jest.mock('@frontend/services/apiNotificationService');
const apiNotificationService = _apiNotificationService as jest.Mocked<
  typeof _apiNotificationService
>;

describe('ConfirmSubscriptionPage', () => {
  beforeEach(() => {
    apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
  });

  test('renders correctly', () => {
    render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    expect(
      screen.getByRole('heading', { name: 'Test title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Please confirm you wish to subscribe to notifications about this data set.',
      ),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Confirm' })).toBeInTheDocument();
  });

  test('successful confirmation', async () => {
    const { user } = render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        apiNotificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith('api-data-set-id', 'test-token');
    });

    expect(
      screen.getByRole('heading', { name: 'Subscribed' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles `ApiPendingSubscriptionAlreadyExpired` errors', async () => {
    const error = createAxiosErrorMock({
      data: {
        errors: [{ code: 'ApiPendingSubscriptionAlreadyExpired' }],
      },
    });
    apiNotificationService.confirmPendingSubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        apiNotificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith('api-data-set-id', 'test-token');
    });

    expect(
      screen.getByRole('heading', { name: 'Verification failed' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/Your subscription verification token has expired/),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Subscribed' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles `ApiVerifiedSubscriptionAlreadyExists` errors', async () => {
    const error = createAxiosErrorMock({
      data: {
        errors: [{ code: 'ApiVerifiedSubscriptionAlreadyExists' }],
      },
    });
    apiNotificationService.confirmPendingSubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        apiNotificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith('api-data-set-id', 'test-token');
    });

    expect(
      screen.getByRole('heading', { name: 'Verification failed' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/You are already subscribed to this data set/),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Subscribed' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles `AuthorizationTokenInvalid` errors', async () => {
    const error = createAxiosErrorMock({
      data: {
        errors: [{ code: 'AuthorizationTokenInvalid' }],
      },
    });
    apiNotificationService.confirmPendingSubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        apiNotificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith('api-data-set-id', 'test-token');
    });

    expect(
      screen.getByRole('heading', { name: 'Verification failed' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/The authorization token is invalid/),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Subscribed' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles 404 errors', async () => {
    const error = createAxiosErrorMock({
      data: '',
      status: 404,
    });
    apiNotificationService.confirmPendingSubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        apiNotificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith('api-data-set-id', 'test-token');
    });

    expect(
      screen.getByRole('heading', { name: 'Verification failed' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('There is no pending subscription for this data set.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Subscribed' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles other errors', async () => {
    const error = createAxiosErrorMock({
      data: '',
    });
    apiNotificationService.confirmPendingSubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmSubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        apiNotificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith('api-data-set-id', 'test-token');
    });

    expect(
      screen.getByRole('heading', { name: 'Verification failed' }),
    ).toBeInTheDocument();

    expect(screen.getByText(/There has been a problem/)).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Subscribed' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });
});
