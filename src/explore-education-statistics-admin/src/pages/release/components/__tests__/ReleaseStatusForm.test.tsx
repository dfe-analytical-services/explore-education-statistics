import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import { Release } from '@admin/services/releaseService';
import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import { render, screen, waitFor, within } from '@testing-library/react';
import { format } from 'date-fns';
import React from 'react';
import ReleaseStatusForm, {
  ReleaseStatusFormValues,
} from '@admin/pages/release/components/ReleaseStatusForm';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('ReleaseStatusForm', () => {
  const testRelease: Release = {
    id: 'release-1',
    slug: 'release-1-slug',
    approvalStatus: 'Draft',
    latestRelease: false,
    live: false,
    amendment: false,
    releaseName: 'Release 1',
    publicationId: 'publication-1',
    publicationTitle: 'Publication 1',
    publicationSlug: 'publication-1-slug',
    timePeriodCoverage: { value: 'W51', label: 'Week 51' },
    title: 'Release Title',
    type: {
      id: 'type-1',
      title: 'Official Statistics',
    },
    contact: {
      id: 'contact-1',
      teamName: 'Test name',
      teamEmail: 'test@test.com',
      contactName: 'Test contact name',
      contactTelNo: '1111 1111 1111',
    },
    previousVersionId: '',
    preReleaseAccessList: '',
  };

  const testStatusPermissions: ReleaseStatusPermissions = {
    canMarkApproved: true,
    canMarkDraft: true,
    canMarkHigherLevelReview: true,
  };

  test('renders with all status options enabled', () => {
    render(
      <ReleaseStatusForm
        release={testRelease}
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
        release={testRelease}
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
        release={testRelease}
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
        release={testRelease}
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
        release={{
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
    expect(statuses.getByLabelText('Ready for higher review')).toBeChecked();
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

      render(
        <ReleaseStatusForm
          release={testRelease}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        latestInternalReleaseNote: '',
        approvalStatus: 'Draft',
      };

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('submits successfully with updated values', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={testRelease}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByLabelText('Ready for higher review'));
      await userEvent.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await userEvent.type(nextReleaseDate.getByLabelText('Month'), '5');
      await userEvent.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        latestInternalReleaseNote: 'Test release note',
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
      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'HigherLevelReview',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      userEvent.click(screen.getByLabelText('Internal note'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByRole('link', { name: 'Enter an internal note' }),
        ).toHaveAttribute(
          'href',
          '#releaseStatusForm-latestInternalReleaseNote',
        );
      });
    });

    test('fails to submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'HigherLevelReview',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', { name: 'Enter an internal note' }),
        ).toBeInTheDocument();

        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('submits successfully with updated values', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'HigherLevelReview',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByLabelText('In draft'));
      await userEvent.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await userEvent.type(nextReleaseDate.getByLabelText('Month'), '5');
      await userEvent.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        latestInternalReleaseNote: 'Test release note',
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
          release={{
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
        statuses.getByLabelText('Ready for higher review'),
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
          release={{
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
        statuses.getByLabelText('Ready for higher review'),
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

    test('shows error message when internal note is empty', async () => {
      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      userEvent.click(screen.getByLabelText('Internal note'));
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByRole('link', { name: 'Enter an internal note' }),
        ).toHaveAttribute(
          'href',
          '#releaseStatusForm-latestInternalReleaseNote',
        );
      });
    });

    test('shows error message when no publishing method selected', async () => {
      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      // Focus the field above before tabbing through the options
      userEvent.click(screen.getByLabelText('Internal note'));
      userEvent.tab();
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByRole('link', { name: 'Choose when to publish' }),
        ).toHaveAttribute('href', '#releaseStatusForm-publishMethod');
      });
    });

    test('shows error message when no publish date', async () => {
      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      userEvent.click(screen.getByLabelText('On a specific date'));
      userEvent.tab();
      userEvent.tab();
      userEvent.tab();
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByRole('link', { name: 'Enter a valid publish date' }),
        ).toHaveAttribute('href', '#releaseStatusForm-publishScheduled');
      });
    });

    test('fails to submit with invalid values', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', { name: 'Enter an internal note' }),
        ).toBeInTheDocument();
        expect(
          screen.getByRole('link', { name: 'Choose when to publish' }),
        ).toBeInTheDocument();

        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('shows mapped server validation error from failed `onSubmit`', async () => {
      const handleSubmit = jest.fn().mockImplementation(() => {
        throw createServerValidationErrorMock([
          'PUBLIC_META_GUIDANCE_REQUIRED',
        ]);
      });

      render(
        <ReleaseStatusForm
          release={testRelease}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', {
            name:
              'All public data guidance must be populated before the release can be approved',
          }),
        ).toBeInTheDocument();
      });
    });

    test('submits successfully with updated values and Draft status', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            publishScheduled: format(new Date(), 'yyyy-MM-dd'),
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByLabelText('In draft'));
      await userEvent.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await userEvent.type(nextReleaseDate.getByLabelText('Month'), '5');
      await userEvent.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        latestInternalReleaseNote: 'Test release note',
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

    test('submits successfully with updated values and publish date', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await userEvent.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      userEvent.click(screen.getByLabelText('On a specific date'));

      const publishDate = within(
        screen.getByRole('group', { name: 'Publish date' }),
      );

      await userEvent.type(publishDate.getByLabelText('Day'), '10');
      await userEvent.type(publishDate.getByLabelText('Month'), '10');
      await userEvent.type(publishDate.getByLabelText('Year'), '2022');

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await userEvent.type(nextReleaseDate.getByLabelText('Month'), '5');
      await userEvent.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        latestInternalReleaseNote: 'Test release note',
        approvalStatus: 'Approved',
        publishScheduled: new Date('2022-10-10'),
        publishMethod: 'Scheduled',
        nextReleaseDate: {
          month: 5,
          year: 2021,
        },
      };

      await waitFor(() => {
        const modal = within(screen.getByRole('dialog'));
        expect(modal.getByRole('heading')).toHaveTextContent(
          'Confirm publish date',
        );
        userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      });
      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('submits successfully with update values and immediate publish', async () => {
      const handleSubmit = jest.fn();

      render(
        <ReleaseStatusForm
          release={{
            ...testRelease,
            approvalStatus: 'Approved',
          }}
          statusPermissions={testStatusPermissions}
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      await userEvent.type(
        screen.getByLabelText('Internal note'),
        'Test release note',
      );

      userEvent.click(screen.getByLabelText('Immediately'));

      const nextReleaseDate = within(
        screen.getByRole('group', { name: 'Next release expected (optional)' }),
      );

      await userEvent.type(nextReleaseDate.getByLabelText('Month'), '5');
      await userEvent.type(nextReleaseDate.getByLabelText('Year'), '2021');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Update status' }));

      const expectedValues: ReleaseStatusFormValues = {
        latestInternalReleaseNote: 'Test release note',
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
  });
});
