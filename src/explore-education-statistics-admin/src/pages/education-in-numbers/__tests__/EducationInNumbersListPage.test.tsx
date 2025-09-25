import _educationInNumbersService, {
  EinSummaryWithPrevVersion,
} from '@admin/services/educationInNumbersService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
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
  const testRootPage: EinSummaryWithPrevVersion = {
    id: 'root-page-id',
    title: 'Education in Numbers',
    slug: '',
    description: 'Root page description',
    version: 0,
  };

  const testDraftPage: EinSummaryWithPrevVersion = {
    id: 'draft-page-id',
    title: 'Draft page',
    slug: 'draft-page',
    description: 'Draft page description',
    version: 0,
  };

  const testDraftAmendmentPage: EinSummaryWithPrevVersion = {
    id: 'draft-amendment-id',
    title: 'Draft amendment page',
    slug: 'draft-amendment-page',
    description: 'Draft amendment description',
    version: 2,
    previousVersionId: 'prev-version-id',
  };

  test('renders loading spinner when loading', () => {
    useQueryMock.mockReturnValue({
      data: [],
      isLoading: true,
      refetch: jest.fn(),
    });

    renderPage();

    expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
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

    expect(
      educationInNumbersService.deleteEducationInNumbersPage,
    ).toHaveBeenCalledWith('draft-page-id');
    expect(refetch).toHaveBeenCalledTimes(1);
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

    expect(
      educationInNumbersService.deleteEducationInNumbersPage,
    ).toHaveBeenCalledWith('draft-amendment-id');
    expect(refetch).toHaveBeenCalledTimes(1);
  });

  test('shows reorder button if two or more subpages', () => {
    useQueryMock.mockReturnValue({
      data: [testRootPage, testDraftPage, testDraftAmendmentPage],
      isLoading: false,
      refetch: jest.fn(),
    });

    renderPage();

    expect(
      screen.getByRole('button', { name: 'Reorder pages' }),
    ).toBeInTheDocument();
  });

  test('does not show reorder button if less than two subpages', () => {
    useQueryMock.mockReturnValue({
      data: [testRootPage, testDraftPage],
      isLoading: false,
      refetch: jest.fn(),
    });

    renderPage();

    expect(
      screen.queryByRole('button', { name: 'Reorder pages' }),
    ).not.toBeInTheDocument();
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
