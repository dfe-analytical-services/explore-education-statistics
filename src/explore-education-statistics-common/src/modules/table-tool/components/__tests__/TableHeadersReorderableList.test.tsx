import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersReorderableList from '@common/modules/table-tool/components/TableHeadersReorderableList';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/tableHeadersConfig.data';
import { Formik } from 'formik';
import { render as baseRender, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersReorderableList', () => {
  test('renders the reorderable list', () => {
    render();

    expect(
      screen.getByRole('group', { name: 'Category group' }),
    ).toBeInTheDocument();

    // Draggable items are buttons
    const buttons = screen.getAllByRole('button');
    expect(buttons).toHaveLength(2);
    expect(within(buttons[0]).getByText('Category 1'));
    expect(within(buttons[1]).getByText('Category 2'));
  });
});

function render() {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider>
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <TableHeadersReorderableList
          id="test-id"
          legend="Category group"
          name="rowGroups[0]"
        />
      </Formik>
    </TableHeadersContextProvider>,
  );
}
