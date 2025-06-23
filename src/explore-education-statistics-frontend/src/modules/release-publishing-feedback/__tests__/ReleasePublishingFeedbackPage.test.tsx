import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';
import _releasePublishingFeedbackService from '@frontend/services/releasePublishingFeedbackService';
import ReleasePublishingFeedbackPage from '@frontend/modules/release-publishing-feedback/ReleasePublishingFeedbackPage';

jest.mock('@frontend/services/releasePublishingFeedbackService');
const releasePublishingFeedbackService =
  _releasePublishingFeedbackService as jest.Mocked<
    typeof _releasePublishingFeedbackService
  >;

describe('ReleasePublishingFeedbackPage', () => {
  test('renders', async () => {
    render(
      <ReleasePublishingFeedbackPage
        emailToken="test-token"
        initialResponse="VerySatisfied"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Your recent publishing experience',
      }),
    ).toBeInTheDocument();

    const responseOptions = screen.getAllByRole('radio');

    expect(responseOptions).toHaveLength(5);
    expect(responseOptions[0]).toEqual(
      screen.getByLabelText('Extremely satisfied'),
    );
    expect(responseOptions[0]).toHaveAttribute('value', 'ExtremelySatisfied');
    expect(responseOptions[0]).not.toBeChecked();
    expect(responseOptions[0]).toBeEnabled();

    expect(responseOptions[1]).toEqual(screen.getByLabelText('Very satisfied'));
    expect(responseOptions[1]).toHaveAttribute('value', 'VerySatisfied');
    expect(responseOptions[1]).toBeChecked();
    expect(responseOptions[1]).toBeEnabled();

    expect(responseOptions[2]).toEqual(screen.getByLabelText('Satisfied'));
    expect(responseOptions[2]).toHaveAttribute('value', 'Satisfied');
    expect(responseOptions[2]).not.toBeChecked();
    expect(responseOptions[2]).toBeEnabled();

    expect(responseOptions[3]).toEqual(
      screen.getByLabelText('Slightly dissatisfied'),
    );
    expect(responseOptions[3]).toHaveAttribute('value', 'SlightlyDissatisfied');
    expect(responseOptions[3]).not.toBeChecked();
    expect(responseOptions[3]).toBeEnabled();

    expect(responseOptions[4]).toEqual(
      screen.getByLabelText('Not satisfied at all'),
    );
    expect(responseOptions[4]).toHaveAttribute('value', 'NotSatisfiedAtAll');
    expect(responseOptions[4]).not.toBeChecked();
    expect(responseOptions[4]).toBeEnabled();
  });

  test('submits feedback without additional feedback', async () => {
    releasePublishingFeedbackService.sendFeedback.mockResolvedValue();

    const { user } = render(
      <ReleasePublishingFeedbackPage
        emailToken="test-token"
        initialResponse="VerySatisfied"
      />,
    );

    await user.click(screen.getByLabelText('Extremely satisfied'));
    await user.click(screen.getByRole('button', { name: 'Send' }));

    await waitFor(() => {
      expect(
        releasePublishingFeedbackService.sendFeedback,
      ).toHaveBeenCalledWith({
        token: 'test-token',
        response: 'ExtremelySatisfied',
      });
    });

    expect(
      screen.getByRole('heading', { name: 'Feedback received' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Thank you.')).toBeInTheDocument();
  });

  test('submits feedback with additional feedback', async () => {
    releasePublishingFeedbackService.sendFeedback.mockResolvedValue();

    const { user } = render(
      <ReleasePublishingFeedbackPage
        emailToken="test-token"
        initialResponse="VerySatisfied"
      />,
    );

    await user.click(screen.getByLabelText('Extremely satisfied'));
    await user.type(
      screen.getByLabelText('Additional feedback (optional)'),
      'Some additional feedback.',
    );
    await user.click(screen.getByRole('button', { name: 'Send' }));

    await waitFor(() => {
      expect(
        releasePublishingFeedbackService.sendFeedback,
      ).toHaveBeenCalledWith({
        token: 'test-token',
        response: 'ExtremelySatisfied',
        additionalFeedback: 'Some additional feedback.',
      });
    });

    expect(
      screen.getByRole('heading', { name: 'Feedback received' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Thank you.')).toBeInTheDocument();
  });
});
