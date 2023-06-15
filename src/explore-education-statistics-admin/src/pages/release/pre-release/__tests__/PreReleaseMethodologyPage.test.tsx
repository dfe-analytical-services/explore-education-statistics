import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { MethodologyContextProvider } from '@admin/pages/methodology/contexts/MethodologyContext';
import PreReleaseMethodologyPage from '@admin/pages/release/pre-release/PreReleaseMethodologyPage';
import {
  preReleaseMethodologyRoute,
  PreReleaseMethodologyRouteParams,
} from '@admin/routes/preReleaseRoutes';
import _methodologyContentService, {
  MethodologyContent,
} from '@admin/services/methodologyContentService';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route } from 'react-router';
import { generatePath } from 'react-router-dom';
import React from 'react';

jest.mock('@admin/services/methodologyContentService');
const methodologyContentService = _methodologyContentService as jest.Mocked<
  typeof _methodologyContentService
>;

describe('PreReleaseMethodologyPage', () => {
  const testMethodologyVersion: MethodologyVersion = {
    id: 'methodology-1',
    amendment: false,
    methodologyId: 'methodology-id-1',
    owningPublication: {
      id: 'publication-1',
      title: 'Owning publication title',
    },
    slug: 'pupil-absence-in-schools-in-england',
    status: 'Approved',
    title: 'Pupil absence statistics: methodology',
  };

  const testMethodology: MethodologyContent = {
    id: 'methodology-1',
    title: 'Pupil absence statistics: methodology',
    published: '2021-02-16T15:32:01',
    status: 'Approved',
    slug: 'pupil-absence-in-schools-in-england',
    content: [
      {
        id: 'content-1',
        order: 0,
        heading: 'Section 1',
        caption: 'Section 1 caption',
        content: [
          {
            body: '<p>section 1 content</p>',
            id: 'section-1-content',
            order: 0,
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
      {
        id: 'content-2',
        order: 1,
        heading: 'Section 2',
        caption: 'Section 2 caption',
        content: [
          {
            body: '<p>section 2 content</p>',
            id: 'section-2-content',
            order: 0,
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
    ],
    annexes: [
      {
        id: 'annex-1',
        order: 0,
        heading: 'Annex 1',
        caption: 'Annex 1 caption',
        content: [
          {
            body: '<p>annex 1 content</p>',
            id: 'annex-1-content',
            order: 0,
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
      {
        id: 'annex-2',
        order: 1,
        heading: 'Annex 2',
        caption: 'Annex 2 caption',
        content: [
          {
            body: '<p>annex 2 content</p>',
            id: 'annex-2-content',
            order: 0,
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
      {
        id: 'annex-3',
        order: 2,
        heading: 'Annex 3',
        caption: 'Annex 3 caption',
        content: [
          {
            body: '<p>annex 3 content</p>',
            id: 'annex-3-content',
            order: 0,
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
    ],
    notes: [
      {
        id: 'note-1',
        displayDate: new Date('2021-09-15T00:00:00'),
        content: 'Latest note',
      },
      {
        id: 'note-2',
        displayDate: new Date('2021-04-19T00:00:00'),
        content: 'Other note',
      },
      {
        id: 'note-3',
        displayDate: new Date('2021-03-01T00:00:00'),
        content: 'Earliest note',
      },
    ],
  };

  test('renders the basic info', async () => {
    methodologyContentService.getMethodologyContent.mockResolvedValue(
      testMethodology,
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Pupil absence statistics: methodology'));
    });

    expect(
      screen.getByRole('heading', {
        name: 'Pupil absence statistics: methodology',
      }),
    );

    expect(screen.getByTestId('Publish date-value')).toHaveTextContent(
      '16 February 2021',
    );

    expect(screen.getByLabelText('Search in this methodology page.'));

    expect(screen.getByRole('button', { name: 'Print this page' }));
  });

  test('renders the content', async () => {
    methodologyContentService.getMethodologyContent.mockResolvedValue(
      testMethodology,
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Pupil absence statistics: methodology'));
    });

    const contentAccordion = screen.getAllByTestId('accordion')[0];
    const contentAccordionSections =
      within(contentAccordion).getAllByTestId('accordionSection');

    expect(contentAccordionSections).toHaveLength(2);

    expect(
      within(contentAccordionSections[0]).getByRole('button', {
        name: 'Section 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(contentAccordionSections[0]).getByText('Section 1 caption'),
    ).toBeInTheDocument();
    expect(
      within(contentAccordionSections[0]).getByText('section 1 content'),
    ).toBeInTheDocument();

    expect(
      within(contentAccordionSections[1]).getByRole('button', {
        name: 'Section 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(contentAccordionSections[1]).getByText('Section 2 caption'),
    ).toBeInTheDocument();
    expect(
      within(contentAccordionSections[1]).getByText('section 2 content'),
    ).toBeInTheDocument();
  });

  test('renders the annexes', async () => {
    methodologyContentService.getMethodologyContent.mockResolvedValue(
      testMethodology,
    );
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Pupil absence statistics: methodology'));
    });

    const annexAccordion = screen.getAllByTestId('accordion')[1];
    const annexAccordionSections =
      within(annexAccordion).getAllByTestId('accordionSection');

    expect(
      screen.getByRole('heading', { name: 'Annexes' }),
    ).toBeInTheDocument();

    expect(annexAccordionSections).toHaveLength(3);

    expect(
      within(annexAccordionSections[0]).getByRole('button', {
        name: 'Annex 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(annexAccordionSections[0]).getByText('Annex 1 caption'),
    ).toBeInTheDocument();
    expect(
      within(annexAccordionSections[0]).getByText('annex 1 content'),
    ).toBeInTheDocument();

    expect(
      within(annexAccordionSections[1]).getByRole('button', {
        name: 'Annex 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(annexAccordionSections[1]).getByText('Annex 2 caption'),
    ).toBeInTheDocument();
    expect(
      within(annexAccordionSections[1]).getByText('annex 2 content'),
    ).toBeInTheDocument();

    expect(
      within(annexAccordionSections[2]).getByRole('button', {
        name: 'Annex 3',
      }),
    ).toBeInTheDocument();
    expect(
      within(annexAccordionSections[2]).getByText('Annex 3 caption'),
    ).toBeInTheDocument();
    expect(
      within(annexAccordionSections[2]).getByText('annex 3 content'),
    ).toBeInTheDocument();
  });

  describe('update notes', () => {
    test(`renders 'TBA' for 'Last updated' if there are no notes`, async () => {
      methodologyContentService.getMethodologyContent.mockResolvedValue({
        ...testMethodology,
        notes: [],
      });
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Pupil absence statistics: methodology'));
      });

      expect(screen.getByTestId('Last updated-value')).toHaveTextContent('TBA');
    });

    test(`renders 'Last updated' with the date of the most recent note`, async () => {
      methodologyContentService.getMethodologyContent.mockResolvedValue(
        testMethodology,
      );
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Pupil absence statistics: methodology'));
      });

      expect(screen.getByTestId('Last updated-value')).toHaveTextContent(
        '15 September 2021',
      );
    });

    test(`renders the list of all notes`, async () => {
      methodologyContentService.getMethodologyContent.mockResolvedValue(
        testMethodology,
      );
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Pupil absence statistics: methodology'));
      });

      userEvent.click(
        screen.getByRole('button', { name: 'See all notes (3)' }),
      );

      const notes = within(screen.getByTestId('notes')).getAllByRole(
        'listitem',
      );

      expect(notes).toHaveLength(3);

      expect(within(notes[0]).getByText('15 September 2021'));
      expect(within(notes[0]).getByText('Latest note'));

      expect(within(notes[1]).getByText('19 April 2021'));
      expect(within(notes[1]).getByText('Other note'));

      expect(within(notes[2]).getByText('1 March 2021'));
      expect(within(notes[2]).getByText('Earliest note'));
    });
  });

  const renderPage = (
    initialEntries: string[] = [
      generatePath<PreReleaseMethodologyRouteParams>(
        preReleaseMethodologyRoute.path,
        {
          publicationId: 'publication-1',
          releaseId: 'release-1',
          methodologyId: 'methodology-1',
        },
      ),
    ],
  ) => {
    return render(
      <MemoryRouter initialEntries={initialEntries}>
        <TestConfigContextProvider>
          <MethodologyContextProvider methodology={testMethodologyVersion}>
            <Route
              component={PreReleaseMethodologyPage}
              path={preReleaseMethodologyRoute.path}
            />
          </MethodologyContextProvider>
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
