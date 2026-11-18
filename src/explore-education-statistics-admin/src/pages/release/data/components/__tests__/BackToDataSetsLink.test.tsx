import BackToDataSetsLink from '@admin/pages/release/data/components/BackToDataSetsLink';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';

describe('BackToDataSetsLink', () => {
  test('goes back straight away when there are no unsaved changes', async () => {
    const onBack = jest.fn();

    const { user } = render(<BackToDataSetsLink onBack={onBack} />);

    await user.click(screen.getByRole('button', { name: 'Back to data sets' }));

    expect(onBack).toHaveBeenCalledTimes(1);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test('asks to confirm when there are unsaved changes', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataSetsLink hasUnsavedChanges onBack={onBack} />,
    );

    await user.click(screen.getByRole('button', { name: 'Back to data sets' }));

    expect(
      await screen.findByText('You have unsaved changes'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'If you go back to the data sets, your changes will be lost.',
      ),
    ).toBeInTheDocument();
    expect(onBack).not.toHaveBeenCalled();
  });

  test('discarding the changes goes back', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataSetsLink hasUnsavedChanges onBack={onBack} />,
    );

    await user.click(screen.getByRole('button', { name: 'Back to data sets' }));
    await user.click(
      await screen.findByRole('button', { name: 'Discard changes' }),
    );

    await waitFor(() => expect(onBack).toHaveBeenCalledTimes(1));
  });

  test('keeping editing closes the warning without going back', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <BackToDataSetsLink hasUnsavedChanges onBack={onBack} />,
    );

    await user.click(screen.getByRole('button', { name: 'Back to data sets' }));
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
});
