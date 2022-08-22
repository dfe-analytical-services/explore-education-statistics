import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersGroupControls from '@common/modules/table-tool/components/TableHeadersGroupControls';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/tableHeadersConfig.data';
import { Formik } from 'formik';
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
        screen.queryByRole('button', { name: 'Move Locations to columns' }),
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
        screen.getByRole('button', { name: 'Move Locations to columns' }),
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
        screen.getByRole('button', { name: 'Move Locations to columns' }),
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
});

function render({
  activeGroup = undefined,
  expandedLists = [],
  totalItems = 2,
}: {
  activeGroup?: string;
  expandedLists?: string[];
  totalItems?: number;
}) {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider
      activeGroup={activeGroup}
      expandedLists={expandedLists}
    >
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <TableHeadersGroupControls
          defaultNumberOfItems={2}
          groupName="rowGroups"
          id="test-id"
          legend="Locations"
          totalItems={totalItems}
          onMove={noop}
        />
      </Formik>
    </TableHeadersContextProvider>,
  );
}
