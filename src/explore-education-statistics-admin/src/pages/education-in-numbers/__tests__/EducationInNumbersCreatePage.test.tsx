import _educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import { createMemoryHistory, MemoryHistory } from 'history';
import { Router } from 'react-router-dom';
import EducationInNumbersCreatePage from '@admin/pages/education-in-numbers/EducationInNumbersCreatePage';

jest.mock('@admin/services/educationInNumbersService');

const educationInNumbersService = _educationInNumbersService as jest.Mocked<
  typeof _educationInNumbersService
>;

describe('EducationInNumbersCreatePage', () => {
  const newPage: EinSummary = {
    id: 'new-page-id',
    title: 'New page title',
    slug: 'new-page-title',
    description: 'New page description',
    version: 0,
  };

  test('renders form correctly', () => {
    renderPage();

    expect(
      screen.getByRole('heading', {
        name: 'Create a new Education in Numbers page',
      }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Title')).toBeInTheDocument();
    expect(screen.getByLabelText('Description')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Create page' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('creates page successfully and redirects', async () => {
    educationInNumbersService.createEducationInNumbersPage.mockResolvedValue(
      newPage,
    );
    const history = createMemoryHistory();
    const { user } = renderPage(history);

    await user.type(screen.getByLabelText('Title'), 'New page title');
    await user.type(
      screen.getByLabelText('Description'),
      'New page description',
    );

    await user.click(screen.getByRole('button', { name: 'Create page' }));

    await waitFor(() => {
      expect(
        educationInNumbersService.createEducationInNumbersPage,
      ).toHaveBeenCalledWith({
        title: 'New page title',
        description: 'New page description',
      });
      expect(history.location.pathname).toBe(
        '/education-in-numbers/new-page-id/summary',
      );
    });
  });

  test('clicking cancel navigates back to the list page', async () => {
    const history = createMemoryHistory();
    const { user } = renderPage(history);

    await user.click(screen.getByRole('link', { name: 'Cancel' }));

    expect(history.location.pathname).toBe('/education-in-numbers');
  });

  test('shows validation error if no title is provided', async () => {
    const { user } = renderPage();

    await user.click(screen.getByRole('button', { name: 'Create page' }));

    const error = await screen.findByTestId(
      'educationInNumbersSummaryForm-title-error',
    );
    expect(error).toHaveTextContent('Enter a title');
  });

  test('shows validation error if no description is provided', async () => {
    const { user } = renderPage();

    await user.type(screen.getByLabelText('Title'), 'A title');
    await user.click(screen.getByRole('button', { name: 'Create page' }));

    const error = await screen.findByTestId(
      'educationInNumbersSummaryForm-description-error',
    );
    expect(error).toHaveTextContent('Enter a description');
  });

  function renderPage(history: MemoryHistory = createMemoryHistory()) {
    return render(
      <Router history={history}>
        <TestConfigContextProvider>
          <EducationInNumbersCreatePage />
        </TestConfigContextProvider>
      </Router>,
    );
  }
});
