import ManageIndicators from '@admin/pages/release/data/components/ManageIndicators';
import _adminTableBuilderService from '@admin/services/tableBuilderService';
import render from '@common-test/render';
import _tableBuilderService, {
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/tableBuilderService');
jest.mock('@common/services/tableBuilderService');

const adminTableBuilderService = _adminTableBuilderService as jest.Mocked<
  typeof _adminTableBuilderService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ManageIndicators', () => {
  const testSubject = { id: 'subject-id', name: 'Subject name' } as Subject;

  const testSubjectMeta: SubjectMeta = {
    filters: {},
    indicators: {
      Group2: {
        id: 'group-2',
        label: 'Group 2',
        options: [
          {
            value: 'indicator-3',
            label: 'Indicator 3',
            unit: '',
            name: 'indicator_3',
          },
        ],
        order: 1,
      },
      Group1: {
        id: 'group-1',
        label: 'Group 1',
        options: [
          {
            value: 'indicator-2',
            label: 'Indicator 2',
            unit: '',
            name: 'indicator_2',
          },
          {
            value: 'indicator-1',
            label: 'Indicator 1',
            unit: '',
            name: 'indicator_1',
          },
        ],
        order: 0,
      },
    },
    locations: {},
    timePeriod: { hint: '', legend: '', options: [] },
  };

  beforeEach(() => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-3' },
      ],
    });
  });

  test('renders the indicator order and paired indicators read-only', async () => {
    render(
      <ManageIndicators
        releaseVersionId="release-version-id"
        subject={testSubject}
        onBack={noop}
      />,
    );

    expect(
      await screen.findByRole('heading', {
        name: 'Manage indicators for Subject name',
      }),
    ).toBeInTheDocument();

    const orderHeading = await screen.findByRole('heading', {
      name: 'Indicator order',
    });
    const orderList = within(
      orderHeading.parentElement as HTMLElement,
    ).getAllByRole('listitem');

    expect(orderList.map(item => item.firstChild?.textContent)).toEqual([
      'Group 1',
      'Indicator 2',
      'Indicator 1',
      'Group 2',
      'Indicator 3',
    ]);

    const rows = within(
      screen.getByRole('table', { name: 'Paired indicators for Subject name' }),
    ).getAllByRole('row');

    expect(rows).toHaveLength(2);
    expect(within(rows[1]).getAllByRole('cell')[0]).toHaveTextContent(
      'Indicator 1',
    );
    expect(within(rows[1]).getAllByRole('cell')[1]).toHaveTextContent(
      'Indicator 3',
    );

    // No form is shown in the read-only view
    expect(
      screen.queryByRole('button', { name: 'Save order' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save paired indicators' }),
    ).not.toBeInTheDocument();
  });

  test('renders a message when there are no paired indicators', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    render(
      <ManageIndicators
        releaseVersionId="release-version-id"
        subject={testSubject}
        onBack={noop}
      />,
    );

    expect(
      await screen.findByText(
        'No indicators have been paired for this data set.',
      ),
    ).toBeInTheDocument();
  });

  test('reordering shows only the reorder list, and cancel returns to the read-only view', async () => {
    const { user } = render(
      <ManageIndicators
        releaseVersionId="release-version-id"
        subject={testSubject}
        onBack={noop}
      />,
    );

    await user.click(
      await screen.findByRole('button', { name: 'Reorder indicators' }),
    );

    expect(
      await screen.findByRole('heading', {
        name: 'Reorder indicators for Subject name',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save paired indicators' }),
    ).not.toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: 'Cancel' })).toHaveLength(1);

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(
      await screen.findByRole('heading', {
        name: 'Manage indicators for Subject name',
      }),
    ).toBeInTheDocument();
  });

  test('editing paired indicators shows only the pairing form, and saving returns to the read-only view', async () => {
    adminTableBuilderService.updatePairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    const { user } = render(
      <ManageIndicators
        releaseVersionId="release-version-id"
        subject={testSubject}
        onBack={noop}
      />,
    );

    await user.click(
      await screen.findByRole('button', { name: 'Edit paired indicators' }),
    );

    expect(
      await screen.findByRole('heading', {
        name: 'Pair indicators for Subject name',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save order' }),
    ).not.toBeInTheDocument();

    // Remove the only pair, and the reloaded read-only view shows no pairs
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    await user.click(
      await screen.findByRole('button', { name: 'Remove pair 1' }),
    );
    await user.click(
      screen.getByRole('button', { name: 'Save paired indicators' }),
    );

    await waitFor(() =>
      expect(
        adminTableBuilderService.updatePairedIndicators,
      ).toHaveBeenCalledWith('release-version-id', 'subject-id', {
        pairedIndicators: [],
      }),
    );

    expect(
      await screen.findByText(
        'No indicators have been paired for this data set.',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Manage indicators for Subject name',
      }),
    ).toBeInTheDocument();
  });

  test('cancelling paired indicators returns to the read-only view', async () => {
    const { user } = render(
      <ManageIndicators
        releaseVersionId="release-version-id"
        subject={testSubject}
        onBack={noop}
      />,
    );

    await user.click(
      await screen.findByRole('button', { name: 'Edit paired indicators' }),
    );
    await user.click(await screen.findByRole('button', { name: 'Cancel' }));

    expect(
      await screen.findByRole('button', { name: 'Edit paired indicators' }),
    ).toBeInTheDocument();
    expect(
      adminTableBuilderService.updatePairedIndicators,
    ).not.toHaveBeenCalled();
  });

  test('clicking Back to data files calls onBack', async () => {
    const onBack = jest.fn();

    const { user } = render(
      <ManageIndicators
        releaseVersionId="release-version-id"
        subject={testSubject}
        onBack={onBack}
      />,
    );

    await user.click(
      await screen.findByRole('button', { name: 'Back to data files' }),
    );

    expect(onBack).toHaveBeenCalledTimes(1);
  });
});
