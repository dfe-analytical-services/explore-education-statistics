import ReleaseTableToolPage from '@admin/pages/release/datablocks/ReleaseTableToolPage';
import {
  ReleaseRouteParams,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import _releaseContentService from '@admin/services/releaseContentService';
import { testEditableRelease } from '@admin/pages/release/__data__/testEditableRelease';
import _tableBuilderService from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/releaseContentService');
jest.mock('@admin/services/permissionService');
jest.mock('@common/services/tableBuilderService');

const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ReleaseTableToolPage', () => {
  test('renders correctly on step 1', async () => {
    releaseContentService.getContent.mockResolvedValue({
      release: testEditableRelease,
      availableDataBlocks: [],
    });

    tableBuilderService.getReleaseSubjectsAndHighlights.mockResolvedValue({
      subjects: [
        {
          id: 'subject-1',
          name: 'Subject 1',
          content: '<p>Test content 1</p>',
          timePeriods: {
            from: '2018',
            to: '2020',
          },
          geographicLevels: ['National'],
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
        },
      ],
      highlights: [],
    });

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
