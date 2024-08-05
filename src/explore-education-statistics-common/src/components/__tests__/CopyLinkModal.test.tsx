import CopyLinkModal from '@common/components/CopyLinkModal';
import render from '@common-test/render';
import React from 'react';
import { screen, within } from '@testing-library/react';

describe('CopyLinkModal', () => {
  const testUrl = 'http://test.com/1#test-heading';

  test('renders the copy link button', () => {
    render(<CopyLinkModal url={testUrl} />);

    expect(
      screen.getByRole('button', { name: 'Copy link to the clipboard' }),
    ).toBeInTheDocument();
  });

  test('shows the modal when the button is clicked', async () => {
    const { user } = render(<CopyLinkModal url={testUrl} />);

    await user.click(
      screen.getByRole('button', { name: 'Copy link to the clipboard' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Copy link to the clipboard' }),
    ).toBeInTheDocument();
    expect(modal.getByLabelText('URL')).toHaveValue(testUrl);
    expect(modal.getByRole('button', { name: 'Copy' })).toBeInTheDocument();
  });
});
