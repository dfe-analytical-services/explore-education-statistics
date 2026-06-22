import userEvent from '@testing-library/user-event';
import { render, screen } from '@testing-library/react';
import React from 'react';
import InvitePublicationDrafterForm from '../InvitePublicationDrafterForm';

describe('InvitePublicationDrafterForm', () => {
  test('renders the component correctly', () => {
    renderComponent();

    expect(screen.getByLabelText('Enter an email address')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Invite drafter',
      }),
    ).toBeInTheDocument();
  });

  test('submits correct request when inviting a new drafter', async () => {
    const { onInviteDrafter } = renderComponent();

    await userEvent.type(
      screen.getByLabelText('Enter an email address'),
      'test@test.com',
    );

    await userEvent.click(
      screen.getByRole('button', {
        name: 'Invite drafter',
      }),
    );

    expect(onInviteDrafter).toHaveBeenCalledTimes(1);

    expect(onInviteDrafter).toHaveBeenCalledWith('test@test.com');
  });

  test('shows an error if no email is entered', async () => {
    renderComponent();

    await userEvent.click(
      screen.getByRole('button', {
        name: 'Invite drafter',
      }),
    );

    expect(
      screen.getByText('Enter an email address', {
        selector: '#inviteDrafterForm-email-error',
      }),
    ).toBeInTheDocument();
  });
});

function renderComponent() {
  const onInviteDrafter = jest.fn().mockResolvedValue(undefined);

  render(
    <InvitePublicationDrafterForm
      isLoading={false}
      onInviteDrafter={onInviteDrafter}
    />,
  );

  return {
    onInviteDrafter,
  };
}
