import { EducationInNumbersPageContextProvider } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import _educationInNumbersService, {
  EducationInNumbersSummary,
} from '@admin/services/educationInNumbersService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { createMemoryHistory, MemoryHistory } from 'history';
import EducationInNumbersSignOffPage from '@admin/pages/education-in-numbers/sign-off/EducationInNumbersSignOffPage';
import { Router } from 'react-router-dom';

jest.mock('@admin/services/educationInNumbersService');

const educationInNumbersService = _educationInNumbersService as jest.Mocked<
  typeof _educationInNumbersService
>;

describe('EducationInNumbersSignOffPage', () => {
  const testDraftPage: EducationInNumbersSummary = {
    id: 'page-1-id',
    title: 'Page 1 title',
    slug: 'page-1-slug',
    description: 'Page 1 description',
    version: 0,
  };

  const testPublishedPage: EducationInNumbersSummary = {
    ...testDraftPage,
    published: '2022-03-21T10:30:00Z',
  };

  test('renders page details for a draft page correctly', async () => {
    renderPage(testDraftPage);

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

    expect(screen.getByLabelText('URL')).toHaveValue(
      'http://localhost/education-in-numbers/page-1-slug',
    );

    const summaryList = screen.getByTestId('page-list');

    expect(within(summaryList).getByTestId('Title-value')).toHaveTextContent(
      'Page 1 title',
    );

    expect(within(summaryList).getByTestId('Slug-value')).toHaveTextContent(
      'page-1-slug',
    );

    expect(
      within(summaryList).getByTestId('Description-value'),
    ).toHaveTextContent('Page 1 description');

    expect(
      within(summaryList).getByTestId('Published on-value'),
    ).toHaveTextContent('Not yet published');
  });

  test('renders page details for a published page correctly', async () => {
    renderPage(testPublishedPage);

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

    const summaryList = screen.getByTestId('page-list');

    expect(
      within(summaryList).getByTestId('Published on-value'),
    ).toHaveTextContent('10:30:00 - 21 March 2022');
  });

  test('does not show Publish button for a published page', async () => {
    renderPage(testPublishedPage);

    expect(await screen.findByText('Sign off')).toBeInTheDocument();

    expect(screen.queryByRole('button', { name: 'Publish' })).toBeNull();
  });

  describe('publishing a page', () => {
    test('clicking Publish button shows confirmation modal', async () => {
      const { user } = renderPage(testDraftPage);

      expect(await screen.findByText('Sign off')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Publish' }));

      const modal = await screen.findByRole('dialog');
      expect(
        within(modal).getByText(
          'Are you sure you want to publish Page 1 title?',
        ),
      ).toBeInTheDocument();
    });

    test('clicking confirm on modal calls the service and redirects', async () => {
      educationInNumbersService.publishEducationInNumbersPage.mockResolvedValue(
        testPublishedPage,
      );

      const history = createMemoryHistory();

      const { user } = renderPage(testDraftPage, history);

      expect(await screen.findByText('Sign off')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Publish' }));

      const modal = await screen.findByRole('dialog');
      await user.click(within(modal).getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          educationInNumbersService.publishEducationInNumbersPage,
        ).toHaveBeenCalledWith('page-1-id');
        expect(history.location.pathname).toBe(
          '/education-in-numbers/page-1-id/summary',
        );
      });
    });
  });

  function renderPage(
    page: EducationInNumbersSummary,
    history: MemoryHistory = createMemoryHistory(),
  ) {
    return render(
      <Router history={history}>
        <TestConfigContextProvider>
          <EducationInNumbersPageContextProvider educationInNumbersPage={page}>
            <EducationInNumbersSignOffPage />
          </EducationInNumbersPageContextProvider>
        </TestConfigContextProvider>
      </Router>,
    );
  }
});
