import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import { useQuery } from '@tanstack/react-query';
import { MemoryRouter, Route } from 'react-router-dom';
import { generatePath } from 'react-router';
import EducationInNumbersContentPage from '@admin/pages/education-in-numbers/content/EducationInNumbersContentPage';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';
import { EinContent } from '@admin/services/educationInNumbersContentService';
import { EducationInNumbersSummary } from '@admin/services/educationInNumbersService';
import render from '@common-test/render';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { educationInNumbersRoute } from '@admin/routes/routes';

jest.mock('@tanstack/react-query', () => ({
  ...jest.requireActual('@tanstack/react-query'),
  useQuery: jest.fn(),
}));

jest.mock(
  '@admin/pages/education-in-numbers/content/components/EducationInNumbersContent',
  () => {
    const MockComponent = () => <div data-testid="EducationInNumbersContent" />;
    MockComponent.displayName = 'EducationInNumbersContent';
    return MockComponent;
  },
);

const useQueryMock = useQuery as jest.Mock;

describe('EducationInNumbersContentPage', () => {
  const testPageVersion: EducationInNumbersSummary = {
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

  beforeEach(() => {
    useQueryMock.mockReset();
  });

  test('renders loading spinner while fetching data', async () => {
    useQueryMock.mockReturnValue({
      isLoading: true,
    });

    await renderPage();

    expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
  });

  describe('when data has loaded successfully', () => {
    test('renders the page title and content', async () => {
      useQueryMock.mockReturnValueOnce({
        data: { ...testPageVersion },
        isLoading: false,
      });
      useQueryMock.mockReturnValueOnce({
        data: { ...testPageContent },
        isLoading: false,
      });

      await renderPage();

      expect(screen.getByTestId('page-title')).toHaveTextContent(
        'Test Page Title',
      );
      expect(
        screen.getByTestId('EducationInNumbersContent'),
      ).toBeInTheDocument();
    });

    test('is in edit mode and shows the mode toggle when the page is not published', async () => {
      useQueryMock.mockReturnValueOnce({
        data: { ...testPageVersion, published: undefined },
        isLoading: false,
      });
      useQueryMock.mockReturnValueOnce({
        data: { ...testPageContent },
        isLoading: false,
      });

      await renderPage();

      expect(screen.getByLabelText('Edit content')).toBeInTheDocument();
      expect(screen.getByLabelText('Preview content')).toBeInTheDocument();
    });

    test('is in preview mode and hides the mode toggle when the page is published', async () => {
      useQueryMock.mockReturnValueOnce({
        data: { ...testPageVersion, published: '2023-08-28T12:00:00Z' },
        isLoading: false,
      });
      useQueryMock.mockReturnValueOnce({
        data: { ...testPageContent },
        isLoading: false,
      });

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
    await waitFor(() => {
      expect(useQueryMock).toHaveBeenCalled();
    });
  };
});
