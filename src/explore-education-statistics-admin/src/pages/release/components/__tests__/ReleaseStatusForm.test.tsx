import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import { ReleaseVersionChecklistErrorCode } from '@admin/services/releaseVersionService';
import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import { screen, waitFor, within } from '@testing-library/react';
import { format } from 'date-fns';
import React from 'react';
import ReleaseStatusForm, {
  ReleaseStatusFormValues,
} from '@admin/pages/release/components/ReleaseStatusForm';
import noop from 'lodash/noop';
import render from '@common-test/render';

describe('ReleaseStatusForm', () => {
  const testStatusPermissions: ReleaseStatusPermissions = {
    canMarkApproved: true,
    canMarkDraft: true,
    canMarkHigherLevelReview: true,
  };

  test('renders with all status options enabled', () => {
    render(
      <ReleaseStatusForm
        releaseVersion={testRelease}
        statusPermissions={testStatusPermissions}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const statuses = within(
      screen.getByRole('group', { name: 'Status' }),
    ).getAllByRole('radio');

    expect(statuses).toHaveLength(3);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeChecked();
    expect(statuses[0]).toBeEnabled();

    expect(statuses[1]).toHaveAttribute('value', 'HigherLevelReview');
    expect(statuses[1]).not.toBeChecked();
    expect(statuses[1]).toBeEnabled();

    expect(statuses[2]).toHaveAttribute('value', 'Approved');
    expect(statuses[2]).not.toBeChecked();
    expect(statuses[2]).toBeEnabled();
  });

  test('renders with Approved status option disabled', () => {
    render(
      <ReleaseStatusForm
        releaseVersion={testRelease}
        statusPermissions={{
          ...testStatusPermissions,
          canMarkApproved: false,
        }}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const statuses = within(
      screen.getByRole('group', { name: 'Status' }),
    ).getAllByRole('radio');

    expect(statuses).toHaveLength(3);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeChecked();
    expect(statuses[0]).toBeEnabled();

    expect(statuses[1]).toHaveAttribute('value', 'HigherLevelReview');
    expect(statuses[1]).not.toBeChecked();
    expect(statuses[1]).toBeEnabled();

    expect(statuses[2]).toHaveAttribute('value', 'Approved');
    expect(statuses[2]).not.toBeChecked();
    expect(statuses[2]).toBeDisabled();
  });

  test('renders with HigherLevelReview and Approved status options disabled', () => {
    render(
      <ReleaseStatusForm
        releaseVersion={testRelease}
        statusPermissions={{
          ...testStatusPermissions,
          canMarkHigherLevelReview: false,
          canMarkApproved: false,
        }}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const statuses = within(
      screen.getByRole('group', { name: 'Status' }),
    ).getAllByRole('radio');

    expect(statuses).toHaveLength(3);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeChecked();
    expect(statuses[0]).toBeEnabled();

    expect(statuses[1]).toHaveAttribute('value', 'HigherLevelReview');
    expect(statuses[1]).not.toBeChecked();
    expect(statuses[1]).toBeDisabled();

    expect(statuses[2]).toHaveAttribute('value', 'Approved');
    expect(statuses[2]).not.toBeChecked();
    expect(statuses[2]).toBeDisabled();
  });

  test('renders with all options disabled', () => {
    render(
      <ReleaseStatusForm
        releaseVersion={testRelease}
        statusPermissions={{
          canMarkDraft: false,
          canMarkHigherLevelReview: false,
          canMarkApproved: false,
        }}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const statuses = within(
      screen.getByRole('group', { name: 'Status' }),
    ).getAllByRole('radio');

    expect(statuses).toHaveLength(3);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeChecked();
    expect(statuses[0]).toBeDisabled();

    expect(statuses[1]).toHaveAttribute('value', 'HigherLevelReview');
    expect(statuses[1]).not.toBeChecked();
    expect(statuses[1]).toBeDisabled();

    expect(statuses[2]).toHaveAttribute('value', 'Approved');
    expect(statuses[2]).not.toBeChecked();
    expect(statuses[2]).toBeDisabled();
  });

  test('renders correctly with initial values', () => {
    render(
      <ReleaseStatusForm
        releaseVersion={{
          ...testRelease,
          approvalStatus: 'HigherLevelReview',
          nextReleaseDate: {
            month: 10,
            year: 2021,
          },
          latestInternalReleaseNote: 'Test release note',
        }}
        statusPermissions={testStatusPermissions}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const statuses = within(screen.getByRole('group', { name: 'Status' }));

    expect(statuses.getByLabelText('In draft')).not.toBeChecked();
    expect(
      statuses.getByLabelText(
        'Ready for higher review (this will notify approvers)',
      ),
    ).toBeChecked();
    expect(
      statuses.getByLabelText('Approved for publication'),
    ).not.toBeChecked();

    expect(
      screen.queryByRole('group', { name: 'When to publish' }),
    ).not.toBeInTheDocument();

    expect(screen.getByLabelText('Internal note')).toHaveValue(
      'Test release note',
    );

    const nextReleaseDate = within(
      screen.getByRole('group', { name: 'Next release expected (optional)' }),
    );

    expect(nextReleaseDate.getByLabelText('Month')).toHaveValue(10);
    expect(nextReleaseDate.getByLabelText('Year')).toHaveValue(2021);
  });

  describe('in Draft', () => {
    test('submits successfully without changing values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={testRelease}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        approvalStatus: 'Draft',
        internalReleaseNote: '',
        notifySubscribers: undefined,
        publishMethod: undefined,
        publishScheduled: undefined,
        updatePublishedDate: undefined,
      };

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('submits successfully with updated values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={testRelease}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(
        screen.getByLabelText(
          'Ready for higher review (this will notify approvers)',
        ),
      );
      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await user.type(nextReleaseDate.getByLabelText('Month'), '5');
      await user.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        internalReleaseNote: 'Test release note',
        approvalStatus: 'HigherLevelReview',
        nextReleaseDate: {
          month: 5,
          year: 2021,
        },
      };

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });
  });

  describe('in Higher Level Review', () => {
    test('shows error message when internal note is empty', async () => {
      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'HigherLevelReview',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('Internal note'));
      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toHaveAttribute('href', '#releaseStatusForm-internalReleaseNote');
    });

    test('fails to submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'HigherLevelReview',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('submits successfully with updated values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'HigherLevelReview',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(screen.getByLabelText('In draft'));
      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await user.type(nextReleaseDate.getByLabelText('Month'), '5');
      await user.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        internalReleaseNote: 'Test release note',
        approvalStatus: 'Draft',
        nextReleaseDate: {
          month: 5,
          year: 2021,
        },
      };

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });
  });

  describe('in Approved', () => {
    test('renders correctly with no published date', () => {
      render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      const statuses = within(screen.getByRole('group', { name: 'Status' }));

      expect(statuses.getByLabelText('In draft')).not.toBeChecked();
      expect(
        statuses.getByLabelText(
          'Ready for higher review (this will notify approvers)',
        ),
      ).not.toBeChecked();

      expect(statuses.getByLabelText('Approved for publication')).toBeChecked();

      const whenToPublish = within(
        screen.getByRole('group', { name: 'When to publish' }),
      );

      expect(
        whenToPublish.getByLabelText('On a specific date'),
      ).not.toBeChecked();
      expect(whenToPublish.getByLabelText('Immediately')).not.toBeChecked();
    });

    test('renders correctly with published date', () => {
      render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
            publishScheduled: '2020-12-15',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      const statuses = within(screen.getByRole('group', { name: 'Status' }));

      expect(statuses.getByLabelText('In draft')).not.toBeChecked();
      expect(
        statuses.getByLabelText(
          'Ready for higher review (this will notify approvers)',
        ),
      ).not.toBeChecked();
      expect(statuses.getByLabelText('Approved for publication')).toBeChecked();

      const whenToPublish = within(
        screen.getByRole('group', { name: 'When to publish' }),
      );

      expect(whenToPublish.getByLabelText('On a specific date')).toBeChecked();

      const publishDate = within(
        whenToPublish.getByRole('group', {
          name: 'Publish date',
        }),
      );

      expect(publishDate.getByLabelText('Day')).toHaveValue(15);
      expect(publishDate.getByLabelText('Month')).toHaveValue(12);
      expect(publishDate.getByLabelText('Year')).toHaveValue(2020);
    });

    test('shows pre-release warnings when pre-release users have been added', () => {
      render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
            preReleaseUsersOrInvitesAdded: true,
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByText(
          'Pre-release users will have access to a preview of the release 24 hours before the scheduled publish date.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.getByText(
          'Pre-release users will not have access to a preview of the release if it is published immediately.',
        ),
      ).toBeInTheDocument();
    });

    test('does not show pre-release warnings when pre-release users have not been added', () => {
      render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
            preReleaseUsersOrInvitesAdded: false,
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(
        screen.queryByText(
          'Pre-release users will have access to a preview of the release 24 hours before the scheduled publish date.',
        ),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByText(
          'Pre-release users will not have access to a preview of the release if it is published immediately.',
        ),
      ).not.toBeInTheDocument();
    });

    test('shows error message when internal note is empty', async () => {
      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('Internal note'));
      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toHaveAttribute('href', '#releaseStatusForm-internalReleaseNote');
    });

    test('shows error message when no publishing method selected', async () => {
      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      // Focus the field above before tabbing through the options
      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Choose when to publish' }),
      ).toHaveAttribute('href', '#releaseStatusForm-publishMethod');
    });

    test('shows error message when no publish date', async () => {
      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('On a specific date'));
      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Enter a valid publish date' }),
      ).toHaveAttribute('href', '#releaseStatusForm-publishScheduled');
    });

    test('fails to submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Choose when to publish' }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test.each(ReleaseVersionChecklistErrorCode)(
      'shows checklist error message `%s` after failed submit',
      async checklistError => {
        const handleSubmit = jest.fn().mockImplementation(() => {
          throw createServerValidationErrorMock([
            { code: checklistError, message: '' },
          ]);
        });

        const { user } = render(
          <ReleaseStatusForm
            releaseVersion={testRelease}
            statusPermissions={testStatusPermissions}
            onCancel={noop}
            onSubmit={handleSubmit}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Update status' }));

        await waitFor(() => {
          expect(screen.getByText('There is a problem')).toBeInTheDocument();
        });

        const errorLink = screen.getByRole('link', {
          name: 'Resolve all errors in the publishing checklist',
        });

        expect(errorLink).toHaveAttribute(
          'href',
          '#releaseStatusForm-approvalStatus',
        );
      },
    );

    test('shows generic server validation error message for `approvalStatus` after failed submit', async () => {
      const handleSubmit = jest.fn().mockImplementation(() => {
        throw createServerValidationErrorMock([
          { code: 'UnexpectedError', message: '' },
        ]);
      });

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={testRelease}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      const errorLink = screen.getByRole('link', {
        name: 'There was a problem updating the approval status of this release',
      });

      expect(errorLink).toHaveAttribute(
        'href',
        '#releaseStatusForm-approvalStatus',
      );
    });

    test('submits successfully with updated values and Draft status', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            publishScheduled: format(new Date(), 'yyyy-MM-dd'),
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(screen.getByLabelText('In draft'));
      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await user.type(nextReleaseDate.getByLabelText('Month'), '5');
      await user.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        internalReleaseNote: 'Test release note',
        approvalStatus: 'Draft',
        nextReleaseDate: {
          month: 5,
          year: 2021,
        },
      };

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('shows confirmation modal when submitting with valid values and publish date', async () => {
      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      await user.click(screen.getByLabelText('On a specific date'));

      const publishDate = within(
        screen.getByRole('group', { name: 'Publish date' }),
      );

      const nextYear = new Date().getFullYear() + 1;

      await user.type(publishDate.getByLabelText('Day'), '10');
      await user.type(publishDate.getByLabelText('Month'), '10');
      await user.type(publishDate.getByLabelText('Year'), nextYear.toString());

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('Confirm publish date')).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('heading')).toHaveTextContent(
        'Confirm publish date',
      );
    });

    test('does not show confirmation modal when submitting invalid values with valid publish date', async () => {
      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('On a specific date'));

      const publishDate = within(
        screen.getByRole('group', { name: 'Publish date' }),
      );

      const nextYear = new Date().getFullYear() + 1;

      await user.type(publishDate.getByLabelText('Day'), '10');
      await user.type(publishDate.getByLabelText('Month'), '10');
      await user.type(publishDate.getByLabelText('Year'), nextYear.toString());

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    test('shows error modal when submitted with publish date that could not be scheduled', async () => {
      const handleSubmit = jest.fn().mockImplementation(() => {
        throw createServerValidationErrorMock([
          { code: 'PublishDateCannotBeScheduled', message: '' },
        ]);
      });

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      await user.click(screen.getByLabelText('On a specific date'));

      const publishDate = within(
        screen.getByRole('group', { name: 'Publish date' }),
      );

      const nextYear = new Date().getFullYear() + 1;

      await user.type(publishDate.getByLabelText('Day'), '10');
      await user.type(publishDate.getByLabelText('Month'), '10');
      await user.type(publishDate.getByLabelText('Year'), nextYear.toString());

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('Confirm publish date')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          screen.getByText('Publish date cannot be scheduled'),
        ).toBeInTheDocument();
      });

      const errorModal = within(screen.getByRole('dialog'));

      expect(errorModal.getByRole('heading')).toHaveTextContent(
        'Publish date cannot be scheduled',
      );

      await user.click(screen.getByRole('button', { name: 'Back to form' }));

      expect(
        screen.getByRole('link', {
          name: 'Release must be scheduled at least one day in advance of the publishing day',
        }),
      ).toHaveAttribute('href', '#releaseStatusForm-publishScheduled');
    });

    test('submits successfully with updated values and publish date', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      await user.click(screen.getByLabelText('On a specific date'));

      const publishDate = within(
        screen.getByRole('group', { name: 'Publish date' }),
      );

      const nextYear = new Date().getFullYear() + 1;

      await user.type(publishDate.getByLabelText('Day'), '10');
      await user.type(publishDate.getByLabelText('Month'), '10');
      await user.type(publishDate.getByLabelText('Year'), nextYear.toString());

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await user.type(nextReleaseDate.getByLabelText('Month'), '5');
      await user.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        internalReleaseNote: 'Test release note',
        approvalStatus: 'Approved',
        publishScheduled: new Date(`${nextYear}-10-10`),
        publishMethod: 'Scheduled',
        nextReleaseDate: {
          month: 5,
          year: 2021,
        },
      };

      await waitFor(() => {
        expect(screen.getByText('Confirm publish date')).toBeInTheDocument();
      });

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('heading')).toHaveTextContent(
        'Confirm publish date',
      );

      await user.click(screen.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('submits successfully with updated values and immediate publish', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ReleaseStatusForm
          releaseVersion={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      await user.click(screen.getByLabelText('Immediately'));

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await user.type(nextReleaseDate.getByLabelText('Month'), '5');
      await user.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        internalReleaseNote: 'Test release note',
        approvalStatus: 'Approved',
        publishMethod: 'Immediate',
        nextReleaseDate: {
          month: 5,
          year: 2021,
        },
      };

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    describe('amendments', () => {
      test('renders with `notifySubscribers` and `updatePublishedDate` options', async () => {
        const handleSubmit = jest.fn();

        render(
          <ReleaseStatusForm
            releaseVersion={{
              ...testRelease,
              approvalStatus: 'Approved',
              amendment: true,
              notifySubscribers: true,
              updatePublishedDate: true,
            }}
            statusPermissions={testStatusPermissions}
            onCancel={noop}
            onSubmit={handleSubmit}
          />,
        );

        await waitFor(() => {
          expect(
            screen.getByLabelText('Notify subscribers by email'),
          ).toBeChecked();
        });

        expect(screen.getByLabelText('Update published date')).toBeChecked();
      });

      test('renders default values for `notifySubscribers` and `updatePublishedDate` options when status is changed to Approved', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ReleaseStatusForm
            releaseVersion={{
              ...testRelease,
              approvalStatus: 'Approved',
              amendment: true,
              notifySubscribers: false,
              updatePublishedDate: true,
            }}
            statusPermissions={testStatusPermissions}
            onCancel={noop}
            onSubmit={handleSubmit}
          />,
        );

        // Begin with checkboxes in opposite states to their default values
        await waitFor(() => {
          expect(
            screen.getByLabelText('Notify subscribers by email'),
          ).not.toBeChecked();
        });

        expect(screen.getByLabelText('Update published date')).toBeChecked();

        // Toggle to Draft status and expect the options to not be rendered
        await user.click(screen.getByLabelText('In draft'));

        expect(
          screen.queryByLabelText('Notify subscribers by email'),
        ).not.toBeInTheDocument();

        expect(
          screen.queryByLabelText('Update published date'),
        ).not.toBeInTheDocument();

        // Toggle back to Approved and expect the options have their default values
        await user.click(screen.getByLabelText('Approved for publication'));

        expect(
          screen.getByLabelText('Notify subscribers by email'),
        ).toBeChecked();

        expect(
          screen.getByLabelText('Update published date'),
        ).not.toBeChecked();
      });

      test('shows warning message when `updatePublishedDate` is selected', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ReleaseStatusForm
            releaseVersion={{
              ...testRelease,
              approvalStatus: 'Approved',
              amendment: true,
              notifySubscribers: false,
              updatePublishedDate: false,
            }}
            statusPermissions={testStatusPermissions}
            onCancel={noop}
            onSubmit={handleSubmit}
          />,
        );

        await waitFor(() => {
          expect(
            screen.getByLabelText('Update published date'),
          ).not.toBeChecked();
        });

        expect(
          screen.getByText(
            "The release's published date in the public view will be updated once the publication is complete.",
          ),
        ).toBeInTheDocument();

        expect(screen.getByLabelText('Update published date')).toHaveAttribute(
          'aria-expanded',
          'false',
        );

        await user.click(screen.getByLabelText('Update published date'));

        expect(screen.getByLabelText('Update published date')).toHaveAttribute(
          'aria-expanded',
          'true',
        );
      });

      test('submits successfully with updated values', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ReleaseStatusForm
            releaseVersion={{
              ...testRelease,
              approvalStatus: 'Approved',
              amendment: true,
              notifySubscribers: true,
              updatePublishedDate: false,
            }}
            statusPermissions={testStatusPermissions}
            onCancel={noop}
            onSubmit={handleSubmit}
          />,
        );

        await user.type(
          screen.getByLabelText('Internal note'),
          'Test release note',
        );

        await user.click(screen.getByLabelText('Notify subscribers by email'));

        await user.click(screen.getByLabelText('Update published date'));

        await user.click(screen.getByLabelText('Immediately'));

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Update status' }));

        const expectedValues: ReleaseStatusFormValues = {
          internalReleaseNote: 'Test release note',
          approvalStatus: 'Approved',
          publishMethod: 'Immediate',
          notifySubscribers: false,
          updatePublishedDate: true,
        };

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
        });
      });
    });
  });
});
