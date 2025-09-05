import render from '@common-test/render';
import ReleasePageTabNav from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ReleasePageTabNav', () => {
  test('renders the nav correctly', () => {
    render(
      <ReleasePageTabNav
        activePage="explore"
        releaseUrlBase="/find-statistics/test-publication/test-slug/"
      />,
    );

    const heading = screen.getByRole('navigation', { name: 'Release' });
    expect(
      within(heading).getByRole('link', { name: 'Release home' }),
    ).toHaveAttribute('href', '/find-statistics/test-publication/test-slug');
    expect(
      within(heading).getByRole('link', { name: 'Release home' }),
    ).not.toHaveAttribute('aria-current', 'page');
    expect(
      within(heading).getByRole('link', { name: 'Explore and download data' }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/test-slug/explore',
    );
    expect(
      within(heading).getByRole('link', { name: 'Explore and download data' }),
    ).toHaveAttribute('aria-current', 'page');
  });
});
