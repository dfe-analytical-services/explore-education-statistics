import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import React from 'react';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('CancelAmendmentModal', () => {
  test('renders with no methodologies supplied', async () => {
    render(
      <CancelAmendmentModal
        triggerButton={<button type="button">Open</button>}
        scheduledMethodologies={[]}
        onCancel={noop}
        onConfirm={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Open' }));

    await waitFor(() => {
      expect(
        screen.queryByText(
          'The following methodologies are scheduled to be published with this amended release:',
        ),
      ).not.toBeInTheDocument();
    });
  });

  test('renders with methodologies supplied', async () => {
    render(
      <CancelAmendmentModal
        triggerButton={<button type="button">Open</button>}
        scheduledMethodologies={[
          {
            id: 'methodology-1',
            title: 'Methodology 1',
          },
          {
            id: 'methodology-2',
            title: 'Methodology 2',
          },
        ]}
        onCancel={noop}
        onConfirm={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Open' }));

    await waitFor(() => {
      expect(
        screen.getByText(
          'The following methodologies are scheduled to be published with this amended release:',
        ),
      ).toBeInTheDocument();

      expect(screen.getByText('Methodology 1')).toBeInTheDocument();

      expect(screen.getByText('Methodology 2')).toBeInTheDocument();
    });
  });

  test('clicking confirm calls the correct callback', async () => {
    const onConfirm = jest.fn();

    render(
      <CancelAmendmentModal
        triggerButton={<button type="button">Open</button>}
        scheduledMethodologies={[]}
        onCancel={noop}
        onConfirm={onConfirm}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Open' }));

    await waitFor(() => {
      expect(screen.getByText('Confirm')).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(onConfirm).toHaveBeenCalled();
    });
  });

  test('clicking cancel calls the correct callback', async () => {
    const onCancel = jest.fn();

    render(
      <CancelAmendmentModal
        triggerButton={<button type="button">Open</button>}
        scheduledMethodologies={[]}
        onCancel={onCancel}
        onConfirm={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Open' }));

    await waitFor(() => {
      expect(screen.getByText('Cancel')).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    );

    await waitFor(() => {
      expect(onCancel).toHaveBeenCalled();
    });
  });
});
