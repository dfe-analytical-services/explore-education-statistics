import Filters from '@frontend/modules/find-statistics/components/Filters';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { render, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

describe('Filters', () => {
  describe('desktop', () => {
    test('renders correctly', () => {
      render(
        <Filters
          showMobileFilters={false}
          themes={testThemeSummaries}
          totalResults={13}
          onChange={noop}
          onCloseMobileFilters={noop}
        />,
      );

      expect(
        screen.getByRole('group', { name: 'Filter by theme' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('group', { name: 'Filter by release type' }),
      ).toBeInTheDocument();

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  describe('mobile', () => {
    beforeAll(() => {
      mockIsMedia = true;
    });

    test('does not render when showMobileFilters is false', () => {
      render(
        <Filters
          showMobileFilters={false}
          themes={testThemeSummaries}
          totalResults={13}
          onChange={noop}
          onCloseMobileFilters={noop}
        />,
      );

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      expect(
        screen.queryByRole('group', { name: 'Filter by theme' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('group', { name: 'Filter by release type' }),
      ).not.toBeInTheDocument();
    });

    test('renders correctly when showMobileFilters is true', () => {
      render(
        <Filters
          showMobileFilters
          themes={testThemeSummaries}
          totalResults={13}
          onChange={noop}
          onCloseMobileFilters={noop}
        />,
      );

      const modal = within(screen.getByRole('dialog'));

      expect(
        modal.getByRole('group', { name: 'Filter by theme' }),
      ).toBeInTheDocument();
      expect(
        modal.getByRole('group', { name: 'Filter by release type' }),
      ).toBeInTheDocument();
      expect(
        modal.getByRole('button', { name: 'Back to results' }),
      ).toBeInTheDocument();
      expect(
        modal.getByRole('button', { name: 'Show 13 results' }),
      ).toBeInTheDocument();
    });
  });
});
