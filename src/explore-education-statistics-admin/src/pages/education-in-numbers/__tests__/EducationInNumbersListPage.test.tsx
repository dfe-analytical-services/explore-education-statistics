import _educationInNumbersService, {
  EducationInNumbersSummary,
  EducationInNumbersSummaryWithPrevVersion,
} from '@admin/services/educationInNumbersService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { createMemoryHistory, MemoryHistory } from 'history';
import { Router } from 'react-router-dom';
import EducationInNumbersListPage from '@admin/pages/education-in-numbers/EducationInNumbersListPage';
import { useQuery } from '@tanstack/react-query';

// Mock the service and the react-query hook
jest.mock('@admin/services/educationInNumbersService');
jest.mock('@tanstack/react-query', () => ({
  ...jest.requireActual('@tanstack/react-query'),
  useQuery: jest.fn(),
}));

const educationInNumbersService = _educationInNumbersService as jest.Mocked<
  typeof _educationInNumbersService
>;
const useQueryMock = useQuery as jest.Mock;

describe('EducationInNumbersListPage', () => {
  const testDraftPage: EducationInNumbersSummaryWithPrevVersion = {
    id: 'draft-page-id',
    title: 'Draft page',
    slug: 'draft-page',
    description: 'Draft page description',
    version: 0,
  };

  const testDraftAmendmentPage: EducationInNumbersSummaryWithPrevVersion = {
    id: 'draft-amendment-id',
    title: 'Draft amendment page',
    slug: 'draft-amendment-page',
    description: 'Draft amendment description',
    version: 2,
    previousVersionId: 'prev-version-id',
  };

  const testPublishedPage: EducationInNumbersSummaryWithPrevVersion = {
    id: 'published-page-id',
    title: 'Published page',
    slug: 'published-page',
    description: 'Published page description',
    published: '2025-04-21T10:30:00.0000000+00:00', // utc
    version: 1,
  };

  const testPages = [testDraftPage, testDraftAmendmentPage, testPublishedPage];

  test('renders loading spinner when loading', () => {
    useQueryMock.mockReturnValue({
      data: [],
      isLoading: true,
      refetch: jest.fn(),
    });

    renderPage();

    expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
  });

  test('renders draft, draft amendment, and published rows correctly', async () => {
    const refetch = jest.fn();
    useQueryMock.mockReturnValue({
      data: testPages,
      isLoading: false,
      refetch,
    });

    renderPage();

    const table = await screen.findByTestId('education-in-numbers-table');
    const rows = within(table).getAllByRole('row');
    expect(rows).toHaveLength(4); // 1 header row + 3 data rows

    // Check draft row
    const draftRow = within(table).getByRole('row', { name: /Draft page/ });
    const draftRowCells = within(draftRow).getAllByRole('cell');
    expect(draftRowCells).toHaveLength(6);
    expect(draftRowCells[0].textContent).toBe('Draft page');
    expect(draftRowCells[1].textContent).toBe('draft-page');
    expect(draftRowCells[2].textContent).toBe('Draft');
    expect(draftRowCells[3].textContent).toBe('Not yet published');
    expect(draftRowCells[4].textContent).toBe('0');
    expect(
      within(draftRowCells[5]).getByRole('link', { name: 'Edit' }),
    ).toBeInTheDocument();
    expect(
      within(draftRowCells[5]).getByRole('button', { name: 'Delete' }),
    ).toBeInTheDocument();

    // Check draft amendment row
    const amendmentRow = within(table).getByRole('row', {
      name: /Draft amendment page/,
    });
    const amendmentRowCells = within(amendmentRow).getAllByRole('cell');
    expect(amendmentRowCells).toHaveLength(6);
    expect(amendmentRowCells[0].textContent).toBe('Draft amendment page');
    expect(amendmentRowCells[1].textContent).toBe('draft-amendment-page');
    expect(amendmentRowCells[2].textContent).toBe('Draft amendment');
    expect(amendmentRowCells[3].textContent).toBe('Not yet published');
    expect(amendmentRowCells[4].textContent).toBe('2');
    expect(
      within(amendmentRowCells[5]).getByRole('link', {
        name: 'View currently published page',
      }),
    ).toBeInTheDocument();
    expect(
      within(amendmentRowCells[5]).getByRole('link', { name: 'Edit' }),
    ).toBeInTheDocument();
    expect(
      within(amendmentRowCells[5]).getByRole('button', {
        name: 'Cancel amendment',
      }),
    ).toBeInTheDocument();

    // Check published row
    const publishedRow = within(table).getByRole('row', {
      name: /Published page/,
    });
    const publishedRowCells = within(publishedRow).getAllByRole('cell');
    expect(publishedRowCells).toHaveLength(6);
    expect(publishedRowCells[0].textContent).toBe('Published page');
    expect(publishedRowCells[1].textContent).toBe('published-page');
    expect(publishedRowCells[2].textContent).toBe('Published');
    expect(publishedRowCells[3].textContent).toBe('11:30:00 - 21 April 2025'); // adjusted for BST
    expect(publishedRowCells[4].textContent).toBe('1');
    expect(
      within(publishedRowCells[5]).getByRole('link', { name: 'View' }),
    ).toBeInTheDocument();
    expect(
      within(publishedRowCells[5]).getByRole('button', {
        name: 'Create amendment',
      }),
    ).toBeInTheDocument();
  });

  test("'View currently published page' link navigates to the previous version's summary page", async () => {
    useQueryMock.mockReturnValue({
      data: [testDraftAmendmentPage],
      isLoading: false,
      refetch: jest.fn(),
    });
    const history = createMemoryHistory();
    const { user } = renderPage(history);

    const table = await screen.findByTestId('education-in-numbers-table');
    const row = within(table).getByRole('row', {
      name: /Draft amendment page/,
    });
    const link = within(row).getByRole('link', {
      name: 'View currently published page',
    });

    await user.click(link);

    expect(history.location.pathname).toBe(
      '/education-in-numbers/prev-version-id/summary',
    );
  });

  test('Edit/View link navigates to the correct summary page', async () => {
    useQueryMock.mockReturnValue({
      data: testPages,
      isLoading: false,
      refetch: jest.fn(),
    });
    const history = createMemoryHistory();
    const { user } = renderPage(history);

    const table = await screen.findByTestId('education-in-numbers-table');

    // Test 'Edit' link
    const draftRow = within(table).getByRole('row', { name: /Draft page/ });
    await user.click(within(draftRow).getByRole('link', { name: 'Edit' }));
    expect(history.location.pathname).toBe(
      '/education-in-numbers/draft-page-id/summary',
    );

    // Test 'View' link
    const publishedRow = within(table).getByRole('row', {
      name: /Published page/,
    });
    await user.click(within(publishedRow).getByRole('link', { name: 'View' }));
    expect(history.location.pathname).toBe(
      '/education-in-numbers/published-page-id/summary',
    );
  });

  test("'Create amendment' button calls service and navigates to the new summary page", async () => {
    const newAmendment: EducationInNumbersSummary = {
      id: 'new-amendment-id',
      title: 'Published page',
      slug: 'published-page',
      description: 'New amendment description',
      version: 2,
    };
    educationInNumbersService.createEducationInNumbersPageAmendment.mockResolvedValue(
      newAmendment,
    );
    useQueryMock.mockReturnValue({
      data: [testPublishedPage],
      isLoading: false,
      refetch: jest.fn(),
    });
    const history = createMemoryHistory();
    const { user } = renderPage(history);

    const table = await screen.findByTestId('education-in-numbers-table');
    const row = within(table).getByRole('row', { name: /Published page/ });
    await user.click(
      within(row).getByRole('button', { name: 'Create amendment' }),
    );

    await waitFor(() => {
      expect(
        educationInNumbersService.createEducationInNumbersPageAmendment,
      ).toHaveBeenCalledWith('published-page-id');
      expect(history.location.pathname).toBe(
        '/education-in-numbers/new-amendment-id/summary',
      );
    });
  });

  test('shows draft deletion modal and refetches on confirm', async () => {
    const refetch = jest.fn();
    useQueryMock.mockReturnValue({
      data: [testDraftPage],
      isLoading: false,
      refetch,
    });
    const { user } = renderPage();

    const table = await screen.findByTestId('education-in-numbers-table');
    const row = within(table).getByRole('row', { name: /Draft page/ });
    await user.click(within(row).getByRole('button', { name: 'Delete' }));

    const modal = await screen.findByRole('dialog');
    expect(
      within(modal).getByText('Are you sure you want to delete Draft page?'),
    ).toBeInTheDocument();

    await user.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        educationInNumbersService.deleteEducationInNumbersPage,
      ).toHaveBeenCalledWith('draft-page-id');
      expect(refetch).toHaveBeenCalledTimes(1);
    });
  });

  test('shows amendment cancellation modal and refetches on confirm', async () => {
    const refetch = jest.fn();
    useQueryMock.mockReturnValue({
      data: [testDraftAmendmentPage],
      isLoading: false,
      refetch,
    });
    const { user } = renderPage();

    const table = await screen.findByTestId('education-in-numbers-table');
    const row = within(table).getByRole('row', {
      name: /Draft amendment page/,
    });
    await user.click(
      within(row).getByRole('button', { name: 'Cancel amendment' }),
    );

    const modal = await screen.findByRole('dialog');
    expect(
      within(modal).getByText(
        'Are you sure you want to cancel the amendment to Draft amendment page?',
      ),
    ).toBeInTheDocument();

    await user.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        educationInNumbersService.deleteEducationInNumbersPage,
      ).toHaveBeenCalledWith('draft-amendment-id');
      expect(refetch).toHaveBeenCalledTimes(1);
    });
  });

  function renderPage(history: MemoryHistory = createMemoryHistory()) {
    return render(
      <Router history={history}>
        <TestConfigContextProvider>
          <EducationInNumbersListPage />
        </TestConfigContextProvider>
      </Router>,
    );
  }
});
