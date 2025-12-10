import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';
import _publicationService from '@common/services/publicationService';
import _notificationService from '@frontend/services/notificationService';
import testPublicationTitle from '@frontend/modules/subscriptions/__tests__/__data__/testPublicationData';
import testActiveSubscription from '@frontend/modules/subscriptions/__tests__/__data__/testSubscriptionData';
import ConfirmUnsubscriptionPage from '@frontend/modules/subscriptions/ConfirmUnsubscriptionPage';

jest.mock('@common/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@frontend/services/notificationService');
const notificationService = _notificationService as jest.Mocked<
  typeof _notificationService
>;

describe('ConfirmUnsubscriptionPage', () => {
  beforeEach(() => {
    publicationService.getPublicationTitle.mockResolvedValue(
      testPublicationTitle,
    );
  });

  test('renders', async () => {
    render(
      <ConfirmUnsubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    expect(
      screen.getByRole('heading', { name: testPublicationTitle.title }),
    ).toBeInTheDocument();
  });

  test('calls the service when Confirm is clicked', async () => {
    notificationService.confirmUnsubscription.mockResolvedValue(
      testActiveSubscription,
    );

    const { user } = render(
      <ConfirmUnsubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(notificationService.confirmUnsubscription).toHaveBeenCalledWith(
        testPublicationTitle.publicationId,
        'test-token',
      );
    });
  });

  test('shows the confirmation message upon successful API response', async () => {
    notificationService.confirmUnsubscription.mockResolvedValue(
      testActiveSubscription,
    );

    const { user } = render(
      <ConfirmUnsubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(notificationService.confirmUnsubscription).toHaveBeenCalledWith(
        testPublicationTitle.publicationId,
        'test-token',
      );
    });

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText(
        /You have successfully unsubscribed from these updates./,
      ),
    ).toBeInTheDocument();
  });
});
