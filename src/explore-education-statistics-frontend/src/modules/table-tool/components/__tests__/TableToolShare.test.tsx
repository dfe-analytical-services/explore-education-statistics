import TableToolShare from '@frontend/modules/table-tool/components/TableToolShare';
import {
  testTableHeaders,
  testQuery,
  testSelectedPublicationWithLatestRelease,
} from '@frontend/modules/table-tool/components/__tests__/__data__/tableData';
import _permalinkService, {
  Permalink,
} from '@common/services/permalinkService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@common/services/permalinkService');

const permalinkService = _permalinkService as jest.Mocked<
  typeof _permalinkService
>;

describe('TableToolShare', () => {
  test('renders the generate button', () => {
    render(
      <TableToolShare
        query={testQuery}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    expect(screen.getByText('Save table')).toBeInTheDocument();
    screen.getByRole('button', {
      name: 'Generate shareable link',
    });
  });

  test('shows the share link when the button is clicked', async () => {
    permalinkService.createPermalink.mockResolvedValue({
      id: 'permalink-id',
    } as Permalink);
    render(
      <TableToolShare
        query={testQuery}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Generate shareable link',
      }),
    );

    await waitFor(() => {
      expect(screen.getByText('Generated share link')).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('button', {
        name: 'Generate shareable link',
      }),
    ).not.toBeInTheDocument();

    const urlInput = screen.getByTestId('permalink-generated-url');
    expect(urlInput).toBeInTheDocument();
    expect(urlInput).toHaveValue(
      'http://localhost/data-tables/permalink/permalink-id',
    );

    expect(
      screen.getByRole('button', {
        name: 'Copy link',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'View share link',
      }),
    ).toBeInTheDocument();
  });

  test('copies the link to the clipboard when the copy button is clicked', async () => {
    permalinkService.createPermalink.mockResolvedValue({
      id: 'permalink-id',
    } as Permalink);

    render(
      <TableToolShare
        query={testQuery}
        tableHeaders={testTableHeaders}
        selectedPublication={testSelectedPublicationWithLatestRelease}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Generate shareable link',
      }),
    );

    await waitFor(() => {
      expect(screen.getByText('Generated share link')).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', {
        name: 'Copy link',
      }),
    );

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith(
      'http://localhost/data-tables/permalink/permalink-id',
    );
  });
});
