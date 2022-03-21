import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import userEvent from '@testing-library/user-event';
import userService from '@admin/services/userService';
import PublicationInviteNewUsersTab from '../PublicationInviteNewUsersTab';

jest.mock('@admin/services/userService');

const publication: BasicPublicationDetails = {
  id: 'publication-id',
  title: 'Publication title',
  slug: 'publication-slug',
  legacyReleases: [],
  themeId: 'theme-id',
  topicId: 'topic-id',
};

const releases: ReleaseSummary[] = [
  {
    id: 'release1-id',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic Year',
    },
    title: 'Academic Year 2000/01',
    releaseName: '2000',
    type: 'AdHocStatistics',
    publishScheduled: '2001-01-01',
    latestInternalReleaseNote: 'release1-release-note',
    approvalStatus: 'Draft',
    yearTitle: '2000/01',
  },
  {
    id: 'release2-id',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic Year',
    },
    title: 'Academic Year 2001/02',
    releaseName: '2001',
    type: 'AdHocStatistics',
    publishScheduled: '2002-01-01',
    latestInternalReleaseNote: 'release2-release-note',
    approvalStatus: 'Draft',
    yearTitle: '2001/02',
  },
  {
    id: 'release3-id',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic Year',
    },
    title: 'Academic Year 2002/03',
    releaseName: '2002',
    type: 'AdHocStatistics',
    publishScheduled: '2003-01-01',
    latestInternalReleaseNote: 'release3-release-note',
    approvalStatus: 'Draft',
    yearTitle: '2002/03',
  },
];

describe('PublicationInviteNewUsersTab', () => {
  test('submits correct request', async () => {
    render(
      <PublicationInviteNewUsersTab
        publication={publication}
        releases={releases}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit Publication title'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('Academic Year 2000/01'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('Academic Year 2001/02'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('Academic Year 2002/03'),
      ).toBeInTheDocument();
    });

    const emailInput = screen.getByLabelText('Enter an email address');
    await userEvent.type(emailInput, 'test@test.com');

    const checkboxes = screen.getAllByLabelText(
      /Academic Year /,
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

  test('no email entered', async () => {
    render(
      <PublicationInviteNewUsersTab
        publication={publication}
        releases={releases}
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

  test('invite to no releases', async () => {
    render(
      <PublicationInviteNewUsersTab
        publication={publication}
        releases={releases}
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
      /Academic Year /,
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
