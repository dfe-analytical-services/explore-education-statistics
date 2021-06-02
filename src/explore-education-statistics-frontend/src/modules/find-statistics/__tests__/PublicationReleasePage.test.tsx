import { Release } from '@common/services/publicationService';
import PublicationReleasePage from '@frontend/modules/find-statistics/PublicationReleasePage';
import { render, screen } from '@testing-library/react';
import React from 'react';
import NationalStats from './__data__/content.api.response.national.stats.json';
import OfficialStats from './__data__/content.api.response.official.stats.json';

describe('PublicationReleasePage', () => {
  test('renders national statistics image', () => {
    const { container } = render(
      <PublicationReleasePage
        release={(NationalStats as unknown) as Release}
      />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeDefined();
  });

  test('renders national statistics section', () => {
    render(
      <PublicationReleasePage
        release={(NationalStats as unknown) as Release}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'National Statistics' }),
    ).toBeInTheDocument();
  });

  test('renders official statistics image', () => {
    const { container } = render(
      <PublicationReleasePage
        release={(OfficialStats as unknown) as Release}
      />,
    );

    expect(
      container.querySelector(
        'img[alt="UK statistics authority quality mark"]',
      ),
    ).toBeNull();
  });

  test('renders official statistics section', () => {
    render(
      <PublicationReleasePage
        release={(OfficialStats as unknown) as Release}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'National Statistics' }),
    ).not.toBeInTheDocument();
  });

  test('renders "Next update" section if this is the latest Release for a Publication and there is a valid partial date', () => {
    render(
      <PublicationReleasePage
        release={{
          ...((OfficialStats as unknown) as Release),
          latestRelease: true,
          nextReleaseDate: {
            month: 2,
            year: 2022,
          },
        }}
      />,
    );

    const nextUpdateValue = screen.getByTestId('next-update-list-item-value');
    expect(nextUpdateValue.textContent).toEqual('February 2022');
  });

  test(`doesn't render "Next update" section if there is no valid partial date`, () => {
    render(
      <PublicationReleasePage
        release={{
          ...((OfficialStats as unknown) as Release),
          latestRelease: true,
          nextReleaseDate: undefined,
        }}
      />,
    );

    expect(
      screen.queryByTestId('next-update-list-item-value'),
    ).not.toBeInTheDocument();
  });

  test(`doesn't render "Next update" section if the Release is not the latest Release for the Publication`, () => {
    const release = (OfficialStats as unknown) as Release;

    render(
      <PublicationReleasePage
        release={{
          ...release,
          latestRelease: false,
          nextReleaseDate: {
            month: 2,
            year: 2022,
          },
          publication: {
            ...release.publication,
            otherReleases: [
              {
                id: 'latest-release',
                title: 'Latest Release Title',
                slug: 'latest-release-slug',
              },
            ],
          },
        }}
      />,
    );

    expect(
      screen.queryByTestId('next-update-list-item-value'),
    ).not.toBeInTheDocument();
  });
});
