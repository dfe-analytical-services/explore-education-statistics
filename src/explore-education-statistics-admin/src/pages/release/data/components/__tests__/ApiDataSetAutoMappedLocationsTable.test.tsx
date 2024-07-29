import ApiDataSetAutoMappedLocationsTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedLocationsTable';
import { AutoMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetAutoMappedLocationsTable', () => {
  const testLocations: AutoMappedLocation[] = [
    {
      candidate: { label: 'Location 1', code: 'location-1-code' },
      mapping: {
        candidateKey: 'Location1Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 1',
          code: 'location-1-code',
        },
      },
    },
    {
      candidate: { label: 'Location 2', code: 'location-2-code' },
      mapping: {
        candidateKey: 'Location2Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 2',
          code: 'location-2-code',
        },
      },
    },
    {
      candidate: { label: 'Location 3', code: 'location-3-code' },
      mapping: {
        candidateKey: 'Location3Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 3',
          code: 'location-3-code',
        },
      },
    },
    {
      candidate: { label: 'Location 4', code: 'location-4-code' },
      mapping: {
        candidateKey: 'Location4Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 4',
          code: 'location-4-code',
        },
      },
    },
    {
      candidate: { label: 'Location 5', code: 'location-5-code' },
      mapping: {
        candidateKey: 'Location5Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 5',
          code: 'location-5-code',
        },
      },
    },
    {
      candidate: { label: 'Location 6', code: 'location-6-code' },
      mapping: {
        candidateKey: 'Location6Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 6',
          code: 'location-6-code',
        },
      },
    },
    {
      candidate: { label: 'Location 7', code: 'location-7-code' },
      mapping: {
        candidateKey: 'Location7Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 7',
          code: 'location-7-code',
        },
      },
    },
    {
      candidate: { label: 'Location 8', code: 'location-8-code' },
      mapping: {
        candidateKey: 'Location8Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 8',
          code: 'location-8-code',
        },
      },
    },
    {
      candidate: { label: 'Location 9', code: 'location-9-code' },
      mapping: {
        candidateKey: 'Location9Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 9',
          code: 'location-9-code',
        },
      },
    },
    {
      candidate: { label: 'Location 10', code: 'location-10-code' },
      mapping: {
        candidateKey: 'Location10Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 10',
          code: 'location-10-code',
        },
      },
    },
    {
      candidate: { label: 'Location 11', code: 'location-11-code' },
      mapping: {
        candidateKey: 'Location11Key',
        type: 'AutoMapped',
        source: {
          label: 'Location 11',
          code: 'location-11-code',
        },
      },
    },
  ];

  test('renders the first 10 auto mapped locations', () => {
    render(<ApiDataSetAutoMappedLocationsTable locations={testLocations} />);
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(11);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Location 1');
    expect(row1Cells[0]).toHaveTextContent('location-1-code');
    expect(row1Cells[1]).toHaveTextContent('Location 1');
    expect(row1Cells[1]).toHaveTextContent('location-1-code');
    expect(row1Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 1',
      }),
    ).toBeInTheDocument();

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Location 2');
    expect(row2Cells[0]).toHaveTextContent('location-2-code');
    expect(row2Cells[1]).toHaveTextContent('Location 2');
    expect(row2Cells[1]).toHaveTextContent('location-2-code');
    expect(row2Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 2',
      }),
    ).toBeInTheDocument();

    // Row 3
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Location 3');
    expect(row3Cells[0]).toHaveTextContent('location-3-code');
    expect(row3Cells[1]).toHaveTextContent('Location 3');
    expect(row3Cells[1]).toHaveTextContent('location-3-code');
    expect(row3Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 3',
      }),
    ).toBeInTheDocument();

    // Row 4
    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Location 4');
    expect(row4Cells[0]).toHaveTextContent('location-4-code');
    expect(row4Cells[1]).toHaveTextContent('Location 4');
    expect(row4Cells[1]).toHaveTextContent('location-4-code');
    expect(row4Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 4',
      }),
    ).toBeInTheDocument();

    // Row 5
    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('Location 5');
    expect(row5Cells[0]).toHaveTextContent('location-5-code');
    expect(row5Cells[1]).toHaveTextContent('Location 5');
    expect(row5Cells[1]).toHaveTextContent('location-5-code');
    expect(row5Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row5Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 5',
      }),
    ).toBeInTheDocument();

    // Row 6
    const row6Cells = within(rows[6]).getAllByRole('cell');
    expect(row6Cells[0]).toHaveTextContent('Location 6');
    expect(row6Cells[0]).toHaveTextContent('location-6-code');
    expect(row6Cells[1]).toHaveTextContent('Location 6');
    expect(row6Cells[1]).toHaveTextContent('location-6-code');
    expect(row6Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row6Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 6',
      }),
    ).toBeInTheDocument();

    // Row 7
    const row7Cells = within(rows[7]).getAllByRole('cell');
    expect(row7Cells[0]).toHaveTextContent('Location 7');
    expect(row7Cells[0]).toHaveTextContent('location-7-code');
    expect(row7Cells[1]).toHaveTextContent('Location 7');
    expect(row7Cells[1]).toHaveTextContent('location-7-code');
    expect(row7Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row7Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 7',
      }),
    ).toBeInTheDocument();

    // Row 8
    const row8Cells = within(rows[8]).getAllByRole('cell');
    expect(row8Cells[0]).toHaveTextContent('Location 8');
    expect(row8Cells[0]).toHaveTextContent('location-8-code');
    expect(row8Cells[1]).toHaveTextContent('Location 8');
    expect(row8Cells[1]).toHaveTextContent('location-8-code');
    expect(row8Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row8Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 8',
      }),
    ).toBeInTheDocument();

    // Row 9
    const row9Cells = within(rows[9]).getAllByRole('cell');
    expect(row9Cells[0]).toHaveTextContent('Location 9');
    expect(row9Cells[0]).toHaveTextContent('location-9-code');
    expect(row9Cells[1]).toHaveTextContent('Location 9');
    expect(row9Cells[1]).toHaveTextContent('location-9-code');
    expect(row9Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row9Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 9',
      }),
    ).toBeInTheDocument();

    // Row 10
    const row10Cells = within(rows[10]).getAllByRole('cell');
    expect(row10Cells[0]).toHaveTextContent('Location 10');
    expect(row10Cells[0]).toHaveTextContent('location-10-code');
    expect(row10Cells[1]).toHaveTextContent('Location 10');
    expect(row10Cells[1]).toHaveTextContent('location-10-code');
    expect(row10Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row10Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 10',
      }),
    ).toBeInTheDocument();
  });
});
