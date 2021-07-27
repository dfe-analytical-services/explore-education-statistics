import MethodologyStatusForm, {
  FormValues,
} from '@admin/pages/methodology/edit-methodology/status/components/MethodolodyStatusForm';
import { Release } from '@admin/services/releaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';
import { BasicMethodology } from 'src/services/methodologyService';

describe('MethodologyStatusForm', () => {
  const testUnpublishedReleases: Release[] = [
    {
      id: 'test-release-1',
      title: 'Test Release 1',
    } as Release,
    {
      id: 'test-release-2',
      title: 'Test Release 2',
    } as Release,
  ];

  test('renders the form with draft initial values', () => {
    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Draft',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('Edit methodology status')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();

    const statuses = within(
      screen.getByRole('group', { name: 'Status' }),
    ).getAllByRole('radio');

    expect(statuses).toHaveLength(2);
    expect(statuses[0]).toHaveAttribute('value', 'Draft');
    expect(statuses[0]).toBeEnabled();
    expect(statuses[0]).toBeChecked();
    expect(screen.getByLabelText('In draft')).toBeInTheDocument();

    expect(statuses[1]).toHaveAttribute('value', 'Approved');
    expect(statuses[1]).toBeEnabled();
    expect(
      screen.getByLabelText('Approved for publication'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('group', { name: 'When to publish' }),
    ).not.toBeInTheDocument();
  });

  test('renders the form with approved status initial values', () => {
    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Approved',
            latestInternalReleaseNote: 'The latest note',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    const approvedRadio = screen.getByLabelText('Approved for publication');
    expect(approvedRadio).toBeChecked();

    const noteField = screen.getByLabelText('Internal note');
    expect(noteField).toHaveValue('The latest note');

    expect(
      screen.queryByRole('group', { name: 'When to publish' }),
    ).toBeInTheDocument();
  });

  test('renders the form with publishing strategy initial values', () => {
    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Approved',
            latestInternalReleaseNote: 'The latest note',
            publishingStrategy: 'WithRelease',
            withReleaseId: 'test-release-2',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        unPublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('When to publish')).toBeInTheDocument();

    const publishStatuses = within(
      screen.getByRole('group', { name: 'When to publish' }),
    ).getAllByRole('radio');

    expect(publishStatuses[0]).toBeChecked();
    expect(publishStatuses[0]).toBeEnabled();
    expect(
      screen.getByLabelText('With a specific release'),
    ).toBeInTheDocument();

    expect(publishStatuses[1]).not.toBeChecked();
    expect(publishStatuses[1]).toBeEnabled();
    expect(screen.getByLabelText('Immediately')).toBeInTheDocument();

    const releaseSelect = screen.getByLabelText('Select release');
    expect(releaseSelect.children.length).toBe(3);
    expect(releaseSelect).toHaveValue('test-release-2');
  });

  test('Shows validation error if internal note is empty and status is approved', async () => {
    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Draft',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Approved for publication'));
    userEvent.click(screen.getByLabelText('Internal note'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter an internal note' }),
      ).toHaveAttribute(
        'href',
        '#methodologyStatusForm-latestInternalReleaseNote',
      );
    });
  });

  test('Shows validation error if a release is not selected when publish strategy is with release', async () => {
    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Approved',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        unPublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('With a specific release'));
    userEvent.click(screen.getByLabelText('Select release'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Select a release' }),
      ).toHaveAttribute('href', '#methodologyStatusForm-withReleaseId');
    });
  });

  test('Fails to submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Approved',
            publishingStrategy: 'WithRelease',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        unPublishedReleases={testUnpublishedReleases}
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
        screen.getByRole('link', { name: 'Select a release' }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('Successfully submits with valid values', async () => {
    const handleSubmit = jest.fn();

    render(
      <MethodologyStatusForm
        methodologySummary={
          {
            status: 'Approved',
            publishingStrategy: 'WithRelease',
          } as BasicMethodology
        }
        showWithRelease // EES-2163 flag
        unPublishedReleases={testUnpublishedReleases}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Internal note'),
      'Test release note',
    );

    await userEvent.selectOptions(screen.getByLabelText('Select release'), [
      'test-release-1',
    ]);

    userEvent.click(screen.getByRole('button', { name: 'Update status' }));

    const expectedValues: FormValues = {
      latestInternalReleaseNote: 'Test release note',
      status: 'Approved',
      publishingStrategy: 'WithRelease',
      withReleaseId: 'test-release-1',
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(
        expectedValues,
        expect.anything(),
      );
    });
  });
});
