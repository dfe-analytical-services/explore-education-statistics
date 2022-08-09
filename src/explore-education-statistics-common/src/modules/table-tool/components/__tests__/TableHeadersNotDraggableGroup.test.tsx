import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersNotDraggableGroup from '@common/modules/table-tool/components/TableHeadersNotDraggableGroup';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/TableHeadersConfig.data';
import { Formik } from 'formik';
import { render as baseRender, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import _ from 'lodash';

describe('TableHeadersNotDraggableGroup', () => {
  test('renders the reorderable list and group controls when reordering is active', () => {
    render({ activeList: 'group-category-group' });

    expect(
      screen.getByRole('group', { name: 'Category group' }),
    ).toBeInTheDocument();

    // Draggable items are buttons
    const buttons = screen.getAllByRole('button');
    expect(buttons).toHaveLength(3);
    expect(within(buttons[0]).getByText('Category 1')).toBeInTheDocument();
    expect(within(buttons[1]).getByText('Category 2')).toBeInTheDocument();

    expect(within(buttons[2]).getByText('Done'));
  });

  test('renders the readonly list and group controls when reordering is not active', () => {
    render({ activeList: 'anotherGroup' });

    expect(
      screen.getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();

    const listitems = screen.getAllByRole('listitem');
    expect(listitems).toHaveLength(2);
    expect(within(listitems[0]).getByText('Category 1')).toBeInTheDocument();
    expect(within(listitems[1]).getByText('Category 2')).toBeInTheDocument();

    expect(screen.getAllByRole('button')).toHaveLength(2);
    expect(
      screen.getByRole('button', { name: 'Reorder items in Category group' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Category group to columns' }),
    ).toBeInTheDocument();
  });
});

function render({ activeList }: { activeList: string }) {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider activeList={activeList}>
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <TableHeadersNotDraggableGroup
          legend="Category group"
          name="rowGroups[0]"
          totalItems={2}
        />
      </Formik>
    </TableHeadersContextProvider>,
  );
}
