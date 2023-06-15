import CopyLinkButton from '@common/components/CopyLinkButton';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('CopyLinkButton', () => {
  const testUrl = 'http://test.com/1#test-heading';

  test('renders the copy link button', () => {
    render(<CopyLinkButton url={testUrl} />);

    expect(
      screen.getByRole('button', { name: 'Copy link to the clipboard' }),
    ).toBeInTheDocument();
  });

  test('shows the modal when the button is clicked', () => {
    render(<CopyLinkButton url={testUrl} />);

    userEvent.click(
      screen.getByRole('button', { name: 'Copy link to the clipboard' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Copy link to the clipboard' }),
    ).toBeInTheDocument();
    expect(modal.getByLabelText('Url')).toHaveValue(testUrl);
    expect(modal.getByRole('button', { name: 'Copy' })).toBeInTheDocument();
  });

  test('copies the url to the clipboard and shows a message when the button is clicked', async () => {
    render(<CopyLinkButton url={testUrl} />);

    userEvent.click(
      screen.getByRole('button', { name: 'Copy link to the clipboard' }),
    );

    const modal = within(screen.getByRole('dialog'));
    userEvent.click(modal.getByRole('button', { name: 'Copy' }));

    await waitFor(() => {
      expect(
        modal.getByText('Link copied to the clipboard.'),
      ).toBeInTheDocument();
    });

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith(testUrl);
  });
});
