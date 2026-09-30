import { EducationInNumbersPageContextProvider } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import _educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import EducationInNumbersSignOffPage from '@admin/pages/education-in-numbers/sign-off/EducationInNumbersSignOffPage';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';
import { educationInNumbersListRoute } from '@admin/routes/routes';
import { educationInNumbersSummaryRoute } from '@admin/routes/educationInNumbersRoutes';
import { expectLocation } from '@admin/components/testing/TestLocationContext';

jest.mock('@admin/services/educationInNumbersService');

const educationInNumbersService = _educationInNumbersService as jest.Mocked<
  typeof _educationInNumbersService
>;

describe('EducationInNumbersSignOffPage', () => {
  const testDraftPage: EinSummary = {
    id: 'page-1-id',
    title: 'Page 1 title',
    slug: 'page-1-slug',
    description: 'Page 1 description',
    version: 0,
  };

  const testPublishedPage: EinSummary = {
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

      const { user } = renderPage(testDraftPage);

      expect(await screen.findByText('Sign off')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Publish' }));

      const modal = await screen.findByRole('dialog');
      await user.click(within(modal).getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          educationInNumbersService.publishEducationInNumbersPage,
        ).toHaveBeenCalledWith('page-1-id');
      });

      await expectLocation('/education-in-numbers/page-1-id/summary');
    });
  });

  function renderPage(page: EinSummary) {
    return render(
      <TestRouterRenderer
        initialUrl={educationInNumbersListRoute.fullPath}
        route={educationInNumbersListRoute.fullPath}
        routes={[educationInNumbersSummaryRoute.fullPath]}
      >
        <EducationInNumbersPageContextProvider educationInNumbersPage={page}>
          <EducationInNumbersSignOffPage />
        </EducationInNumbersPageContextProvider>
      </TestRouterRenderer>,
    );
  }
});
