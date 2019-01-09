import React from 'react';
import { MemoryRouter } from 'react-router';
import { Route } from 'react-router-dom';
import { render } from 'react-testing-library';
import Breadcrumbs from '../Breadcrumbs';

describe('Breadcrumbs', () => {
  test('renders correctly with just home breadcrumb', () => {
    const path = '/';

    const { container } = render(
      <MemoryRouter initialEntries={[path]}>
        <Route path={path} exact>
          <Breadcrumbs />
        </Route>
      </MemoryRouter>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with multiple breadcrumbs', () => {
    const path = '/publications/test-publication';

    const { container } = render(
      <MemoryRouter initialEntries={[path]}>
        <Route path={path} exact>
          <Breadcrumbs />
        </Route>
      </MemoryRouter>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
