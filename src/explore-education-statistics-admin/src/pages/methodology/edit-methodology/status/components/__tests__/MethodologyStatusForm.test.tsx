import MethodologyStatusForm, {
  MethodologyStatusFormValues,
} from '@admin/pages/methodology/edit-methodology/status/components/MethodologyStatusForm';
import { IdTitlePair } from '@admin/services/types/common';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { MethodologyStatusPermissions } from '@admin/services/permissionService';
import render from '@common-test/render';

describe('MethodologyStatusForm', () => {
  const testUnpublishedReleases: IdTitlePair[] = [
    {
      id: 'test-release-2',
      title: 'Test Release 2',
    },
    {
      id: 'test-release-1',
      title: 'Test Release 1',
    },
  ];

  const statusPermissions: MethodologyStatusPermissions = {
    canMarkDraft: true,
    canMarkHigherLevelReview: true,
    canMarkApproved: true,
  };

  test('renders the form with draft initial values', () => {
    render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Draft',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
        statusPermissions={statusPermissions}
      />,
    );

    expect(screen.getByText('Edit methodology status')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();

    const statusGroup = within(screen.getByRole('group', { name: 'Status' }));
    const statuses = statusGroup.getAllByRole('radio');

    expect(statuses).toHaveLength(3);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeEnabled();
    expect(statuses[0]).toBeChecked();
    expect(statuses[0]).toEqual(statusGroup.getByLabelText('In draft'));

    expect(statuses[1]).toHaveAttribute('value', 'HigherLevelReview');
    expect(statuses[1]).toBeEnabled();
    expect(statuses[1]).toEqual(
      statusGroup.getByLabelText(
        'Ready for higher review (this will notify approvers)',
      ),
    );

    expect(statuses[2]).toHaveAttribute('value', 'Approved');
    expect(statuses[2]).toBeEnabled();
    expect(statuses[2]).toEqual(
      statusGroup.getByLabelText('Approved for publication'),
    );

    expect(
      screen.queryByRole('group', { name: 'When to publish' }),
    ).not.toBeInTheDocument();
  });

  test('renders the form with disabled approval status radios', () => {
    render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Draft',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
        statusPermissions={{
          canMarkDraft: false,
          canMarkHigherLevelReview: false,
          canMarkApproved: false,
        }}
      />,
    );

    expect(screen.getByText('Edit methodology status')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();

    const statusGroup = within(screen.getByRole('group', { name: 'Status' }));
    const statuses = statusGroup.getAllByRole('radio');

    expect(statuses).toHaveLength(3);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeDisabled();

    expect(statuses[1]).toHaveAttribute('value', 'HigherLevelReview');
    expect(statuses[1]).toBeDisabled();

    expect(statuses[2]).toHaveAttribute('value', 'Approved');
    expect(statuses[2]).toBeDisabled();
  });

  test('renders the form with approved status initial values', () => {
    render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Approved',
            internalReleaseNote: 'Test release note',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
        statusPermissions={statusPermissions}
      />,
    );

    const approvedRadio = screen.getByLabelText('Approved for publication');
    expect(approvedRadio).toBeChecked();

    const noteField = screen.getByLabelText('Internal note');
    expect(noteField).toHaveValue('Test release note');

    expect(
      screen.queryByRole('group', { name: 'When to publish' }),
    ).toBeInTheDocument();
  });

  test('renders the form with publishing strategy initial values', () => {
    render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Approved',
            internalReleaseNote: 'Test release note',
            publishingStrategy: 'WithRelease',
            scheduledWithRelease: {
              id: 'test-release-2',
            },
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
        statusPermissions={statusPermissions}
      />,
    );

    expect(screen.getByText('When to publish')).toBeInTheDocument();

    const publishStatusGroup = within(
      screen.getByRole('group', { name: 'When to publish' }),
    );
    const publishStatuses = publishStatusGroup.getAllByRole('radio');

    expect(publishStatuses[0]).toBeChecked();
    expect(publishStatuses[0]).toBeEnabled();
    expect(publishStatuses[0]).toEqual(
      publishStatusGroup.getByLabelText('With a specific release'),
    );

    expect(publishStatuses[1]).not.toBeChecked();
    expect(publishStatuses[1]).toBeEnabled();
    expect(publishStatuses[1]).toEqual(
      publishStatusGroup.getByLabelText('Immediately'),
    );

    const releaseSelect = screen.getByLabelText('Select release');
    expect(releaseSelect.children.length).toBe(3);
    expect(releaseSelect.children[0]).toHaveTextContent('Choose a release');
    expect(releaseSelect.children[0]).toHaveValue('');
    expect(releaseSelect.children[1]).toHaveTextContent('Test Release 2');
    expect(releaseSelect.children[1]).toHaveValue('test-release-2');
    expect(releaseSelect.children[2]).toHaveTextContent('Test Release 1');
    expect(releaseSelect.children[2]).toHaveValue('test-release-1');

    expect(releaseSelect).toHaveDisplayValue('Test Release 2');
    expect(releaseSelect).toHaveValue('test-release-2');
  });

  test('shows validation error if internal note is empty and status is approved', async () => {
    const { user } = render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Draft',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
        statusPermissions={statusPermissions}
      />,
    );

    await user.click(screen.getByLabelText('Approved for publication'));

    await user.click(screen.getByRole('button', { name: 'Update status' }));

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toHaveAttribute(
        'href',
        '#methodologyStatusForm-latestInternalReleaseNote',
      );
    });
  });

  test('shows validation error if a release is not selected when publish strategy is with release', async () => {
    const { user } = render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Approved',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
        statusPermissions={statusPermissions}
      />,
    );

    await user.click(screen.getByLabelText('With a specific release'));

    await user.click(screen.getByRole('button', { name: 'Update status' }));

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Choose a release' }),
      ).toHaveAttribute('href', '#methodologyStatusForm-withReleaseId');
    });
  });

  test('fails to submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Approved',
            publishingStrategy: 'WithRelease',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={handleSubmit}
        statusPermissions={statusPermissions}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Update status' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Choose a release' }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('successfully submits with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <MethodologyStatusForm
        methodology={
          {
            status: 'Approved',
            publishingStrategy: 'WithRelease',
          } as MethodologyVersion
        }
        unpublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={handleSubmit}
        statusPermissions={statusPermissions}
      />,
    );

    await user.type(
      screen.getByLabelText('Internal note'),
      'Test release note',
    );

    await user.selectOptions(screen.getByLabelText('Select release'), [
      'test-release-1',
    ]);

    await user.click(screen.getByRole('button', { name: 'Update status' }));

    const expectedValues: MethodologyStatusFormValues = {
      latestInternalReleaseNote: 'Test release note',
      status: 'Approved',
      publishingStrategy: 'WithRelease',
      withReleaseId: 'test-release-1',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
    });
  });
});
