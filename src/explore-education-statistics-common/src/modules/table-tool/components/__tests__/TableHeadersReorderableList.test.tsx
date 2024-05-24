import FormProvider from '@common/components/form/FormProvider';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersReorderableList from '@common/modules/table-tool/components/TableHeadersReorderableList';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/tableHeadersConfig.data';
import { render as baseRender, screen } from '@testing-library/react';
import React from 'react';

describe('TableHeadersReorderableList', () => {
  test('renders the reorderable list', () => {
    render();

    expect(
      screen.getByRole('group', { name: 'Category group' }),
    ).toBeInTheDocument();

    const buttons = screen.getAllByTestId('reorderable-item');
    expect(buttons).toHaveLength(2);
    expect(buttons[0]).toHaveTextContent('Category 1');
    expect(buttons[1]).toHaveTextContent('Category 2');
  });
});

function render() {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider>
      <FormProvider initialValues={testInitialFormValues}>
        <TableHeadersReorderableList
          id="test-id"
          legend="Category group"
          name="rowGroups[0]"
        />
      </FormProvider>
    </TableHeadersContextProvider>,
  );
}
