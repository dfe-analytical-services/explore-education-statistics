import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import _methodologyService, {
  BasicMethodology,
} from '@admin/services/methodologyService';
import { generatePath, MemoryRouter } from 'react-router';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import { Route } from 'react-router-dom';

jest.mock('@admin/services/methodologyService');
const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;

const testMethodology: BasicMethodology = {
  id: 'm1',
  amendment: false,
  title: 'Test methodology',
  slug: 'test-methodology',
  status: 'Draft',
  owningPublication: {
    id: 'p1',
    title: 'Publication title',
  },
  published: '',
};

describe('MethodologySummaryPage', () => {
  test('renders methodology summary page correctly', async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology summary')).toBeInTheDocument();

      expect(screen.getByTestId('Title-key')).toHaveTextContent('Title');
      expect(screen.getByTestId('Title-value')).toHaveTextContent(
        'Test methodology',
      );

      expect(screen.getByTestId('Status-key')).toHaveTextContent('Status');
      expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');

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
    methodologyService.getMethodology.mockResolvedValue({
      ...testMethodology,
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
    });

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
        <Route
          path={methodologySummaryRoute.path}
          component={MethodologySummaryPage}
        />
      </MemoryRouter>,
    );
  }
});
