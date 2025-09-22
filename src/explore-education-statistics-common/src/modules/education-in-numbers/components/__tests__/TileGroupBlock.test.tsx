import render from '@common-test/render';
import { screen } from '@testing-library/react';
import React from 'react';
import TileGroupBlock from '../TileGroupBlock';

describe('TileGroupBlock', () => {
  test('Renders correctly', () => {
    render(
      <TileGroupBlock
        block={{
          type: 'TileGroupBlock',
          id: 'tile-group-block-1',
          order: 0,
          tiles: [
            {
              id: 'tile-1',
              type: 'FreeTextStatTile',
              order: 0,
              title: 'Tile 1 title',
              statistic: '1000',
              trend: 'Tile 1 trend',
              linkText: 'Tile 1 link text',
              linkUrl: 'https://example.com/tile-1',
            },
            {
              id: 'tile-2',
              type: 'FreeTextStatTile',
              order: 1,
              title: 'Tile 2 title',
              statistic: '2000',
              trend: 'Tile 2 trend',
              linkText: 'Tile 2 link text',
              linkUrl: 'https://example.com/tile-2',
            },
          ],
          title: 'Test Tile Group Block Title',
        }}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Test Tile Group Block Title' }),
    ).toBeInTheDocument();

    expect(screen.getAllByTestId('free-text-stat-tile-tile')).toHaveLength(2);
  });

  test('Renders correctly with no title and one tile', () => {
    render(
      <TileGroupBlock
        block={{
          type: 'TileGroupBlock',
          id: 'tile-group-block-1',
          order: 0,
          tiles: [
            {
              id: 'tile-1',
              type: 'FreeTextStatTile',
              order: 0,
              title: 'Tile 1 title',
              statistic: '1000',
              trend: 'Tile 1 trend',
              linkText: 'Tile 1 link text',
              linkUrl: 'https://example.com/tile-1',
            },
          ],
        }}
      />,
    );

    expect(
      screen.queryByRole('heading', { name: 'Test Tile Group Block Title' }),
    ).not.toBeInTheDocument();

    expect(screen.getAllByTestId('free-text-stat-tile-tile')).toHaveLength(1);
  });
});
