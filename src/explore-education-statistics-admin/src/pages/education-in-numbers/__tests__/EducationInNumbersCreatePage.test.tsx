import _educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import EducationInNumbersCreatePage from '@admin/pages/education-in-numbers/EducationInNumbersCreatePage';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';
import { expectLocation } from '@admin/components/testing/TestLocationContext';
import { educationInNumbersListRoute } from '@admin/routes/routes';
import { educationInNumbersSummaryRoute } from '@admin/routes/educationInNumbersRoutes';

jest.mock('@admin/services/educationInNumbersService');

const educationInNumbersService = _educationInNumbersService as jest.Mocked<
  typeof _educationInNumbersService
>;

describe('EducationInNumbersCreatePage', () => {
  const newPage: EinSummary = {
    id: 'new-page-id',
    title: 'New page title',
    slug: 'new-page-title',
    description: 'New page description',
    version: 0,
  };

  test('renders form correctly', () => {
    renderPage();

    expect(
      screen.getByRole('heading', {
        name: 'Create a new Education in Numbers page',
      }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Title')).toBeInTheDocument();
    expect(screen.getByLabelText('Description')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Create page' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('creates page successfully and redirects', async () => {
    educationInNumbersService.createEducationInNumbersPage.mockResolvedValue(
      newPage,
    );
    const { user } = renderPage();

    await user.type(screen.getByLabelText('Title'), 'New page title');
    await user.type(
      screen.getByLabelText('Description'),
      'New page description',
    );

    await user.click(screen.getByRole('button', { name: 'Create page' }));

    await waitFor(() => {
      expect(
        educationInNumbersService.createEducationInNumbersPage,
      ).toHaveBeenCalledWith({
        title: 'New page title',
        description: 'New page description',
      });
    });

    await expectLocation('/education-in-numbers/new-page-id/summary');
  });

  test('clicking cancel navigates back to the list page', async () => {
    const { user } = renderPage();

    await user.click(screen.getByRole('link', { name: 'Cancel' }));

    await expectLocation('/education-in-numbers');
  });

  test('shows validation error if no title is provided', async () => {
    const { user } = renderPage();

    await user.click(screen.getByRole('button', { name: 'Create page' }));

    const error = await screen.findByTestId(
      'educationInNumbersSummaryForm-title-error',
    );
    expect(error).toHaveTextContent('Enter a title');
  });

  test('shows validation error if no description is provided', async () => {
    const { user } = renderPage();

    await user.type(screen.getByLabelText('Title'), 'A title');
    await user.click(screen.getByRole('button', { name: 'Create page' }));

    const error = await screen.findByTestId(
      'educationInNumbersSummaryForm-description-error',
    );
    expect(error).toHaveTextContent('Enter a description');
  });

  function renderPage() {
    return render(
      <TestRouterRenderer
        initialUrl={educationInNumbersListRoute.fullPath}
        route={educationInNumbersListRoute.fullPath}
        routes={[educationInNumbersSummaryRoute.fullPath]}
      >
        <EducationInNumbersCreatePage />
      </TestRouterRenderer>,
    );
  }
});
