import { render, screen, waitFor } from '@testing-library/react';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { generatePath, MemoryRouter } from 'react-router';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import { MethodologyContextProvider } from '@admin/pages/methodology/contexts/MethodologyContext';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import { Route } from 'react-router-dom';
import React from 'react';

const testMethodology: MethodologyVersion = {
  id: 'm1',
  amendment: false,
  methodologyId: 'm-1',
  title: 'Test methodology',
  slug: 'test-methodology',
  status: 'Draft',
  otherPublications: [
    {
      id: 'op1',
      title: 'Other publication title 1',
    },
    {
      id: 'op2',
      title: 'Other publication title 2',
    },
  ],
  owningPublication: {
    id: 'p1',
    title: 'Publication title',
  },
  published: '',
};

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
      <MemoryRouter
        initialEntries={[
          generatePath<MethodologyRouteParams>(methodologySummaryRoute.path, {
            methodologyId: testMethodology.id,
          }),
        ]}
      >
        <MethodologyContextProvider methodology={testMethodology}>
          <Route
            path={methodologySummaryRoute.path}
            component={MethodologySummaryPage}
          />
        </MethodologyContextProvider>
      </MemoryRouter>,
    );
  }
});
