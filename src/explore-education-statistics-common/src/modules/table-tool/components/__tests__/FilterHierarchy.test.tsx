import FilterHierarchy from '@common/modules/table-tool/components/FilterHierarchy';
import {
  testFilterHierarchy,
  testOptionsLabelsMap,
} from '@common/modules/table-tool/components/__tests__/__data__/testFiltersData.data';
import FormProvider from '@common/components/form/FormProvider';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('FilterHierarchy', () => {
  test('renders', async () => {
    const { user } = render(
      <FormProvider>
        <FilterHierarchy
          filterHierarchy={testFilterHierarchy}
          name="test-filter"
          optionLabelsMap={testOptionsLabelsMap}
        />
      </FormProvider>,
    );

    // Expand filter
    await user.click(
      screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers) - show options',
      }),
    );

    // Check search rendered
    expect(
      screen.getByLabelText(
        'Search all tiers and name of course being studied',
      ),
    ).toBeInTheDocument();

    // Check the modal
    await user.click(
      screen.getByRole('button', {
        name: 'What are name of course being studied tiers?',
      }),
    );
    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', {
        name: 'Name of course being studied tiers',
      }),
    ).toBeInTheDocument();
    expect(
      modal.getByRole('heading', { name: 'Level of qualification (tier 1)' }),
    ).toBeInTheDocument();
    expect(
      modal.getByRole('heading', { name: 'Sector subject area (tier 2)' }),
    ).toBeInTheDocument();
    expect(
      modal.getByRole('heading', {
        name: 'Name of course being studied (tier 3)',
      }),
    ).toBeInTheDocument();
    await user.click(modal.getByText('Close'));

    // Check all the checkboxes are rendered
    const optionsContainer = within(
      screen.getByTestId('filter-hierarchy-options'),
    );
    expect(optionsContainer.getAllByRole('checkbox')).toHaveLength(22);

    // Top level checkboxes
    expect(
      optionsContainer.getByRole('checkbox', {
        name: 'Total',
        description: 'Level of qualification',
      }),
    ).toBeInTheDocument();

    expect(
      optionsContainer.getByRole('checkbox', {
        name: 'Entry level',
        description: 'Level of qualification',
      }),
    ).toBeInTheDocument();

    expect(
      optionsContainer.getByRole('checkbox', {
        name: 'Higher',
        description: 'Level of qualification',
      }),
    ).toBeInTheDocument();

    // Expand entry level
    await user.click(
      screen.getByRole('button', {
        name: 'Show sub categories for entry level',
      }),
    );
    expect(
      screen.getByRole('button', {
        name: 'Close this sub category for entry level',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Select this and all sub categories for entry level',
      }),
    ).toBeInTheDocument();

    // Entry level sub categories
    const entryLevelCategories = within(
      screen.getByTestId(
        'filter-hierarchy-options-option-1,option-4,option-11',
      ),
    );
    expect(
      entryLevelCategories.getByRole('checkbox', {
        name: 'Science',
        description: 'Sector subject area',
      }),
    ).toBeInTheDocument();
    expect(
      entryLevelCategories.getByRole('checkbox', {
        name: 'Engineering',
        description: 'Sector subject area',
      }),
    ).toBeInTheDocument();

    // Expand engineering
    await user.click(
      screen.getByRole('button', {
        name: 'Show sub categories for engineering',
      }),
    );
    expect(
      screen.getByRole('button', {
        name: 'Close this sub category for engineering',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Select this and all sub categories for engineering',
      }),
    ).toBeInTheDocument();

    // Engineering sub categories
    const engineeringCategories = within(
      screen.getByTestId(
        'filter-hierarchy-options-option-1,option-2,option-11',
      ),
    );
    expect(
      engineeringCategories.getByRole('checkbox', {
        name: 'Civil engineering',
      }),
    ).toBeInTheDocument();
    expect(
      engineeringCategories.getByRole('checkbox', {
        name: 'Electrical engineering',
      }),
    ).toBeInTheDocument();
  });

  test('search filters the list and emboldeneds the option labels', async () => {
    const { user } = render(
      <FormProvider>
        <FilterHierarchy
          filterHierarchy={testFilterHierarchy}
          name="test-filter"
          optionLabelsMap={testOptionsLabelsMap}
        />
      </FormProvider>,
    );

    // Expand filter
    await user.click(
      screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers) - show options',
      }),
    );

    const optionsContainer = within(
      screen.getByTestId('filter-hierarchy-options'),
    );
    const options = optionsContainer.getAllByRole('checkbox');
    expect(options).toHaveLength(22);

    // type a term into the search
    await user.type(
      screen.getByLabelText(
        'Search all tiers and name of course being studied',
      ),
      'engineering',
    );
    await user.click(screen.getByRole('button', { name: 'Search' }));

    expect(
      screen.getByText(
        `Searching 'engineering' in all tiers of name of course being studied`,
      ),
    ).toBeInTheDocument();

    // see options list has changed
    const filteredOptions = optionsContainer.getAllByRole('checkbox');
    expect(filteredOptions).toHaveLength(4);
    expect(filteredOptions).toContain(
      optionsContainer.getByLabelText('Entry level'),
    );
    expect(filteredOptions).toContain(
      optionsContainer.getByLabelText('Engineering'),
    );
    expect(filteredOptions).toContain(
      optionsContainer.getByLabelText('Civil engineering'),
    );
    expect(filteredOptions).toContain(
      optionsContainer.getByLabelText('Electrical engineering'),
    );

    const searchHighlights =
      optionsContainer.getAllByTestId('search-highlight');
    expect(searchHighlights).toHaveLength(3);

    // see labels with search term contain the matching term text in bold
    searchHighlights.forEach(highlight =>
      expect(
        within(highlight).getByText('engineering', { exact: false }).nodeName,
      ).toEqual('EM'),
    );
  });

  test('clearing search resets the UI', async () => {
    const { user } = render(
      <FormProvider>
        <FilterHierarchy
          filterHierarchy={testFilterHierarchy}
          name="test-filter"
          optionLabelsMap={testOptionsLabelsMap}
        />
      </FormProvider>,
    );

    // Expand filter
    await user.click(
      screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers) - show options',
      }),
    );

    const optionsContainer = screen.getByTestId('filter-hierarchy-options');
    expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(22);

    // type a term into the search
    const hierarchySearch = screen.getByLabelText(
      'Search all tiers and name of course being studied',
    );
    await user.type(hierarchySearch, 'bricklaying');
    await user.click(screen.getByRole('button', { name: 'Search' }));

    const searchDescription = screen.getByText(
      `Searching 'bricklaying' in all tiers of name of course being studied`,
    );
    expect(searchDescription).toBeInTheDocument();

    // see options list has changed
    expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(10);

    // submit empty search term
    await user.clear(hierarchySearch);
    await user.type(hierarchySearch, ' ');
    await user.click(screen.getByRole('button', { name: 'Search' }));

    // expect UI to be reset
    expect(searchDescription).toHaveTextContent(
      'Browse all tiers of name of course being studied',
    );
    expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(22);

    // type a term into the search
    await user.type(hierarchySearch, 'engineering');
    await user.click(screen.getByRole('button', { name: 'Search' }));

    // see options list has changed
    expect(searchDescription).toHaveTextContent(
      `Searching 'engineering' in all tiers of name of course being studied`,
    );
    expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(4);

    // click clear search
    const clearSearchButton = screen.getByRole('button', {
      name: 'Clear search',
    });
    await user.click(clearSearchButton);

    // expect UI to be reset
    expect(searchDescription).toHaveTextContent(
      'Browse all tiers of name of course being studied',
    );
    expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(22);
  });

  test('select all sub categories', async () => {
    const { user } = render(
      <FormProvider>
        <FilterHierarchy
          filterHierarchy={testFilterHierarchy}
          name="test-filter"
          optionLabelsMap={testOptionsLabelsMap}
        />
      </FormProvider>,
    );

    // Expand filter
    await user.click(
      screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers) - show options',
      }),
    );

    // Expand entry level
    await user.click(
      screen.getByRole('button', {
        name: 'Show sub categories for entry level',
      }),
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Select all sub categories for entry level',
      }),
    );

    expect(
      screen.getByRole('button', {
        name: 'Unselect all sub categories for entry level',
      }),
    ).toBeInTheDocument();

    expect(screen.getByRole('checkbox', { name: 'Engineering' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Science' })).toBeChecked();

    expect(
      screen.getByRole('checkbox', { name: 'Entry level' }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Civil engineering' }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Electrical engineering' }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Biochemistry' }),
    ).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Physics' })).not.toBeChecked();

    expect(screen.getByTestId('test-filter-count')).toHaveTextContent(
      '2 selected',
    );
  });

  test('select this and all sub categories', async () => {
    const { user } = render(
      <FormProvider>
        <FilterHierarchy
          filterHierarchy={testFilterHierarchy}
          name="test-filter"
          optionLabelsMap={testOptionsLabelsMap}
        />
      </FormProvider>,
    );

    // Expand filter
    await user.click(
      screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers) - show options',
      }),
    );

    // Expand entry level
    await user.click(
      screen.getByRole('button', {
        name: 'Show sub categories for entry level',
      }),
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Select this and all sub categories for entry level',
      }),
    );

    expect(
      screen.getByRole('button', {
        name: 'Unselect this and all sub categories for entry level',
      }),
    ).toBeInTheDocument();

    expect(screen.getByRole('checkbox', { name: 'Entry level' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Engineering' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Science' })).toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Civil engineering' }),
    ).toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Electrical engineering' }),
    ).toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Biochemistry' }),
    ).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Physics' })).toBeChecked();

    expect(screen.getByTestId('test-filter-count')).toHaveTextContent(
      '7 selected',
    );
  });
});
