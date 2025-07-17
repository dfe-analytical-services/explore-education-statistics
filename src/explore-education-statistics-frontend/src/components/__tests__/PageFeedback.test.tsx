import PageFeedback from '@frontend/components/PageFeedback';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import userEvent from '@testing-library/user-event';
import _feedbackService from '@frontend/services/pageFeedbackService';
import { PageFeedbackRequest } from '@common/services/types/pageFeedback';

jest.mock('@frontend/services/pageFeedbackService');

const feedbackService = jest.mocked(_feedbackService);

describe('Feedback', () => {
  test('renders initial state correctly', () => {
    render(<PageFeedback />);

    expect(screen.getByText('Is this page useful?')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Yes this page is useful' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'No this page is not useful' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Report a problem with this page' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Help us improve Explore education statistics'),
    ).not.toBeVisible();

    expect(
      screen.queryByText('Thank you for your feedback'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly and submits when "Yes" is selected', async () => {
    const user = userEvent.setup();

    render(<PageFeedback />);

    await user.click(
      screen.getByRole('button', { name: 'Yes this page is useful' }),
    );

    await waitFor(() => {
      expect(feedbackService.sendFeedback).toHaveBeenCalledWith(
        expect.objectContaining<Omit<PageFeedbackRequest, 'userAgent'>>({
          url: '/',
          response: 'Useful',
        }),
      );
    });

    expect(screen.getByText('Thank you for your feedback')).toBeInTheDocument();
  });

  test('renders correctly when "No" is selected', async () => {
    const user = userEvent.setup();

    render(<PageFeedback />);

    expect(
      screen.queryByLabelText('What were you doing?'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByLabelText('What were you hoping to achieve?'),
    ).not.toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'No this page is not useful' }),
    );

    expect(screen.getByLabelText('What were you doing?')).toBeInTheDocument();
    expect(
      screen.getByLabelText('What were you hoping to achieve?'),
    ).toBeInTheDocument();
  });

  test('submits correctly when "No" is selected', async () => {
    const user = userEvent.setup();

    render(<PageFeedback />);

    await user.click(
      screen.getByRole('button', { name: 'No this page is not useful' }),
    );

    const contextTextArea = screen.getByLabelText('What were you doing?');
    const intentTextArea = screen.getByLabelText(
      'What were you hoping to achieve?',
    );

    await user.type(contextTextArea, 'test context');
    await user.type(intentTextArea, 'test intent');
    await user.click(screen.getByRole('button', { name: 'Send' }));

    await waitFor(() => {
      expect(feedbackService.sendFeedback).toHaveBeenCalledWith(
        expect.objectContaining<Omit<PageFeedbackRequest, 'userAgent'>>({
          url: '/',
          response: 'NotUseful',
          context: 'test context',
          intent: 'test intent',
        }),
      );
    });

    expect(screen.getByText('Thank you for your feedback')).toBeInTheDocument();
  });

  test('renders correctly when "Report a problem with this page" is selected', async () => {
    const user = userEvent.setup();

    render(<PageFeedback />);

    expect(
      screen.queryByLabelText('What were you doing?'),
    ).not.toBeInTheDocument();
    expect(screen.queryByLabelText('What went wrong?')).not.toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Report a problem with this page' }),
    );

    expect(screen.getByLabelText('What were you doing?')).toBeInTheDocument();
    expect(screen.getByLabelText('What went wrong?')).toBeInTheDocument();
  });

  test('submits correctly when "Report a problem with this page" is selected', async () => {
    const user = userEvent.setup();

    render(<PageFeedback />);

    await user.click(
      screen.getByRole('button', { name: 'Report a problem with this page' }),
    );

    const contextTextArea = screen.getByLabelText('What were you doing?');
    const issueTextArea = screen.getByLabelText('What went wrong?');

    await user.type(contextTextArea, 'test context');
    await user.type(issueTextArea, 'test issue');
    await user.click(screen.getByRole('button', { name: 'Send' }));

    await waitFor(() => {
      expect(feedbackService.sendFeedback).toHaveBeenCalledWith(
        expect.objectContaining<Omit<PageFeedbackRequest, 'userAgent'>>({
          url: '/',
          response: 'ProblemEncountered',
          context: 'test context',
          issue: 'test issue',
        }),
      );
    });

    expect(screen.getByText('Thank you for your feedback')).toBeInTheDocument();
  });
});
