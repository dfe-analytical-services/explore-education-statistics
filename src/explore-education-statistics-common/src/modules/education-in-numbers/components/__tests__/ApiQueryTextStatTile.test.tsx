import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';
import ApiQueryTextStatTile from '../ApiQueryTextStatTile';

describe('ApiQueryTextStatTile', () => {
  test('Renders link text using publication and release labels, and slugs in the url', () => {
    render(
      <ApiQueryTextStatTile
        publicAppUrl="https://example.com"
        tile={{
          id: 'tile-1',
          type: 'ApiQueryStatTile',
          order: 0,
          title: 'Tile title',
          statistic: '93',
          dataSetId: 'data-set-1',
          isLatestVersion: true,
          publicationSlug: 'publication-slug',
          releaseSlug: 'release-slug',
          publicationLabel: 'Publication title',
          releaseLabel: 'Academic year 2023/24',
        }}
      />,
    );

    expect(
      screen.getByTestId('api-query-stat-tile-link-publication'),
    ).toHaveTextContent('Publication title');
    expect(
      screen.getByTestId('api-query-stat-tile-link-release'),
    ).toHaveTextContent('Academic year 2023/24');

    expect(screen.getByTestId('api-query-stat-tile-link')).toHaveAttribute(
      'href',
      'https://example.com/find-statistics/publication-slug/release-slug',
    );
  });

  test('Formats the statistic using the indicator unit and decimal places', () => {
    render(
      <ApiQueryTextStatTile
        publicAppUrl="https://example.com"
        tile={{
          id: 'tile-1',
          type: 'ApiQueryStatTile',
          order: 0,
          title: 'Tile title',
          statistic: '93.456',
          indicatorUnit: '%',
          decimalPlaces: 1,
          dataSetId: 'data-set-1',
          isLatestVersion: true,
        }}
      />,
    );

    expect(
      screen.getByTestId('api-query-stat-tile-statistic'),
    ).toHaveTextContent('93.5%');
  });

  test('Does not render a link when labels are not available, even if slugs are', () => {
    render(
      <ApiQueryTextStatTile
        publicAppUrl="https://example.com"
        tile={{
          id: 'tile-1',
          type: 'ApiQueryStatTile',
          order: 0,
          title: 'Tile title',
          dataSetId: 'data-set-1',
          isLatestVersion: true,
          publicationSlug: 'publication-slug',
          releaseSlug: 'release-slug',
        }}
      />,
    );

    expect(
      screen.queryByTestId('api-query-stat-tile-link'),
    ).not.toBeInTheDocument();
  });

  test('Does not render a link when publication or release is not set', () => {
    render(
      <ApiQueryTextStatTile
        publicAppUrl="https://example.com"
        tile={{
          id: 'tile-1',
          type: 'ApiQueryStatTile',
          order: 0,
          title: 'Tile title',
          dataSetId: 'data-set-1',
          isLatestVersion: true,
        }}
      />,
    );

    expect(
      screen.queryByTestId('api-query-stat-tile-link'),
    ).not.toBeInTheDocument();
  });
});
