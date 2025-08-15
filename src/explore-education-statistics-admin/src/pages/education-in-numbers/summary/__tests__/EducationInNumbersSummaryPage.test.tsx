import { EducationInNumbersPageContextProvider } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import { EducationInNumbersSummary } from '@admin/services/educationInNumbersService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { createMemoryHistory, MemoryHistory } from 'history';
import { Router } from 'react-router-dom';
import EducationInNumbersSummaryPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryPage';

describe('EducationInNumbersSummaryPage', () => {
  const testDraftPage: EducationInNumbersSummary = {
    id: 'page-1-id',
    title: 'Page 1 title',
    slug: 'page-1-slug',
    description: 'Page 1 description',
    version: 0,
  };

  const testPublishedPage: EducationInNumbersSummary = {
    ...testDraftPage,
    published: '2022-04-01T12:00:00+00:00', // utc
  };

  const testDraftAmendmentPage: EducationInNumbersSummary = {
    ...testDraftPage,
    version: 1,
  };

  const testPageWithNoSlug: EducationInNumbersSummary = {
    ...testDraftPage,
    slug: undefined,
  };

  test('renders page details for a draft page correctly', async () => {
    renderPage(testDraftPage);

    const summaryList = screen.getByTestId('summary-list');
    expect(
      within(summaryList).getByText('Title').nextElementSibling,
    ).toHaveTextContent('Page 1 title');
    expect(
      within(summaryList).getByText('Slug').nextElementSibling,
    ).toHaveTextContent('page-1-slug');
    expect(
      within(summaryList).getByText('Description').nextElementSibling,
    ).toHaveTextContent('Page 1 description');
    expect(
      within(summaryList).getByText('Status').nextElementSibling,
    ).toHaveTextContent('Draft');
    expect(
      within(summaryList).getByText('Published on').nextElementSibling,
    ).toHaveTextContent('Not yet published');

    expect(
      screen.getByRole('link', { name: 'Edit summary' }),
    ).toBeInTheDocument();
  });

  test('renders page details for a published page correctly', async () => {
    renderPage(testPublishedPage);

    const summaryList = screen.getByTestId('summary-list');
    expect(
      within(summaryList).getByText('Status').nextElementSibling,
    ).toHaveTextContent('Published');
    expect(
      within(summaryList).getByText('Published on').nextElementSibling,
    ).toHaveTextContent('13:00:00 - 1 April 2022'); // Europe/London time

    expect(screen.queryByRole('link', { name: 'Edit summary' })).toBeNull();
  });

  test('renders page details for a draft amendment correctly', async () => {
    renderPage(testDraftAmendmentPage);

    const summaryList = screen.getByTestId('summary-list');
    expect(
      within(summaryList).getByText('Status').nextElementSibling,
    ).toHaveTextContent('Draft amendment');

    expect(screen.queryByRole('link', { name: 'Edit summary' })).toBeNull();
  });

  test('does not render "Edit summary" button if slug is undefined', async () => {
    renderPage(testPageWithNoSlug);

    expect(screen.queryByRole('link', { name: 'Edit summary' })).toBeNull();
  });

  function renderPage(
    page: EducationInNumbersSummary,
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
