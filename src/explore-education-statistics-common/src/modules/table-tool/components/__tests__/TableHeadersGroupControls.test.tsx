import TableHeadersGroupControls from '@common/modules/table-tool/components/TableHeadersGroupControls';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import { render as baseRender, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersGroupControls', () => {
  describe('a list is being reordered', () => {
    test('renders the Done button when the list is active', () => {
      render({ activeGroup: 'test-id' });

      expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Reorder items in Locations' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Move Locations' }),
      ).not.toBeInTheDocument();
    });

    test('renders disabled show more, reorder and move buttons when the list is not active', () => {
      render({ activeGroup: 'another-id' });

      expect(
        screen.queryByRole('button', { name: 'Done' }),
      ).not.toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Reorder items in Locations' }),
      ).toBeDisabled();
      expect(
        screen.getByRole('button', { name: 'Move Locations' }),
      ).toBeDisabled();
    });
  });

  describe('groups are reorderable', () => {
    test('renders the show more, reorder and move buttons', () => {
      render({});
      expect(
        screen.getByRole('button', { name: 'Reorder items in Locations' }),
      ).not.toBeDisabled();
      expect(
        screen.getByRole('button', { name: 'Move Locations' }),
      ).not.toBeDisabled();
      expect(
        screen.queryByRole('button', { name: 'Done' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('show more / fewer buttons', () => {
    test('renders the show more button if the list has more than the default number of items', () => {
      render({ totalItems: 5 });

      expect(
        screen.getByRole('button', { name: 'Show 3 more items in Locations' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Show fewer items for Locations',
        }),
      ).not.toBeInTheDocument();
    });

    test('renders the show fewer button if the list is expanded', () => {
      render({ expandedLists: ['test-id'], totalItems: 5 });
      expect(
        screen.getByRole('button', { name: 'Show fewer items for Locations' }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Show 3 more items in Locations',
        }),
      ).not.toBeInTheDocument();
    });

    test('does not render the show more button if the list has less than or equal the default number of items', () => {
      render({ totalItems: 2 });

      expect(
        screen.queryByRole('button', {
          name: 'Show 3 more items in Locations',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Show fewer items for Locations',
        }),
      ).not.toBeInTheDocument();
    });
  });

  describe('move controls', () => {
    test('renders the move controls when showMovingControls is true', async () => {
      render({ showMovingControls: true });
      expect(
        screen.queryByRole('button', { name: 'Move Locations' }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Move Locations left' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Move Locations right' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Move Locations to columns' }),
      ).toBeInTheDocument();
      expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument();
    });

    test('does not render the move controls when showMovingControls is false', async () => {
      render({ showMovingControls: false });
      expect(
        screen.queryByRole('button', { name: 'Move Locations left' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Move Locations right' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Move Locations to columns' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Done' }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Move Locations' }),
      ).toBeInTheDocument();
    });
  });
});

jest.mock( "react-hook-form", () => ( {
  ...jest.requireActual( "react-hook-form" ),
  useFormContext: () => ( {
    getValues: () => jest.fn(),
    setValues: () => jest.fn(),
  }),
}));

function render({
  activeGroup = undefined,
  expandedLists = [],
  showMovingControls = false,
  totalItems = 2,
}: {
  activeGroup?: string;
  expandedLists?: string[];
  showMovingControls?: boolean;
  totalItems?: number;
}) {
  baseRender(
    <TableHeadersContextProvider
      activeGroup={activeGroup}
      expandedLists={expandedLists}
    >
      <TableHeadersGroupControls
        defaultNumberOfItems={2}
        groupName="rowGroups"
        id="test-id"
        index={1}
        isLastGroup={false}
        legend="Locations"
        showMovingControls={showMovingControls}
        totalItems={totalItems}
        onMoveAxis={noop}
        onMoveDown={noop}
        onMoveUp={noop}
      />
    </TableHeadersContextProvider>,
  );
}
