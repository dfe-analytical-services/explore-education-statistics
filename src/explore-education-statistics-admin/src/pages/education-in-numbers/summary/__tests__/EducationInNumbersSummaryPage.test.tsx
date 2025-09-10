import { EducationInNumbersPageContextProvider } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import { EinSummary } from '@admin/services/educationInNumbersService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { createMemoryHistory, MemoryHistory } from 'history';
import { Router } from 'react-router-dom';
import EducationInNumbersSummaryPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryPage';

describe('EducationInNumbersSummaryPage', () => {
  const testDraftPage: EinSummary = {
    id: 'page-1-id',
    title: 'Page 1 title',
    slug: 'page-1-slug',
    description: 'Page 1 description',
    version: 0,
  };

  const testPublishedPage: EinSummary = {
    ...testDraftPage,
    published: '2022-04-01T12:00:00+00:00', // utc
  };

  const testDraftAmendmentPage: EinSummary = {
    ...testDraftPage,
    version: 1,
  };

  const testPageWithNoSlug: EinSummary = {
    ...testDraftPage,
    slug: undefined,
  };

  test('renders page details for a draft page correctly', async () => {
    renderPage(testDraftPage);

    const summaryList = screen.getByTestId('summary-list');
    expect(within(summaryList).getByTestId('Title-value')).toHaveTextContent(
      'Page 1 title',
    );
    expect(within(summaryList).getByTestId('Slug-value')).toHaveTextContent(
      'page-1-slug',
    );
    expect(
      within(summaryList).getByTestId('Description-value'),
    ).toHaveTextContent('Page 1 description');
    expect(within(summaryList).getByTestId('Status-value')).toHaveTextContent(
      'Draft',
    );
    expect(
      within(summaryList).getByTestId('Published on-value'),
    ).toHaveTextContent('Not yet published');

    expect(
      screen.getByRole('link', { name: 'Edit summary' }),
    ).toBeInTheDocument();
  });

  test('renders page details for a published page correctly', async () => {
    renderPage(testPublishedPage);

    const summaryList = screen.getByTestId('summary-list');
    expect(within(summaryList).getByTestId('Status-value')).toHaveTextContent(
      'Published',
    );
    expect(
      within(summaryList).getByTestId('Published on-value'),
    ).toHaveTextContent('13:00:00 - 1 April 2022'); // Europe/London time

    expect(screen.queryByRole('link', { name: 'Edit summary' })).toBeNull();
  });

  test('renders page details for a draft amendment correctly', async () => {
    renderPage(testDraftAmendmentPage);

    const summaryList = screen.getByTestId('summary-list');
    expect(within(summaryList).getByTestId('Status-value')).toHaveTextContent(
      'Draft amendment',
    );

    expect(screen.queryByRole('link', { name: 'Edit summary' })).toBeNull();
  });

  test('does not render "Edit summary" button if slug is undefined', async () => {
    renderPage(testPageWithNoSlug);

    expect(screen.queryByRole('link', { name: 'Edit summary' })).toBeNull();
  });

  function renderPage(
    page: EinSummary,
    history: MemoryHistory = createMemoryHistory(),
  ) {
    return render(
      <Router history={history}>
        <TestConfigContextProvider>
          <EducationInNumbersPageContextProvider educationInNumbersPage={page}>
            <EducationInNumbersSummaryPage />
          </EducationInNumbersPageContextProvider>
        </TestConfigContextProvider>
      </Router>,
    );
  }
});
