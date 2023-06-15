import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import React from 'react';
import Breadcrumbs from '../Breadcrumbs';

describe('Breadcrumbs', () => {
  test('renders correctly with just home breadcrumb', () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Breadcrumbs breadcrumbs={[]} />
      </MemoryRouter>,
    );

    const breadcrumbs = container.querySelectorAll('li');

    expect(breadcrumbs).toHaveLength(1);
    expect(breadcrumbs[0].textContent).toBe('Home');
    expect(breadcrumbs[0].querySelector('a')).toHaveAttribute(
      'href',
      '/dashboard',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with multiple breadcrumbs', () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/publications', '/test-publication']}>
        <Breadcrumbs
          breadcrumbs={[
            {
              link: '/publications',
              name: 'Publications',
            },
            {
              link: '/test-publication',
              name: 'Test publication',
            },
          ]}
        />
      </MemoryRouter>,
    );

    const breadcrumbs = container.querySelectorAll('li');

    expect(breadcrumbs).toHaveLength(3);
    expect(breadcrumbs[0].textContent).toBe('Home');
    expect(breadcrumbs[0].querySelector('a')).toHaveAttribute(
      'href',
      '/dashboard',
    );
    expect(breadcrumbs[1].textContent).toBe('Publications');
    expect(breadcrumbs[1].querySelector('a')).toHaveAttribute(
      'href',
      '/publications',
    );
    expect(breadcrumbs[2].textContent).toBe('Test publication');

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render a link if breadcrumb is missing a link', () => {
    const { container } = render(
      <MemoryRouter>
        <Breadcrumbs
          breadcrumbs={[
            {
              name: 'Publications',
            },
            {
              link: '/test-publication',
              name: 'Test publication',
            },
          ]}
        />
        ,
      </MemoryRouter>,
    );

    const breadcrumbs = container.querySelectorAll('li');

    expect(breadcrumbs).toHaveLength(3);
    expect(breadcrumbs[1].textContent).toBe('Publications');
    expect(breadcrumbs[1].querySelector('a')).toBe(null);
  });

  test('does not render last breadcrumb as a link', () => {
    const { container } = render(
      <MemoryRouter>
        <Breadcrumbs
          breadcrumbs={[
            {
              link: '/publications',
              name: 'Publications',
            },
            {
              link: '/test-publication',
              name: 'Test publication',
            },
          ]}
        />
        ,
      </MemoryRouter>,
    );

    const lastBreadcrumb = container.querySelector(
      'li:last-child',
    ) as HTMLElement;

    expect(lastBreadcrumb).toBeDefined();
    expect(lastBreadcrumb.querySelector('a')).toBeNull();
  });

  test('does not render Home breadcrumb if empty path', () => {
    const { container } = render(
      <MemoryRouter>
        <Breadcrumbs
          homePath=""
          breadcrumbs={[
            {
              link: '/publications',
              name: 'Publications',
            },
          ]}
        />
        ,
      </MemoryRouter>,
    );

    const breadcrumbs = container.querySelectorAll('li');

    expect(breadcrumbs).toHaveLength(1);
    expect(breadcrumbs[0].textContent).toBe('Publications');
    expect(breadcrumbs[0].querySelector('a')).toBe(null);
  });
});
