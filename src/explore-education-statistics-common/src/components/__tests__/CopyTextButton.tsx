import CopyTextButton from '@common/components/CopyTextButton';
import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';

describe('CopyTextButton', () => {
  const testText = 'http://test.com/1#test-heading';

  test('copies the text to the clipboard and shows a message when the button is clicked', async () => {
    const { user } = render(<CopyTextButton text={testText} />);

    await user.click(screen.getByRole('button', { name: 'Copy' }));

    await waitFor(() => {
      expect(
        screen.getByText('Text copied to the clipboard.'),
      ).toBeInTheDocument();
    });

    const copiedText = await window.navigator.clipboard.readText();
    expect(copiedText).toEqual(testText);
  });
});
