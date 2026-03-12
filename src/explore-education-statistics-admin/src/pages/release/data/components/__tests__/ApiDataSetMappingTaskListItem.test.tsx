import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import ApiDataSetMappingTaskListItem from '@admin/pages/release/data/components/ApiDataSetMappingTaskListItem';

interface Props {
  id: string;
  isPatch: boolean;
  mappingCompleteForFacet: boolean;
  majorChangesForFacet: boolean;
  mappingPageRoute: string;
  mappingText: string;
  mappingHintText: string;
  majorVersionRejected: boolean;
}

describe('ApiDataSetMappingTaskListItem', () => {
  test('renders "Incomplete" when mapping for facet is incomplete', () => {
    render(
      <MemoryRouter>
        <ApiDataSetMappingTaskListItem
          id="indicator-mapping-task"
          taskText="Map indicators"
          taskHintText="Hint text for mapping indicators"
          isPatch={false}
          mappingCompleteForFacet={false}
          mappingPageRoute="/mapping-page-route"
          majorChangesForFacet={false}
        />
        ,
      </MemoryRouter>,
    );

    expect(
      screen.getByTestId('indicator-mapping-task-tag'),
    ).toBeInTheDocument();
    const tag = screen.getByTestId('indicator-mapping-task-tag');
    expect(tag.textContent).toBe('Incomplete');
    expect(tag).toHaveClass('govuk-tag--red');

    expect(
      screen.getByText('Hint text for mapping indicators'),
    ).toBeInTheDocument();
    expect(screen.getByText('Hint text for mapping indicators')).toHaveClass(
      'govuk-task-list__hint',
    );

    expect(screen.getByRole('link')).toBeInTheDocument();
    expect(screen.getByRole('link')).toHaveAttribute(
      'href',
      '/mapping-page-route',
    );
  });

  test('renders "Complete" when mapping for facet is complete', () => {
    render(
      <MemoryRouter>
        <ApiDataSetMappingTaskListItem
          id="indicator-mapping-task"
          taskText="Map indicators"
          taskHintText="Hint text for mapping indicators"
          isPatch={false}
          mappingCompleteForFacet
          mappingPageRoute="/mapping-page-route"
          majorChangesForFacet={false}
        />
        ,
      </MemoryRouter>,
    );

    const tag = screen.getByTestId('indicator-mapping-task-tag');
    expect(tag.textContent).toBe('Complete');
    expect(tag).toHaveClass('govuk-tag--blue');
  });

  test('renders "Major Change" when mapping is complete but is a patch replacement and major change', () => {
    render(
      <MemoryRouter>
        <ApiDataSetMappingTaskListItem
          id="indicator-mapping-task"
          taskText="Map indicators"
          taskHintText="Hint text for mapping indicators"
          isPatch
          mappingCompleteForFacet
          mappingPageRoute="/mapping-page-route"
          majorChangesForFacet
        />
        ,
      </MemoryRouter>,
    );

    const tag = screen.getByTestId('indicator-mapping-task-tag');
    expect(tag.textContent).toBe('Major Change');
    expect(tag).toHaveClass('govuk-tag--red');
  });

  test('renders "Incomplete" when patch replacement and major change but mapping is not yet complete', () => {
    render(
      <MemoryRouter>
        <ApiDataSetMappingTaskListItem
          id="indicator-mapping-task"
          taskText="Map indicators"
          taskHintText="Hint text for mapping indicators"
          isPatch
          mappingCompleteForFacet={false}
          mappingPageRoute="/mapping-page-route"
          majorChangesForFacet
        />
        ,
      </MemoryRouter>,
    );

    const tag = screen.getByTestId('indicator-mapping-task-tag');
    expect(tag.textContent).toBe('Incomplete');
    expect(tag).toHaveClass('govuk-tag--red');
  });

  test('renders "Complete" when patch replacement and minor change and mapping is complete', () => {
    render(
      <MemoryRouter>
        <ApiDataSetMappingTaskListItem
          id="indicator-mapping-task"
          taskText="Map indicators"
          taskHintText="Hint text for mapping indicators"
          isPatch
          mappingCompleteForFacet
          mappingPageRoute="/mapping-page-route"
          majorChangesForFacet={false}
        />
        ,
      </MemoryRouter>,
    );

    const tag = screen.getByTestId('indicator-mapping-task-tag');
    expect(tag.textContent).toBe('Complete');
    expect(tag).toHaveClass('govuk-tag--blue');
  });
});
