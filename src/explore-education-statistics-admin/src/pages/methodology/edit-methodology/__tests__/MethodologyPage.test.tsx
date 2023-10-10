import MethodologyPage from '@admin/pages/methodology/edit-methodology/MethodologyPage';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import { methodologyRoute } from '@admin/routes/routes';
import _methodologyService from '@admin/services/methodologyService';
import _methodologyContentService from '@admin/services/methodologyContentService';
import _permissionService from '@admin/services/permissionService';
import { generatePath, MemoryRouter } from 'react-router';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { Route } from 'react-router-dom';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import render from '@common-test/render';
import testMethodology, {
  testMethodologyAmendment,
  testMethodologyContent,
} from './__data__/mock-data';

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
  describe('Non-amendment', () => {
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
    });

    it('Renders the page with the summary tab', async () => {
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

    it('Shows the status tag', async () => {
      expect(screen.getByTestId('page-title-caption')).not.toHaveTextContent(
        'Amend methodology',
      );
      expect(screen.getByText('Draft')).toHaveClass('govuk-tag');
    });

    test('using the tab navigation', async () => {
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

    it('Renders the Help and Support section', async () => {
      methodologyService.getMethodology.mockResolvedValue(
        testMethodologyAmendment,
      );

      expect(
        within(selectRelatedInformationSection()).getByRole('heading', {
          name: 'Help and support',
        }),
      ).toBeVisible();
    });

    it('Displays a link to the contact us section within the Related Information section', () => {
      expect(
        within(selectRelatedInformationSection()).getByRole('link', {
          name: 'Contact us',
        }),
      ).toBeVisible();
    });

    it('Navigates to the Contact Us section when the link is clicked', () => {
      expect(
        within(selectRelatedInformationSection()).getByRole('link', {
          name: 'Contact us',
        }),
      ).toHaveAttribute('href', '#contact-us');

      expect(
        screen
          .queryAllByRole('heading', { name: 'Contact us' })
          .find(e => e.id === 'contact-us'),
      ).not.toBeUndefined();
    });
  });

  describe('Amendment', () => {
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

  const selectRelatedInformationSection = (): HTMLElement => {
    const relatedInformation = screen.getByRole('heading', {
      name: 'Related information',
    }).parentElement;

    if (!relatedInformation) {
      throw new Error(
        'Failing test early - the "Related information" section could not be found.',
      );
    }

    return relatedInformation;
  };
});
