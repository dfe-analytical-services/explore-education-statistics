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

    const breadcrumbs = container.querySelectorAll('li');

    expect(breadcrumbs).toHaveLength(1);
    expect(breadcrumbs[0]!.textContent).toBe('Home');

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

    const breadcrumbs = container.querySelectorAll('li');

    expect(breadcrumbs).toHaveLength(3);
    expect(breadcrumbs[0]!.textContent).toBe('Home');
    expect(breadcrumbs[1]!.textContent).toBe('Publications');
    expect(breadcrumbs[2]!.textContent).toBe('Test publication');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render last breadcrumb as a link', () => {
    const path = '/publications/test-publication';

    const { container } = render(
      <MemoryRouter initialEntries={[path]}>
        <Route path={path} exact>
          <Breadcrumbs />
        </Route>
      </MemoryRouter>,
    );

    const lastBreadcrumb = container.querySelector('li:last-child');

    expect(lastBreadcrumb).toBeDefined();
    expect(lastBreadcrumb!.querySelector('a')).toBeNull();
  });
});
