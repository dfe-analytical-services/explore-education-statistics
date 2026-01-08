import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';
import _publicationService from '@common/services/publicationService';
import _notificationService from '@frontend/services/notificationService';
import testPublicationTitle from '@frontend/modules/subscriptions/__tests__/__data__/testPublicationData';
import { testPendingSubscription } from '@frontend/modules/subscriptions/__tests__/__data__/testSubscriptionData';
import NewSubscriptionPage from '@frontend/modules/subscriptions/NewSubscriptionPage';

jest.mock('@common/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@frontend/services/notificationService');
const notificationService = _notificationService as jest.Mocked<
  typeof _notificationService
>;

describe('NewSubscriptionPage', () => {
  beforeEach(() => {
    publicationService.getPublicationTitle.mockResolvedValue(
      testPublicationTitle,
    );
  });

  test('renders', async () => {
    render(
      <NewSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
      />,
    );

    expect(
      screen.getByRole('heading', { name: testPublicationTitle.title }),
    ).toBeInTheDocument();
  });

  test('permits form input and calls the service upon submission', async () => {
    notificationService.requestPendingSubscription.mockResolvedValue(
      testPendingSubscription,
    );

    const { user } = render(
      <NewSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
      />,
    );

    await user.type(
      screen.getByRole('textbox', { name: 'Enter your email address' }),
      'test@test.com',
    );

    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    await waitFor(() => {
      expect(
        notificationService.requestPendingSubscription,
      ).toHaveBeenCalledWith({
        email: 'test@test.com',
        slug: 'test-publication-slug',
        title: testPublicationTitle.title,
        id: testPublicationTitle.publicationId,
      });
    });
  });

  test('validates that an email has been entered', async () => {
    notificationService.requestPendingSubscription.mockResolvedValue(
      testPendingSubscription,
    );

    const { user } = render(
      <NewSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    expect(screen.getByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Email is required',
      }),
    ).toBeInTheDocument();
  });

  test.each(['no-at-symbol', 'two-@@-symbols', 'spaces @gmail.com'])(
    'validates emails on submission',
    async userInput => {
      notificationService.requestPendingSubscription.mockResolvedValue(
        testPendingSubscription,
      );

      const { user } = render(
        <NewSubscriptionPage
          publicationSlug="test-publication-slug"
          publicationTitle={testPublicationTitle.title}
          publicationId={testPublicationTitle.publicationId}
        />,
      );

      await user.type(
        screen.getByRole('textbox', { name: 'Enter your email address' }),
        userInput,
      );

      await user.click(screen.getByRole('button', { name: 'Subscribe' }));

      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', {
          name: 'Enter a valid email',
        }),
      ).toBeInTheDocument();
    },
  );

  test('allows form submission once validation errors are solved', async () => {
    notificationService.requestPendingSubscription.mockResolvedValue(
      testPendingSubscription,
    );

    const { user } = render(
      <NewSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    expect(screen.getByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Email is required',
      }),
    ).toBeInTheDocument();

    await user.type(
      screen.getByRole('textbox', { name: 'Enter your email address' }),
      'test@test.com',
    );

    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    await waitFor(() => {
      expect(
        notificationService.requestPendingSubscription,
      ).toHaveBeenCalledWith({
        email: 'test@test.com',
        slug: 'test-publication-slug',
        title: testPublicationTitle.title,
        id: testPublicationTitle.publicationId,
      });
    });
  });

  test('shows the confirmation message upon successful API response', async () => {
    notificationService.requestPendingSubscription.mockResolvedValue(
      testPendingSubscription,
    );

    const { user } = render(
      <NewSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
      />,
    );

    await user.type(
      screen.getByRole('textbox', { name: 'Enter your email address' }),
      'test@test.com',
    );

    await user.click(screen.getByRole('button', { name: 'Subscribe' }));

    await waitFor(() => {
      expect(
        notificationService.requestPendingSubscription,
      ).toHaveBeenCalledWith({
        email: 'test@test.com',
        slug: 'test-publication-slug',
        title: testPublicationTitle.title,
        id: testPublicationTitle.publicationId,
      });
    });

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Subscribed' }));
    expect(
      screen.getByText(
        'Thank you. Check your email to confirm your subscription.',
      ),
    ).toBeInTheDocument();
  });
});
