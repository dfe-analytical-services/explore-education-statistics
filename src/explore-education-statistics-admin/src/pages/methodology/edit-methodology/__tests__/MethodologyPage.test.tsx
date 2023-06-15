import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import { methodologyRoute } from '@admin/routes/routes';
import _methodologyService, {
  MethodologyVersion,
} from '@admin/services/methodologyService';
import _methodologyContentService, {
  MethodologyContent,
} from '@admin/services/methodologyContentService';
import _permissionService from '@admin/services/permissionService';
import { generatePath, MemoryRouter } from 'react-router';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Route } from 'react-router-dom';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import React from 'react';

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

describe('MethodologyPage', () => {
  const testMethodology: MethodologyVersion = {
    id: 'm1',
    amendment: false,
    methodologyId: 'm-1',
    title: 'Test methodology',
    slug: 'test-methodology',
    owningPublication: {
      id: 'p1',
      title: 'Publication title',
    },
    status: 'Draft',
  };
  const testMethodologyAmendment = {
    ...testMethodology,
    amendment: true,
  };

  const testMethodologyContent: MethodologyContent = {
    id: 'mc-1',
    title: 'The content',
    slug: 'content-1',
    status: 'Draft',
    content: [],
    annexes: [],
    notes: [],
  };

  test('renders the page with the summary tab', async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title-caption')).toHaveTextContent(
        'Edit methodology',
      );
      expect(
        screen.getByRole('heading', { name: 'Test methodology' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('navigation', { name: 'Methodology' }),
      ).toBeInTheDocument();

      const primaryNav = screen.getByRole('navigation', {
        name: 'Methodology',
      });
      const navLinks = within(primaryNav).getAllByRole('link');

      expect(navLinks).toHaveLength(3);
      expect(navLinks[0]).toHaveTextContent('Summary');
      expect(navLinks[0].getAttribute('aria-current')).toBe('page');
      expect(navLinks[1]).not.toHaveAttribute('aria-current');
      expect(navLinks[2]).toHaveTextContent('Sign off');
      expect(navLinks[2]).not.toHaveAttribute('aria-current');

      expect(screen.getByText('Methodology summary')).toBeInTheDocument();
    });
  });

  test('showing the status tag', async () => {
    methodologyService.getMethodology.mockResolvedValue(testMethodology);
    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title-caption')).toHaveTextContent(
        'Edit methodology',
      );
      expect(screen.getByTestId('page-title-caption')).not.toHaveTextContent(
        'Amend methodology',
      );
      expect(screen.getByText('Draft')).toHaveClass('govuk-tag');
    });
  });

  test('showing the amendment page caption and tag', async () => {
    methodologyService.getMethodology.mockResolvedValue(
      testMethodologyAmendment,
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title-caption')).toHaveTextContent(
        'Amend methodology',
      );
      expect(screen.getByTestId('page-title-caption')).not.toHaveTextContent(
        'Edit methodology',
      );
      expect(screen.getByText('Draft')).toHaveClass('govuk-tag');

      expect(screen.getByText('Amendment')).toHaveClass('govuk-tag');
    });
  });

  test('using the tab navigation', async () => {
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

    const primaryNav = screen.getByRole('navigation', {
      name: 'Methodology',
    });
    const navLinks = within(primaryNav).getAllByRole('link');

    userEvent.click(navLinks[1]);

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Content' }),
      ).toBeInTheDocument();
      expect(navLinks[1].getAttribute('aria-current')).toBe('page');
      expect(navLinks[0]).not.toHaveAttribute('aria-current');
      expect(navLinks[2]).not.toHaveAttribute('aria-current');
    });

    userEvent.click(navLinks[2]);

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Sign off' }),
      ).toBeInTheDocument();
      expect(navLinks[2].getAttribute('aria-current')).toBe('page');
      expect(navLinks[0]).not.toHaveAttribute('aria-current');
      expect(navLinks[1]).not.toHaveAttribute('aria-current');
    });
  });

  function renderPage() {
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
  }
});
