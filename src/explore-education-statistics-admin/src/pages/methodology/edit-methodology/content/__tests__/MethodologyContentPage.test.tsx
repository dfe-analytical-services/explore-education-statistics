import { screen, waitFor, within } from '@testing-library/react';
import _methodologyService from '@admin/services/methodologyService';
import _methodologyContentService from '@admin/services/methodologyContentService';
import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import { methodologyRoute } from '@admin/routes/routes';
import _permissionService from '@admin/services/permissionService';
import { generatePath, MemoryRouter } from 'react-router';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { Route } from 'react-router-dom';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import testMethodology, {
  testMethodologyContent,
} from '../../__tests__/__data__/test-data';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/methodologyContentService');
jest.mock('@admin/services/permissionService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const methodologyContentService = _methodologyContentService as jest.Mocked<
  typeof _methodologyContentService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('MethodologyContentPage', () => {
  beforeEach(async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    methodologyContentService.getMethodologyContent.mockResolvedValue(
      testMethodologyContent,
    );
    permissionService.canUpdateMethodology.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title-caption')).toHaveTextContent(
        'Edit methodology',
      );
    });

    userEvent.click(screen.getByRole('link', { name: 'Manage content' }));
  });

  test('Help and Support section renders', () => {
    expect(
      within(
        screen.getByRole('navigation', { name: 'Related information' }),
      ).getByRole('heading', {
        name: 'Help and support',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('mock team email 1')).toBeInTheDocument();
    expect(screen.getByText('mock team name 1')).toBeInTheDocument();
  });

  test('displays a link to the Contact Us section within the Related Information section', () => {
    expect(
      within(
        screen.getByRole('navigation', { name: 'Related information' }),
      ).getByRole('link', {
        name: 'Contact us',
      }),
    ).toBeInTheDocument();
  });

  test('navigates to the Contact Us section when the link is clicked', () => {
    expect(
      within(
        screen.getByRole('navigation', { name: 'Related information' }),
      ).getByRole('link', {
        name: 'Contact us',
      }),
    ).toHaveAttribute('href', '#contact-us');

    expect(
      screen
        .queryAllByRole('heading', { name: 'Contact us' })
        .find(e => e.id === 'contact-us'),
    ).not.toBeUndefined();
  });

  const renderPage = () => {
    const path = generatePath<MethodologyRouteParams>(
      methodologySummaryRoute.path,
      {
        methodologyId: 'm1',
      },
    );

    render(
      <MemoryRouter initialEntries={[path]}>
        <TestConfigContextProvider>
          <Route component={MethodologyPage} path={methodologyRoute.path} />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
