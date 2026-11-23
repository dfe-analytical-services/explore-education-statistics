import BackToDataFilesLink from '@admin/pages/release/data/components/BackToDataFilesLink';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';

describe('BackToDataFilesLink', () => {
  test('goes back straight away when there are no unsaved changes', async () => {
    const onBack = jest.fn();

    const { user } = render(<BackToDataFilesLink onBack={onBack} />);

    await user.click(
      screen.getByRole('button', { name: 'Back to data files' }),
    );

    expect(onBack).toHaveBeenCalledTimes(1);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test('asks to confirm when there are unsaved changes', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataFilesLink hasUnsavedChanges onBack={onBack} />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Back to data files' }),
    );

    expect(
      await screen.findByText('You have unsaved changes'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'If you go back to the data files, your changes will be lost.',
      ),
    ).toBeInTheDocument();
    expect(onBack).not.toHaveBeenCalled();
  });

  test('discarding the changes goes back', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataFilesLink hasUnsavedChanges onBack={onBack} />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Back to data files' }),
    );
    await user.click(
      await screen.findByRole('button', { name: 'Discard changes' }),
    );

    await waitFor(() => expect(onBack).toHaveBeenCalledTimes(1));
  });

  test('keeping editing closes the warning without going back', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataFilesLink hasUnsavedChanges onBack={onBack} />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Back to data files' }),
    );
    await user.click(
      await screen.findByRole('button', { name: 'Keep editing' }),
    );

    await waitFor(() =>
      expect(
        screen.queryByText('You have unsaved changes'),
      ).not.toBeInTheDocument(),
    );
    expect(onBack).not.toHaveBeenCalled();
  });

  test('keeping editing with the keyboard returns focus to the back link', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataFilesLink hasUnsavedChanges onBack={onBack} />,
    );

    const backLink = screen.getByRole('button', {
      name: 'Back to data files',
    });

    await user.tab();
    expect(backLink).toHaveFocus();
    await user.keyboard('{Enter}');

    const keepEditing = await screen.findByRole('button', {
      name: 'Keep editing',
    });
    keepEditing.focus();
    await user.keyboard('{Enter}');

    await waitFor(() =>
      expect(
        screen.queryByText('You have unsaved changes'),
      ).not.toBeInTheDocument(),
    );
    await waitFor(() => expect(backLink).toHaveFocus());
    expect(onBack).not.toHaveBeenCalled();
  });

  test('closing the warning with Escape returns focus to the back link', async () => {
    const { user } = render(
      <BackToDataFilesLink hasUnsavedChanges onBack={jest.fn()} />,
    );

    const backLink = screen.getByRole('button', {
      name: 'Back to data files',
    });

    await user.tab();
    await user.keyboard('{Enter}');
    expect(
      await screen.findByText('You have unsaved changes'),
    ).toBeInTheDocument();

    await user.keyboard('{Escape}');

    await waitFor(() =>
      expect(
        screen.queryByText('You have unsaved changes'),
      ).not.toBeInTheDocument(),
    );
    await waitFor(() => expect(backLink).toHaveFocus());
  });
});
