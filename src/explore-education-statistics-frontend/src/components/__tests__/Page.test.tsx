import render from '@common-test/render';
import Page from '@frontend/components/Page';
import { screen } from '@testing-library/react';
import React from 'react';

describe('Page', () => {
  test('renders breadcrumbs if back link not provided', () => {
    render(<Page title="Page Title" />);
    expect(
      screen.getByRole('navigation', {
        name: 'Breadcrumb',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Back',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders back link instead of breadcrumbs if provided', () => {
    render(<Page title="Page Title" backLinkDestination="/back-destination" />);
    expect(
      screen.queryByRole('navigation', {
        name: 'Breadcrumb',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Back',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Back',
      }),
    ).toHaveAttribute('href', '/back-destination');
  });
});
