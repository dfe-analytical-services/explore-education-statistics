import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { generatePath } from 'react-router';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import { MethodologyContextProvider } from '@admin/pages/methodology/contexts/MethodologyContext';
import { methodologySummaryRoute } from '@admin/routes/methodologyRoutes';
import testMethodology from '@admin/pages/methodology/edit-methodology/__tests__/__data__/testMethodologyVersionsAmendmentsAndContents';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';

describe('MethodologySummaryPage', () => {
  test('renders methodology summary page correctly', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology summary')).toBeInTheDocument();

      expect(screen.getByTestId('Title-key')).toHaveTextContent('Title');
      expect(screen.getByTestId('Title-value')).toHaveTextContent(
        'Test methodology',
      );

      expect(screen.getByTestId('Published on-key')).toHaveTextContent(
        'Published on',
      );
      expect(screen.getByTestId('Published on-value')).toHaveTextContent(
        'Not yet published',
      );

      expect(screen.getByTestId('Owning publication-key')).toHaveTextContent(
        'Owning publication',
      );
      expect(screen.getByTestId('Owning publication-value')).toHaveTextContent(
        'Publication title',
      );
    });
  });

  test('renders other publications list correctly', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology summary')).toBeInTheDocument();

      expect(screen.getByTestId('Other publications-key')).toHaveTextContent(
        'Other publications',
      );

      const otherPublications = screen.getAllByTestId('other-publication-item');

      expect(otherPublications).toHaveLength(2);
      expect(otherPublications[0]).toHaveTextContent(
        'Other publication title 1',
      );
      expect(otherPublications[1]).toHaveTextContent(
        'Other publication title 2',
      );
    });
  });

  function renderPage() {
    render(
      <TestRouterRenderer
        initialUrl={generatePath(methodologySummaryRoute.fullPath, {
          methodologyId: testMethodology.id,
        })}
        route={methodologySummaryRoute.fullPath}
      >
        <MethodologyContextProvider methodology={testMethodology}>
          <MethodologySummaryPage />
        </MethodologyContextProvider>
      </TestRouterRenderer>,
    );
  }
});
