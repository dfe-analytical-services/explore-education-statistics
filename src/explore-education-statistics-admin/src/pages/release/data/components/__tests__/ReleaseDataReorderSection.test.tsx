import ReleaseDataReorderSection from '@admin/pages/release/data/components/ReleaseDataReorderSection';
import _adminTableBuilderService from '@admin/services/tableBuilderService';
import render from '@common-test/render';
import _tableBuilderService, {
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/tableBuilderService');
jest.mock('@common/services/tableBuilderService');

const adminTableBuilderService = _adminTableBuilderService as jest.Mocked<
  typeof _adminTableBuilderService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ReleaseDataReorderSection', () => {
  const testSubjects = [
    { id: 'subject-1', name: 'Subject 1' },
    { id: 'subject-2', name: 'Subject 2' },
  ] as Subject[];

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
        ],
        order: 0,
      },
    },
    locations: {},
    timePeriod: { hint: '', legend: '', options: [] },
  };

  test('renders the manage buttons for each data set', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    render(
      <ReleaseDataReorderSection
        releaseVersionId="release-version-id"
        canUpdateRelease
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Manage filters and indicators' }),
    ).toBeInTheDocument();

    const rows = within(await screen.findByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(3);
    expect(
      within(rows[1]).getByRole('button', {
        name: 'Manage filters for Subject 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', {
        name: 'Manage indicators for Subject 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', {
        name: 'Manage indicators for Subject 2',
      }),
    ).toBeInTheDocument();
  });

  test('renders a warning and no data sets when the release cannot be updated', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    render(
      <ReleaseDataReorderSection
        releaseVersionId="release-version-id"
        canUpdateRelease={false}
      />,
    );

    expect(
      await screen.findByText(
        'This release has been approved, and can no longer be updated.',
      ),
    ).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('managing indicators shows the reorder list and paired indicators form, and saving pairs returns to the data sets', async () => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    adminTableBuilderService.getPairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });
    adminTableBuilderService.updatePairedIndicators.mockResolvedValue({
      pairedIndicators: [
        { parentIndicatorId: 'indicator-1', childIndicatorId: 'indicator-2' },
      ],
    });

    const { user } = render(
      <ReleaseDataReorderSection
        releaseVersionId="release-version-id"
        canUpdateRelease
      />,
    );

    await user.click(
      await screen.findByRole('button', {
        name: 'Manage indicators for Subject 2',
      }),
    );

    expect(
      await screen.findByRole('heading', {
        name: 'Reorder indicators for Subject 2',
      }),
    ).toBeInTheDocument();
    expect(
      await screen.findByRole('heading', {
        name: 'Pair indicators for Subject 2',
      }),
    ).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();

    await user.click(
      await screen.findByRole('button', { name: 'Save paired indicators' }),
    );

    await waitFor(() =>
      expect(
        adminTableBuilderService.updatePairedIndicators,
      ).toHaveBeenCalledWith('release-version-id', 'subject-2', {
        pairedIndicators: [
          {
            parentIndicatorId: 'indicator-1',
            childIndicatorId: 'indicator-2',
          },
        ],
      }),
    );

    expect(await screen.findByRole('table')).toBeInTheDocument();
  });
});
