import ReleaseTableToolPage from '@admin/pages/release/datablocks/ReleaseTableToolPage';
import {
  ReleaseRouteParams,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import _publicationService, {
  Publication,
} from '@admin/services/publicationService';
import _tableBuilderService from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/permissionService');
jest.mock('@common/services/tableBuilderService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

const testPublication: Publication = {
  id: 'publication-1',
  title: 'Pupil absence',
  summary: 'Pupil absence summary',
  slug: 'pupil-absence',
  theme: { id: 'theme-1', title: 'Test theme' },
  topic: { id: 'topic-1', title: 'Test topic' },
};

describe('ReleaseTableToolPage', () => {
  test('renders correctly on step 1', async () => {
    publicationService.getPublication.mockResolvedValue(testPublication);

    tableBuilderService.listReleaseFeaturedTables.mockResolvedValue([]);
    tableBuilderService.listReleaseSubjects.mockResolvedValue([
      {
        id: 'subject-1',
        name: 'Subject 1',
        content: '<p>Test content 1</p>',
        timePeriods: {
          from: '2018',
          to: '2020',
        },
        geographicLevels: ['National'],
        file: {
          id: 'file-1',
          name: 'Subject 1',
          fileName: 'file-1.csv',
          extension: 'csv',
          size: '10 Mb',
          type: 'Data',
        },
      },
      {
        id: 'subject-2',
        name: 'Subject 2',
        content: '<p>Test content 2</p>',
        timePeriods: {
          from: '2015/16',
          to: '2018/19',
        },
        geographicLevels: ['National'],
        file: {
          id: 'file-2',
          name: 'Subject 2',
          fileName: 'file-2.csv',
          extension: 'csv',
          size: '20 Mb',
          type: 'Data',
        },
      },
    ]);

    renderPage();

    await waitFor(() => {
      const stepHeadings = screen.queryAllByRole('heading', { name: /Step/ });

      expect(stepHeadings).toHaveLength(1);
      expect(stepHeadings[0]).toHaveTextContent(
        'Step 1 (current) Choose a subject',
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
