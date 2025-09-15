import React from 'react';
import { screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route } from 'react-router-dom';
import { generatePath } from 'react-router';
import EducationInNumbersContentPage from '@admin/pages/education-in-numbers/content/EducationInNumbersContentPage';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';
import _einContentService, {
  EinContent,
} from '@admin/services/educationInNumbersContentService';
import _einService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import render from '@common-test/render';
import { educationInNumbersRoute } from '@admin/routes/routes';
import userEvent from '@testing-library/user-event';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';

jest.mock('@admin/services/educationInNumbersService');
const einService = _einService as jest.Mocked<typeof _einService>;

jest.mock('@admin/services/educationInNumbersContentService');
const einContentService = _einContentService as jest.Mocked<
  typeof _einContentService
>;

describe('EducationInNumbersContentPage', () => {
  const testPageVersion: EinSummary = {
    id: 'test-page-id',
    title: 'Test Page Title',
    slug: 'test-page-slug',
    description: 'Test page description',
    version: 0,
  };

  const testPageContent: EinContent = {
    id: 'test-page-id',
    title: 'Test Page Title',
    slug: 'test-page-slug',
    content: [
      {
        id: 'section-1',
        order: 0,
        heading: 'Section 1',
        content: [
          {
            id: 'block-1',
            type: 'HtmlBlock',
            order: 0,
            body: '<p>Section 1 content</p>',
          },
        ],
      },
      {
        id: 'section-2',
        order: 1,
        heading: 'Section 2',
        content: [
          {
            id: 'block-2',
            type: 'HtmlBlock',
            order: 1,
            body: '<p>Section 2 content</p>',
          },
        ],
      },
    ],
  };

  test('renders the page title and content', async () => {
    einService.getEducationInNumbersPage.mockResolvedValueOnce(testPageVersion);
    einContentService.getEducationInNumbersPageContent.mockResolvedValueOnce(
      testPageContent,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title')).toHaveTextContent(
        'Test Page Title',
      );
      expect(
        screen.getByTestId('education-in-numbers-content'),
      ).toBeInTheDocument();
    });
  });

  test('is in edit mode and shows the mode toggle when the page is not published', async () => {
    einService.getEducationInNumbersPage.mockResolvedValueOnce({
      ...testPageVersion,
      published: undefined,
    });
    einContentService.getEducationInNumbersPageContent.mockResolvedValueOnce(
      testPageContent,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByLabelText('Edit content')).toBeInTheDocument();
      expect(screen.getByLabelText('Preview content')).toBeInTheDocument();
    });
  });

  test('is in preview mode and hides the mode toggle when the page is published', async () => {
    einService.getEducationInNumbersPage.mockResolvedValueOnce({
      ...testPageVersion,
      published: '2023-08-28T12:00:00Z',
    });
    einContentService.getEducationInNumbersPageContent.mockResolvedValue(
      testPageContent,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title')).toHaveTextContent(
        'Test Page Title',
      );
    });

    expect(screen.queryByLabelText('Edit content')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Preview content')).not.toBeInTheDocument();
  });

  test('in edit mode, renders content with edit controls', async () => {
    einService.getEducationInNumbersPage.mockResolvedValueOnce({
      ...testPageVersion,
      published: undefined,
    });
    einContentService.getEducationInNumbersPageContent.mockResolvedValue(
      testPageContent,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title')).toHaveTextContent(
        'Test Page Title',
      );
    });

    expect(screen.getByLabelText('Edit content')).toBeChecked();

    const accordion = screen.getByTestId('accordion');
    const sections = within(accordion).getAllByTestId('accordionSection');

    expect(sections).toHaveLength(2);

    const section1 = sections[0];
    await waitFor(() => {
      expect(
        within(section1).getByRole('button', { name: /Section 1/ }),
      ).toBeInTheDocument();
    });
    expect(within(section1).getByText('Section 1 content')).toBeInTheDocument();
    expect(
      within(section1).getByRole('button', { name: 'Edit section title' }),
    ).toBeInTheDocument();
    expect(
      within(section1).getByRole('button', { name: 'Reorder this section' }),
    ).toBeInTheDocument();
    expect(
      within(section1).getByRole('button', { name: 'Remove this section' }),
    ).toBeInTheDocument();
    expect(
      within(section1).getByRole('button', { name: 'Edit block' }),
    ).toBeInTheDocument();
    expect(
      within(section1).getByRole('button', { name: 'Remove block' }),
    ).toBeInTheDocument();

    const section2 = sections[1];
    await waitFor(() => {
      expect(
        within(section2).getByRole('button', { name: /Section 2/ }),
      ).toBeInTheDocument();
    });
    expect(within(section2).getByText('Section 2 content')).toBeInTheDocument();
    expect(
      within(section2).getByRole('button', { name: 'Edit section title' }),
    ).toBeInTheDocument();
    expect(
      within(section2).getByRole('button', { name: 'Reorder this section' }),
    ).toBeInTheDocument();
    expect(
      within(section2).getByRole('button', { name: 'Remove this section' }),
    ).toBeInTheDocument();
    expect(
      within(section2).getByRole('button', { name: 'Edit block' }),
    ).toBeInTheDocument();
    expect(
      within(section2).getByRole('button', { name: 'Remove block' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Add new section' }),
    ).toBeInTheDocument();
  });

  test('in preview mode, renders content without edit controls', async () => {
    einService.getEducationInNumbersPage.mockResolvedValueOnce({
      ...testPageVersion,
      published: undefined,
    });
    einContentService.getEducationInNumbersPageContent.mockResolvedValue(
      testPageContent,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('page-title')).toHaveTextContent(
        'Test Page Title',
      );
    });

    await userEvent.click(screen.getByLabelText('Preview content'));

    expect(screen.getByLabelText('Preview content')).toBeChecked();

    const accordion = screen.getByTestId('accordion');
    const sections = within(accordion).getAllByTestId('accordionSection');

    expect(sections).toHaveLength(2);
    const section1 = sections[0];

    await waitFor(() => {
      expect(
        within(section1).getByRole('button', { name: /Section 1/ }),
      ).toBeInTheDocument();
    });
    expect(within(section1).getByText('Section 1 content')).toBeInTheDocument();
    expect(
      within(section1).queryByRole('button', { name: 'Edit section title' }),
    ).not.toBeInTheDocument();
    expect(
      within(section1).queryByRole('button', {
        name: 'Reorder this section',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(section1).queryByRole('button', {
        name: 'Remove this section',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(section1).queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      within(section1).queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();

    const section2 = sections[1];
    await waitFor(() => {
      expect(
        within(section2).getByRole('button', { name: /Section 2/ }),
      ).toBeInTheDocument();
    });
    expect(within(section2).getByText('Section 2 content')).toBeInTheDocument();
    expect(
      within(section2).queryByRole('button', { name: 'Edit section title' }),
    ).not.toBeInTheDocument();
    expect(
      within(section2).queryByRole('button', {
        name: 'Reorder this section',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(section2).queryByRole('button', {
        name: 'Remove this section',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(section2).queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      within(section2).queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Add new section' }),
    ).not.toBeInTheDocument();
  });

  const renderPage = () => {
    const path = generatePath<EducationInNumbersRouteParams>(
      educationInNumbersRoute.path,
      {
        educationInNumbersPageId: 'test-page-id',
      },
    );

    render(
      <MemoryRouter initialEntries={[path]}>
        <TestConfigContextProvider>
          <Route
            component={EducationInNumbersContentPage}
            path={educationInNumbersRoute.path}
          />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
