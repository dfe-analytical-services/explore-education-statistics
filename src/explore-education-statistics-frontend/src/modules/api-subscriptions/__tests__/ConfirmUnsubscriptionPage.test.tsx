import render from '@common-test/render';
import createAxiosErrorMock from '@common-test/createAxiosErrorMock';
import _apiDataSetService from '@frontend/services/apiDataSetService';
import _apiNotificationService from '@frontend/services/apiNotificationService';
import { testApiDataSet } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import ConfirmUnsubscriptionPage from '@frontend/modules/api-subscriptions/ConfirmUnsubscriptionPage';
import { screen, waitFor } from '@testing-library/react';

jest.mock('@frontend/services/apiDataSetService');
const apiDataSetService = _apiDataSetService as jest.Mocked<
  typeof _apiDataSetService
>;

jest.mock('@frontend/services/apiNotificationService');
const apiNotificationService = _apiNotificationService as jest.Mocked<
  typeof _apiNotificationService
>;

describe('ConfirmUnsubscriptionPage', () => {
  beforeEach(() => {
    apiDataSetService.getDataSet.mockResolvedValue(testApiDataSet);
  });

  test('renders correctly', () => {
    render(
      <ConfirmUnsubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    expect(
      screen.getByRole('heading', { name: 'Test title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Please confirm you wish to unsubscribe from notifications about this data set.',
      ),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Confirm' })).toBeInTheDocument();
  });

  test('successful confirmation', async () => {
    const { user } = render(
      <ConfirmUnsubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(apiNotificationService.confirmUnsubscription).toHaveBeenCalledWith(
        'api-data-set-id',
        'test-token',
      );
    });

    expect(
      screen.getByRole('heading', { name: 'Unsubscribed' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles `ApiSubscriptionHasNotBeenVerified` errors', async () => {
    const error = createAxiosErrorMock({
      data: {
        errors: [{ code: 'ApiSubscriptionHasNotBeenVerified' }],
      },
    });
    apiNotificationService.confirmUnsubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmUnsubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(apiNotificationService.confirmUnsubscription).toHaveBeenCalledWith(
        'api-data-set-id',
        'test-token',
      );
    });

    expect(
      screen.getByRole('heading', { name: 'Unsubscribe failed' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/The subscription has not been verified/),
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
    apiNotificationService.confirmUnsubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmUnsubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(apiNotificationService.confirmUnsubscription).toHaveBeenCalledWith(
        'api-data-set-id',
        'test-token',
      );
    });

    expect(
      screen.getByRole('heading', { name: 'Unsubscribe failed' }),
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
    const error = createAxiosErrorMock({ data: '', status: 404 });
    apiNotificationService.confirmUnsubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmUnsubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(apiNotificationService.confirmUnsubscription).toHaveBeenCalledWith(
        'api-data-set-id',
        'test-token',
      );
    });

    expect(
      screen.getByRole('heading', { name: 'Unsubscribe failed' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('You are not subscribed to this data set.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Subscribed' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
  });

  test('handles other errors', async () => {
    const error = createAxiosErrorMock({ data: '', status: 500 });
    apiNotificationService.confirmUnsubscription.mockRejectedValue(error);
    const { user } = render(
      <ConfirmUnsubscriptionPage dataSet={testApiDataSet} token="test-token" />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(apiNotificationService.confirmUnsubscription).toHaveBeenCalledWith(
        'api-data-set-id',
        'test-token',
      );
    });

    expect(
      screen.getByRole('heading', { name: 'Unsubscribe failed' }),
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
