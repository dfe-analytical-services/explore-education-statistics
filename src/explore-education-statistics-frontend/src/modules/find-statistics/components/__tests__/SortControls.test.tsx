import SortControls from '@frontend/modules/find-statistics/components/SortControls';
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

describe('SortControls', () => {
  describe('desktop', () => {
    test('renders correctly', () => {
      render(<SortControls sortBy="newest" onChange={noop} />);

      const sortGroup = within(
        screen.getByRole('group', { name: 'Sort results' }),
      );
      const sortOptions = sortGroup.getAllByRole('radio');
      expect(sortOptions).toHaveLength(3);
      expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
      expect(sortOptions[0]).toBeChecked();
      expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
      expect(sortOptions[1]).not.toBeChecked();
      expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
      expect(sortOptions[2]).not.toBeChecked();

      expect(screen.queryByLabelText('Sort results')).not.toBeInTheDocument();
    });

    test('setting the intial sortBy selects the correct option', () => {
      render(<SortControls sortBy="oldest" onChange={noop} />);

      const sortGroup = within(
        screen.getByRole('group', { name: 'Sort results' }),
      );
      const sortOptions = sortGroup.getAllByRole('radio');
      expect(sortOptions).toHaveLength(3);
      expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
      expect(sortOptions[0]).not.toBeChecked();
      expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
      expect(sortOptions[1]).toBeChecked();
      expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
      expect(sortOptions[2]).not.toBeChecked();
    });

    test('setting hasSearch to true shows the relevance option', () => {
      render(<SortControls hasSearch sortBy="relevance" onChange={noop} />);

      const sortGroup = within(
        screen.getByRole('group', { name: 'Sort results' }),
      );
      const sortOptions = sortGroup.getAllByRole('radio');
      expect(sortOptions).toHaveLength(4);
      expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
      expect(sortOptions[0]).not.toBeChecked();
      expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
      expect(sortOptions[1]).not.toBeChecked();
      expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
      expect(sortOptions[2]).not.toBeChecked();
      expect(sortOptions[3]).toEqual(sortGroup.getByLabelText('Relevance'));
      expect(sortOptions[3]).toBeChecked();

      expect(screen.queryByLabelText('Sort results')).not.toBeInTheDocument();
    });
  });

  describe('mobile', () => {
    beforeAll(() => {
      mockIsMedia = true;
    });

    test('renders correctly', () => {
      render(<SortControls sortBy="newest" onChange={noop} />);

      const sortDropdown = within(screen.getByLabelText('Sort results'));
      const sortOptions = sortDropdown.getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(sortOptions).toHaveLength(3);
      expect(sortOptions[0]).toHaveTextContent('Newest');
      expect(sortOptions[0]).toHaveValue('newest');
      expect(sortOptions[0].selected).toBe(true);
      expect(sortOptions[1]).toHaveTextContent('Oldest');
      expect(sortOptions[1]).toHaveValue('oldest');
      expect(sortOptions[1].selected).toBe(false);
      expect(sortOptions[2]).toHaveTextContent('A to Z');
      expect(sortOptions[2]).toHaveValue('title');
      expect(sortOptions[2].selected).toBe(false);

      expect(screen.queryByRole('radio')).not.toBeInTheDocument();
    });

    test('setting the intial sortBy selects the correct option', () => {
      render(<SortControls sortBy="title" onChange={noop} />);

      const sortDropdown = within(screen.getByLabelText('Sort results'));
      const sortOptions = sortDropdown.getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(sortOptions).toHaveLength(3);
      expect(sortOptions[0]).toHaveTextContent('Newest');
      expect(sortOptions[0]).toHaveValue('newest');
      expect(sortOptions[0].selected).toBe(false);
      expect(sortOptions[1]).toHaveTextContent('Oldest');
      expect(sortOptions[1]).toHaveValue('oldest');
      expect(sortOptions[1].selected).toBe(false);
      expect(sortOptions[2]).toHaveTextContent('A to Z');
      expect(sortOptions[2]).toHaveValue('title');
      expect(sortOptions[2].selected).toBe(true);
    });

    test('setting hasSearch to true shows the relevance option', () => {
      render(<SortControls hasSearch sortBy="relevance" onChange={noop} />);

      const sortDropdown = within(screen.getByLabelText('Sort results'));
      const sortOptions = sortDropdown.getAllByRole(
        'option',
      ) as HTMLOptionElement[];
      expect(sortOptions).toHaveLength(4);
      expect(sortOptions[0]).toHaveTextContent('Newest');
      expect(sortOptions[0]).toHaveValue('newest');
      expect(sortOptions[0].selected).toBe(false);
      expect(sortOptions[1]).toHaveTextContent('Oldest');
      expect(sortOptions[1]).toHaveValue('oldest');
      expect(sortOptions[1].selected).toBe(false);
      expect(sortOptions[2]).toHaveTextContent('A to Z');
      expect(sortOptions[2]).toHaveValue('title');
      expect(sortOptions[2].selected).toBe(false);
      expect(sortOptions[3]).toHaveTextContent('Relevance');
      expect(sortOptions[3]).toHaveValue('relevance');
      expect(sortOptions[3].selected).toBe(true);
    });
  });
});
