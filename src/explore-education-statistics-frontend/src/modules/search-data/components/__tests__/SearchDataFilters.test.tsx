import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import Filters from '@frontend/modules/search-data/components/SearchDataFilters';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { SortOption } from '@frontend/components/SortControls';
import { testPublicationTree } from '@frontend/modules/search-data/__tests__/__data__/testPublicationTree';

describe('SearchDataFilters', () => {
  const defaultProps = {
    includeDataFilters: true,
    geographicLevelOptions: [
      { label: 'National', value: 'NAT' },
      { label: 'Local Authority', value: 'LA' },
    ],
    publicationTree: testPublicationTree,
    releaseTypeOptions: [
      {
        label: 'Accredited Official Statistics',
        value: 'AccreditedOfficialStatistics',
      },
    ],
    sortBy: 'newest' as PublicationSortOption,
    sortOptions: [
      { label: 'Newest', value: 'newest' },
      { label: 'Oldest', value: 'oldest' },
      { label: 'A to Z', value: 'title' },
    ] as SortOption[],
    themes: testThemeSummaries,
    themeOptions: [
      { label: 'Theme 1', value: 'theme-id-1' },
      { label: 'Theme 2', value: 'theme-id-2' },
    ],
    onChange: jest.fn(),
    onChangeBatch: jest.fn(),
    onSortChange: jest.fn(),
  };

  describe('rendering', () => {
    test('renders correctly when includeDataFilters is true', () => {
      render(<Filters {...defaultProps} />);

      expect(screen.getByText('Filter and sort')).toBeInTheDocument();

      // Should render the complex ThemesAndReleasesFilterGroup
      expect(
        screen.getByTestId('themes-and-releases-filter-group'),
      ).toBeInTheDocument();
      // Should NOT render the basic theme checkbox group
      expect(
        screen.queryByRole('group', { name: 'Filter by Theme' }),
      ).not.toBeInTheDocument();

      // Data-specific filters should be visible
      expect(
        screen.getByRole('group', { name: 'Filter by Geographic Level' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('group', { name: 'Show latest or all releases' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('group', { name: 'Type of data' }),
      ).toBeInTheDocument();

      // Universal filters should be visible
      expect(
        screen.getByRole('group', { name: 'Filter by Release type' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('group', { name: 'Sort by' }),
      ).toBeInTheDocument();
    });

    test('renders correctly when includeDataFilters is false', () => {
      render(<Filters {...defaultProps} includeDataFilters={false} />);

      // Should render basic theme checkbox group instead of the complex component
      expect(
        screen.getByRole('group', { name: 'Filter by Theme' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByTestId('themes-and-releases-filter-group'),
      ).not.toBeInTheDocument();

      // Data-specific filters should NOT be visible
      expect(
        screen.queryByRole('group', { name: 'Filter by Geographic Level' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('group', { name: 'Show latest or all releases' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('group', { name: 'Type of data' }),
      ).not.toBeInTheDocument();

      // Universal filters should still be visible
      expect(
        screen.getByRole('group', { name: 'Filter by Release type' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('group', { name: 'Sort by' }),
      ).toBeInTheDocument();
    });
  });

  describe('interactions', () => {
    test('calls onChangeBatch from ThemesAndReleasesFilterGroup when includeDataFilters is true', async () => {
      render(<Filters {...defaultProps} />);

      await userEvent.click(screen.getByLabelText('Theme 1'));

      expect(defaultProps.onChangeBatch).toHaveBeenCalledWith({
        themeId: ['theme-id-1'],
        publicationId: [],
      });
    });

    test('calls onChange when a basic Theme checkbox is selected (includeDataFilters = false)', async () => {
      render(<Filters {...defaultProps} includeDataFilters={false} />);

      await userEvent.click(screen.getByLabelText('Theme 1'));

      expect(defaultProps.onChange).toHaveBeenCalledWith({
        filterType: 'themeId',
        nextValue: 'theme-id-1',
      });
    });

    test('calls onChange when a Geographic Level checkbox is selected', async () => {
      render(<Filters {...defaultProps} />);

      await userEvent.click(screen.getByLabelText('National'));

      expect(defaultProps.onChange).toHaveBeenCalledWith({
        filterType: 'geographicLevel',
        nextValue: 'NAT',
      });
    });

    test('calls onChange when a Show Latest radio is selected', async () => {
      render(<Filters {...defaultProps} latestDataOnly />);

      await userEvent.click(screen.getByLabelText('Show all releases'));

      expect(defaultProps.onChange).toHaveBeenCalledWith({
        filterType: 'latestDataOnly',
        nextValue: 'false',
      });
    });

    test('calls onChange when a Release Type checkbox is selected', async () => {
      render(<Filters {...defaultProps} />);

      await userEvent.click(
        screen.getByLabelText('Accredited Official Statistics'),
      );

      expect(defaultProps.onChange).toHaveBeenCalledWith({
        filterType: 'releaseType',
        nextValue: 'AccreditedOfficialStatistics',
      });
    });

    test('calls onChange when a Data Set Type radio is selected', async () => {
      render(<Filters {...defaultProps} />);

      await userEvent.click(screen.getByLabelText('API data sets only'));

      expect(defaultProps.onChange).toHaveBeenCalledWith({
        filterType: 'dataSetType',
        nextValue: 'api',
      });
    });

    test('calls onSortChange when a Sort By radio is selected', async () => {
      render(<Filters {...defaultProps} />);

      await userEvent.click(screen.getByLabelText('Oldest'));

      expect(defaultProps.onSortChange).toHaveBeenCalledWith('oldest');
    });
  });
});
