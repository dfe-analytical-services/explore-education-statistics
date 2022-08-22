import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersReadOnlyList from '@common/modules/table-tool/components/TableHeadersReadOnlyList';
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

describe('TableHeadersReadOnlyList', () => {
  test('renders the list with the default number of items', () => {
    render();

    expect(
      screen.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();

    const listitems = screen.getAllByRole('listitem');
    expect(listitems).toHaveLength(2);
    expect(within(listitems[0]).getByText('Location 1')).toBeInTheDocument();
    expect(within(listitems[1]).getByText('Location 2')).toBeInTheDocument();
  });

  test('renders the list with all items when the list is expanded', () => {
    render(['test-id']);

    expect(
      screen.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();

    const listitems = screen.getAllByRole('listitem');
    expect(listitems).toHaveLength(5);
    expect(within(listitems[0]).getByText('Location 1')).toBeInTheDocument();
    expect(within(listitems[1]).getByText('Location 2')).toBeInTheDocument();
    expect(within(listitems[2]).getByText('Location 3')).toBeInTheDocument();
    expect(within(listitems[3]).getByText('Location 4')).toBeInTheDocument();
    expect(within(listitems[4]).getByText('Location 5')).toBeInTheDocument();
  });
});

function render(expandedLists: string[] = []) {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider expandedLists={expandedLists}>
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <TableHeadersReadOnlyList
          defaultNumberOfItems={2}
          id="test-id"
          legend="Locations"
          name="columnGroups[1]"
        />
      </Formik>
    </TableHeadersContextProvider>,
  );
}
