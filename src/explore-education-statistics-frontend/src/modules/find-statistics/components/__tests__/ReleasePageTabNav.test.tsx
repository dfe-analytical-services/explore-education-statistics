import render from '@common-test/render';
import ReleasePageTabNav from '@frontend/modules/find-statistics/components/ReleasePageTabNav';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { releasePageTabRouteItems } from '../../PublicationReleasePage';

describe('ReleasePageTabNav', () => {
  test('renders the nav correctly', () => {
    render(
      <ReleasePageTabNav
        activePage="explore"
        releaseUrlBase="/find-statistics/test-publication/test-slug"
        tabNavItems={releasePageTabRouteItems}
      />,
    );

    const heading = screen.getByRole('navigation', { name: 'Release' });
    expect(
      within(heading).getByRole('link', { name: 'Release home' }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/test-slug?redesign=true#content',
    ); // TODO EES-6449 - remove query param
    expect(
      within(heading).getByRole('link', { name: 'Release home' }),
    ).not.toHaveAttribute('aria-current', 'page');

    expect(
      within(heading).getByRole('link', { name: 'Explore and download data' }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/test-slug/explore#content',
    );
    expect(
      within(heading).getByRole('link', { name: 'Explore and download data' }),
    ).toHaveAttribute('aria-current', 'page');

    expect(
      within(heading).getByRole('link', { name: 'Methodology' }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/test-slug/methodology#content',
    );
    expect(
      within(heading).getByRole('link', { name: 'Methodology' }),
    ).not.toHaveAttribute('aria-current', 'page');

    expect(
      within(heading).getByRole('link', {
        name: 'Help and related information',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/test-publication/test-slug/help#content',
    );
    expect(
      within(heading).getByRole('link', {
        name: 'Help and related information',
      }),
    ).not.toHaveAttribute('aria-current', 'page');
  });
});
