import ReleaseTableToolPage from '@admin/pages/release/datablocks/ReleaseTableToolPage';
import {
  ReleaseRouteParams,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import _tableBuilderService from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ReleaseTableToolPage', () => {
  test('renders correctly on step 1', async () => {
    tableBuilderService.getRelease.mockResolvedValue({
      id: 'release-1',
      subjects: [
        {
          id: 'subject-1',
          label: 'Subject 1',
        },
        {
          id: 'subject-2',
          label: 'Subject 2',
        },
      ],
      highlights: [],
    });

    renderPage();

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(1);
      expect(stepHeadings[0]).toHaveTextContent(
        'Step 1 (current): Choose a subject',
      );
    });

    const step = within(screen.getByTestId('wizardStep-1'));

    expect(step.getAllByLabelText(/Subject/)).toHaveLength(2);

    expect(step.getByLabelText('Subject 1')).toBeInTheDocument();
    expect(step.getByLabelText('Subject 2')).toBeInTheDocument();
  });

  const renderPage = () => {
    return render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseRouteParams>(releaseTableToolRoute.path, {
            publicationId: 'publication-1',
            releaseId: 'release-1',
          }),
        ]}
      >
        <Route
          component={ReleaseTableToolPage}
          path={releaseTableToolRoute.path}
        />
      </MemoryRouter>,
    );
  };
});
