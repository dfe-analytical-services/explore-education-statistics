import {
  EducationInNumbersPageContextProvider,
  EducationInNumbersPageContextState,
} from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import _educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';
import EducationInNumbersSummaryEditPage from '@admin/pages/education-in-numbers/summary/EducationInNumbersSummaryEditPage';
import * as router from 'react-router';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';
import { educationInNumbersSummaryRoute } from '@admin/routes/educationInNumbersRoutes';
import { expectLocation } from '@admin/components/testing/TestLocationContext';

jest.mock('@admin/services/educationInNumbersService');

const educationInNumbersService = _educationInNumbersService as jest.Mocked<
  typeof _educationInNumbersService
>;

describe('EducationInNumbersSummaryEditPage', () => {
  const testPage: EinSummary = {
    id: 'page-1-id',
    title: 'Page 1 title',
    slug: 'page-1-slug',
    description: 'Page 1 description',
    version: 0,
  };

  test('renders form with initial values', () => {
    renderPage(testPage);

    expect(screen.getByLabelText('Title')).toHaveValue('Page 1 title');
    expect(screen.getByLabelText('Description')).toHaveValue(
      'Page 1 description',
    );
    expect(
      screen.getByRole('button', { name: 'Update page' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('submitting with updated values calls service and redirects', async () => {
    const updatedPage = { ...testPage, title: 'Updated title' };
    educationInNumbersService.updateEducationInNumbersPage.mockResolvedValue(
      updatedPage,
    );
    const onEducationInNumbersPageChange = jest.fn();
    const { user } = renderPage(testPage, onEducationInNumbersPageChange);

    await user.clear(screen.getByLabelText('Title'));
    await user.type(screen.getByLabelText('Title'), 'Updated title');

    await user.click(screen.getByRole('button', { name: 'Update page' }));

    await waitFor(() => {
      expect(
        educationInNumbersService.updateEducationInNumbersPage,
      ).toHaveBeenCalledWith('page-1-id', {
        title: 'Updated title',
        description: 'Page 1 description',
      });
      expect(onEducationInNumbersPageChange).toHaveBeenCalledWith(updatedPage);
    });

    await expectLocation('/education-in-numbers/page-1-id/summary');
  });

  test('clicking cancel calls useNavigate(-1)', async () => {
    const mockNavigate = jest.fn();

    jest.spyOn(router, 'useNavigate').mockImplementation(() => mockNavigate);

    const { user } = renderPage(testPage);

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(mockNavigate).toHaveBeenCalledWith(-1);
  });

  function renderPage(
    page: EinSummary,
    onEducationInNumbersPageChange: EducationInNumbersPageContextState['onEducationInNumbersPageChange'] = () => {},
  ) {
    return render(
      <TestRouterRenderer
        initialUrl="/"
        route="/"
        routes={[educationInNumbersSummaryRoute.fullPath]}
      >
        <EducationInNumbersPageContextProvider
          educationInNumbersPage={page}
          onEducationInNumbersPageChange={onEducationInNumbersPageChange}
        >
          <EducationInNumbersSummaryEditPage />
        </EducationInNumbersPageContextProvider>
      </TestRouterRenderer>,
    );
  }
});
