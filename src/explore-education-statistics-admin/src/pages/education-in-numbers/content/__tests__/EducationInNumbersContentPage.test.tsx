import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route } from 'react-router-dom';
import { generatePath } from 'react-router';
import EducationInNumbersContentPage from '@admin/pages/education-in-numbers/content/EducationInNumbersContentPage';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';
import _einContentService, {
  EinContent,
} from '@admin/services/educationInNumbersContentService';
import _einService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import render from '@common-test/render';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { educationInNumbersRoute } from '@admin/routes/routes';

jest.mock('@admin/services/educationInNumbersService');
const einService = _einService as jest.Mocked<typeof _einService>;

jest.mock('@admin/services/educationInNumbersContentService');
const einContentService = _einContentService as jest.Mocked<
  typeof _einContentService
>;

jest.mock(
  '@admin/pages/education-in-numbers/content/components/EducationInNumbersContent',
  () => {
    const MockComponent = () => <div data-testid="EducationInNumbersContent" />;
    MockComponent.displayName = 'EducationInNumbersContent';
    return MockComponent;
  },
);

describe('EducationInNumbersContentPage', () => {
  const testPageVersion: EinSummary = {
    id: 'test-page-id',
    title: 'Test Page Title',
    slug: 'test-page-slug',
    description: 'Test page description',
    version: 0,
  };

  const testPageContent: EinContent = {
    id: 'test-page-id',
    title: 'Test Page Title',
    slug: 'test-page-slug',
    content: [],
  };

  describe('when data has loaded successfully', () => {
    test('renders the page title and content', async () => {
      einService.getEducationInNumbersPage.mockResolvedValue(testPageVersion);
      einContentService.getEducationInNumbersPageContent.mockResolvedValueOnce(
        testPageContent,
      );

      await renderPage();

      await waitFor(() => {
        expect(screen.getByTestId('page-title')).toHaveTextContent(
          'Test Page Title',
        );
        expect(
          screen.getByTestId('EducationInNumbersContent'),
        ).toBeInTheDocument();
      });
    });

    test('is in edit mode and shows the mode toggle when the page is not published', async () => {
      einService.getEducationInNumbersPage.mockResolvedValueOnce({
        ...testPageVersion,
        published: undefined,
      });
      einContentService.getEducationInNumbersPageContent.mockResolvedValueOnce(
        testPageContent,
      );

      await renderPage();

      await waitFor(() => {
        expect(screen.getByLabelText('Edit content')).toBeInTheDocument();
        expect(screen.getByLabelText('Preview content')).toBeInTheDocument();
      });
    });

    test('is in preview mode and hides the mode toggle when the page is published', async () => {
      einService.getEducationInNumbersPage.mockResolvedValueOnce({
        ...testPageVersion,
        published: '2023-08-28T12:00:00Z',
      });
      einContentService.getEducationInNumbersPageContent.mockResolvedValueOnce(
        testPageContent,
      );

      await renderPage();

      expect(screen.queryByLabelText('Edit content')).not.toBeInTheDocument();
      expect(
        screen.queryByLabelText('Preview content'),
      ).not.toBeInTheDocument();
    });
  });

  const renderPage = async () => {
    const path = generatePath<EducationInNumbersRouteParams>(
      educationInNumbersRoute.path,
      {
        educationInNumbersPageId: 'test-page-id',
      },
    );

    render(
      <MemoryRouter initialEntries={[path]}>
        <TestConfigContextProvider>
          <Route
            component={EducationInNumbersContentPage}
            path={educationInNumbersRoute.path}
          />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
