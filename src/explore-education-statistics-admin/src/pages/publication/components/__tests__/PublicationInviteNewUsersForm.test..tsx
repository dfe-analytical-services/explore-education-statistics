import { testReleaseSummaries } from '@admin/pages/publication/__data__/testReleases';
import { Publication } from '@admin/services/publicationService';
import userService from '@admin/services/userService';
import PublicationInviteNewUsersForm from '@admin/pages/publication/components/PublicationInviteNewUsersForm';
import userEvent from '@testing-library/user-event';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/userService');

const publication: Publication = {
  id: 'publication-id',
  title: 'Publication title',
  summary: 'Publication summary',
  slug: 'publication-slug',
  theme: { id: 'theme-id', title: 'Test theme title' },
  topic: { id: 'topic-id', title: 'Test topic title' },
};

const currentReleaseId = 'release-1-id';

describe('PublicationInviteNewUsersForm', () => {
  test('submits correct request', async () => {
    render(
      <PublicationInviteNewUsersForm
        publication={publication}
        releases={testReleaseSummaries}
        releaseId={currentReleaseId}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit Publication title'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('Academic year 2000/01'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('Academic year 2001/02'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('Academic year 2002/03'),
      ).toBeInTheDocument();
    });

    const emailInput = screen.getByLabelText('Enter an email address');
    await userEvent.type(emailInput, 'test@test.com');

    const checkboxes = screen.getAllByLabelText(
      /Academic year /,
    ) as HTMLInputElement[];
    expect(checkboxes).toHaveLength(3);

    expect(checkboxes[0].checked).toBe(true);
    expect(checkboxes[0]).toHaveAttribute('value', 'release1-id');

    expect(checkboxes[1].checked).toBe(true);
    expect(checkboxes[1]).toHaveAttribute('value', 'release2-id');

    expect(checkboxes[2].checked).toBe(true);
    expect(checkboxes[2]).toHaveAttribute('value', 'release3-id');

    userEvent.click(checkboxes[1]);

    await waitFor(() => {
      expect(checkboxes[1].checked).toBe(false);
    });

    expect(userService.inviteContributor).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Invite user' }));

    await waitFor(() => {
      expect(userService.inviteContributor).toHaveBeenCalledTimes(1);
      expect(
        userService.inviteContributor,
      ).toHaveBeenCalledWith('test@test.com', 'publication-id', [
        'release1-id',
        'release3-id',
      ]);
    });
  });

  test('shows an error if no email is entered', async () => {
    render(
      <PublicationInviteNewUsersForm
        publication={publication}
        releases={testReleaseSummaries}
        releaseId={currentReleaseId}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit Publication title'),
      ).toBeInTheDocument();
    });

    expect(userService.inviteContributor).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Invite user' }));

    await waitFor(() => {
      expect(userService.inviteContributor).toHaveBeenCalledTimes(0);
      expect(
        screen.getByText('Enter an email address', {
          selector: '#email-error',
        }),
      );
    });
  });

  test('shows an error if no releases are selected', async () => {
    render(
      <PublicationInviteNewUsersForm
        publication={publication}
        releases={testReleaseSummaries}
        releaseId={currentReleaseId}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit Publication title'),
      ).toBeInTheDocument();
    });

    const emailInput = screen.getByLabelText('Enter an email address');
    await userEvent.type(emailInput, 'test@test.com');

    const checkboxes = screen.getAllByLabelText(
      /Academic year /,
    ) as HTMLInputElement[];
    expect(checkboxes).toHaveLength(3);

    userEvent.click(checkboxes[0]);
    userEvent.click(checkboxes[1]);
    userEvent.click(checkboxes[2]);

    await waitFor(() => {
      expect(checkboxes[0].checked).toBe(false);
      expect(checkboxes[1].checked).toBe(false);
      expect(checkboxes[2].checked).toBe(false);
    });

    expect(userService.inviteContributor).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Invite user' }));

    await waitFor(() => {
      expect(userService.inviteContributor).toHaveBeenCalledTimes(0);
      expect(
        screen.getByText('Select at least one release', {
          selector: '#releaseIds-error',
        }),
      );
    });
  });
});
