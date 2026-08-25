import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';
import ApiQueryStatTile, { ApiQueryStatTileProps } from '../ApiQueryStatTile';

const renderLink: ApiQueryStatTileProps['renderLink'] = ({
  children,
  className,
  testId,
  to,
}) => (
  <a className={className} data-testid={testId} href={to}>
    {children}
  </a>
);

describe('ApiQueryStatTile', () => {
  test('Renders link text using publication and release labels, and slugs in the url', () => {
    render(
      <ApiQueryStatTile
        renderLink={renderLink}
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

    const link = screen.getByTestId('api-query-stat-tile-link');

    expect(link).toHaveTextContent('Publication title');
    expect(link).toHaveAttribute(
      'href',
      '/find-statistics/publication-slug/release-slug',
    );

    expect(
      screen.getByTestId('api-query-stat-tile-link-release'),
    ).toHaveTextContent('Academic year 2023/24');
  });

  test('Formats the statistic using the indicator unit and decimal places', () => {
    render(
      <ApiQueryStatTile
        renderLink={renderLink}
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
      <ApiQueryStatTile
        renderLink={renderLink}
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
      <ApiQueryStatTile
        renderLink={renderLink}
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

  test('Renders a `Not the latest data` tag when the tile is not on the latest data set version', () => {
    render(
      <ApiQueryStatTile
        renderLink={renderLink}
        tile={{
          id: 'tile-1',
          type: 'ApiQueryStatTile',
          order: 0,
          title: 'Tile title',
          statistic: '93',
          dataSetId: 'data-set-1',
          isLatestVersion: false,
        }}
      />,
    );

    expect(
      screen.getByTestId('api-query-stat-tile-not-latest-tag'),
    ).toHaveTextContent('Not the latest data');
  });

  test('Does not render a `Not the latest data` tag when the tile is on the latest data set version', () => {
    render(
      <ApiQueryStatTile
        renderLink={renderLink}
        tile={{
          id: 'tile-1',
          type: 'ApiQueryStatTile',
          order: 0,
          title: 'Tile title',
          statistic: '93',
          dataSetId: 'data-set-1',
          isLatestVersion: true,
        }}
      />,
    );

    expect(
      screen.queryByTestId('api-query-stat-tile-not-latest-tag'),
    ).not.toBeInTheDocument();
  });
});
