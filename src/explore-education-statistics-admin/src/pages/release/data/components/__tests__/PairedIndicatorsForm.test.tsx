import PairedIndicatorsForm from '@admin/pages/release/data/components/PairedIndicatorsForm';
import _adminTableBuilderService from '@admin/services/tableBuilderService';
import createAxiosErrorMock from '@common-test/createAxiosErrorMock';
import render from '@common-test/render';
import _tableBuilderService, {
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { ValidationProblemDetails } from '@common/services/types/problemDetails';
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

describe('PairedIndicatorsForm', () => {
  const testSubject = {
    id: 'subject-id',
    name: 'Subject name',
  } as Subject;

  const testSubjectMeta: SubjectMeta = {
    filters: {},
    indicators: {
      Group1: {
        id: 'group-1',
        label: 'Group 1',
        options: [
          {
            value: 'indicator-1',
            label: 'Indicator 1',
            unit: '',
            name: 'indicator_1',
          },
          {
            value: 'indicator-2',
            label: 'Indicator 2',
            unit: '',
            name: 'indicator_2',
          },
          {
            value: 'indicator-3',
            label: 'Indicator 3',
            unit: '',
            name: 'indicator_3',
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
  });

  test('renders existing paired indicators', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });

    render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={jest.fn()}
      />,
    );

    const pair = await screen.findByRole('group', { name: 'Pair 1' });

    expect(within(pair).getByLabelText('Parent indicator')).toHaveValue(
      'indicator-1',
    );
    expect(within(pair).getByLabelText('Child indicator')).toHaveValue(
      'indicator-2',
    );
    expect(adminTableBuilderService.getPairedIndicators).toHaveBeenCalledWith(
      'release-version-id',
      'subject-id',
    );
  });

  test('renders a message when there are no paired indicators', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={jest.fn()}
      />,
    );

    expect(
      await screen.findByText(
        'No indicators have been paired for this data set.',
      ),
    ).toBeInTheDocument();
  });

  test('adding, changing and removing pairs saves the correct request', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });

    const onSave = jest.fn().mockResolvedValue(undefined);

    const { user } = render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={onSave}
      />,
    );

    await screen.findByRole('group', { name: 'Pair 1' });

    await user.click(screen.getByRole('button', { name: 'Add pair' }));

    const pair2 = screen.getByRole('group', { name: 'Pair 2' });
    await user.selectOptions(
      within(pair2).getByLabelText('Parent indicator'),
      'indicator-3',
    );
    await user.selectOptions(
      within(pair2).getByLabelText('Child indicator'),
      'indicator-1',
    );

    // Remove the original pair, otherwise indicator 1 would be both a parent and a child
    await user.click(screen.getByRole('button', { name: 'Remove pair 1' }));

    await user.click(
      screen.getByRole('button', { name: 'Save paired indicators' }),
    );

    await waitFor(() => expect(onSave).toHaveBeenCalledTimes(1));
    expect(onSave).toHaveBeenCalledWith('subject-id', {
      pairedIndicators: [
        { parentIndicatorId: 'indicator-3', childIndicatorId: 'indicator-1' },
      ],
    });
  });

  test('removing all pairs saves an empty request', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });

    const onSave = jest.fn().mockResolvedValue(undefined);

    const { user } = render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={onSave}
      />,
    );

    await user.click(
      await screen.findByRole('button', { name: 'Remove pair 1' }),
    );
    await user.click(
      screen.getByRole('button', { name: 'Save paired indicators' }),
    );

    await waitFor(() =>
      expect(onSave).toHaveBeenCalledWith('subject-id', {
        pairedIndicators: [],
      }),
    );
  });

  test.each([
    {
      name: 'an indicator paired with itself',
      pairs: [['indicator-1', 'indicator-1']],
      message: 'An indicator cannot be paired with itself.',
    },
    {
      name: 'a child indicator with multiple parents',
      pairs: [
        ['indicator-1', 'indicator-3'],
        ['indicator-2', 'indicator-3'],
      ],
      message: 'A child indicator can only have one parent indicator.',
    },
    {
      name: 'a parent indicator with multiple children',
      pairs: [
        ['indicator-1', 'indicator-2'],
        ['indicator-1', 'indicator-3'],
      ],
      message: 'A parent indicator can only have one child indicator.',
    },
    {
      name: 'chained indicators',
      pairs: [
        ['indicator-1', 'indicator-2'],
        ['indicator-2', 'indicator-3'],
      ],
      message: 'An indicator cannot be both a parent and a child indicator.',
    },
  ])('shows a validation error for $name', async ({ pairs, message }) => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: pairs.map(([parentIndicatorId, childIndicatorId]) => ({
        parentIndicatorId,
        childIndicatorId,
      })),
    });

    const onSave = jest.fn();

    const { user } = render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={onSave}
      />,
    );

    await screen.findByRole('group', { name: 'Pair 1' });

    await user.click(
      screen.getByRole('button', { name: 'Save paired indicators' }),
    );

    expect(
      await screen.findByText(message, { selector: 'a' }),
    ).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });

  test('shows a validation error when a pair is incomplete', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    const onSave = jest.fn();

    const { user } = render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={onSave}
      />,
    );

    await user.click(await screen.findByRole('button', { name: 'Add pair' }));
    await user.click(
      screen.getByRole('button', { name: 'Save paired indicators' }),
    );

    expect(
      await screen.findByText('Select a parent indicator', { selector: 'a' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Select a child indicator', { selector: 'a' }),
    ).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });

  test('shows a server validation error when the import is not complete', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });

    const onSave = jest.fn().mockRejectedValue(
      createAxiosErrorMock<ValidationProblemDetails>({
        data: {
          errors: [{ code: 'PairedIndicatorsImportNotComplete', message: '' }],
          title: '',
          type: '',
          status: 400,
        },
      }),
    );

    const { user } = render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={noop}
        onSave={onSave}
      />,
    );

    await screen.findByRole('group', { name: 'Pair 1' });

    await user.click(
      screen.getByRole('button', { name: 'Save paired indicators' }),
    );

    expect(
      await screen.findByText(
        'Paired indicators cannot be changed until the data set import is complete.',
        { selector: 'a' },
      ),
    ).toBeInTheDocument();
  });

  test('clicking Cancel calls onCancel', async () => {
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [],
    });

    const onCancel = jest.fn();

    const { user } = render(
      <PairedIndicatorsForm
        releaseVersionId="release-version-id"
        subject={testSubject}
        onCancel={onCancel}
        onSave={jest.fn()}
      />,
    );

    await user.click(await screen.findByRole('button', { name: 'Cancel' }));

    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
