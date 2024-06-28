import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ContentHtml from '@common/components/ContentHtml';
import { GlossaryEntry } from '@common/services/types/glossary';
import _redirectService from '@common/services/redirectService';
import { within } from '@testing-library/dom';

jest.mock('@common/services/redirectService');
const redirectService = _redirectService as jest.Mocked<
  typeof _redirectService
>;

describe('ContentHtml', () => {
  beforeEach(() => {
    redirectService.list.mockImplementation(async () => {
      return Promise.resolve({
        methodologies: [],
        publications: [],
      });
    });
  });

  test('renders correctly with required props', () => {
    render(<ContentHtml html="<p>Test text</p>" />);

    expect(screen.getByText('Test text')).toBeInTheDocument();
  });

  test('renders glossary info modal when clicking glossary entry', async () => {
    const getEntry = jest.fn().mockResolvedValue({
      title: 'Absence heading',
      slug: 'absence-slug',
      body: '<p>Absence body</p>',
    } as GlossaryEntry);

    render(
      <ContentHtml
        html="<a href='/glossary#absence' data-glossary>Absence</a>"
        getGlossaryEntry={getEntry}
      />,
    );

    const glossaryButton = screen.getByRole('button', {
      name: 'Absence (show glossary term definition)',
    });

    await userEvent.click(glossaryButton);

    await waitFor(() => {
      expect(screen.getByText('Absence body')).toBeInTheDocument();
    });

    expect(getEntry).toHaveBeenCalled();

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Absence heading' }),
    ).toBeInTheDocument();
    expect(modal.getByText('Absence body')).toBeInTheDocument();
    const closeButton = modal.getByRole('button', { name: 'Close modal' });
    expect(closeButton).toBeInTheDocument();
  });

  test('can close glossary info modal', async () => {
    const getEntry = jest.fn().mockResolvedValue({
      title: 'Absence heading',
      slug: 'absence-slug',
      body: '<p>Absence body</p>',
    } as GlossaryEntry);

    render(
      <ContentHtml
        html="<a href='/glossary#absence' data-glossary>Absence</a>"
        getGlossaryEntry={getEntry}
      />,
    );

    const glossaryButton = screen.getByRole('button', {
      name: 'Absence (show glossary term definition)',
    });
    await userEvent.click(glossaryButton);

    await waitFor(() => {
      expect(screen.getByText('Absence body')).toBeInTheDocument();
    });

    expect(getEntry).toHaveBeenCalled();

    const modal = within(screen.getByRole('dialog'));
    const closeButton = modal.getByRole('button', { name: 'Close modal' });
    await userEvent.click(closeButton);

    await waitFor(() => {
      expect(closeButton).not.toBeInTheDocument();
      expect(glossaryButton).toBeInTheDocument();
    });
  });

  test('transforms featured table links when `transformFeaturedTableLinks` is provided', () => {
    render(
      <ContentHtml
        html="<a href='/data-table/fast-track/124356575?featuredTable=true' data-featured-table>Featured table</a>"
        transformFeaturedTableLinks={() => {
          return <span>I am transformed</span>;
        }}
      />,
    );

    expect(screen.getByText('I am transformed')).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'Featured table' }),
    ).not.toBeInTheDocument();
  });

  test('does not transform featured table links when `transformFeaturedTableLinks` is not provided', async () => {
    render(
      <ContentHtml html="<a href='/data-table/fast-track/124356575?featuredTable=true' data-featured-table>Featured table</a>" />,
    );

    expect(screen.queryByText('I am transformed')).not.toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Featured table' }),
    ).toBeInTheDocument();
  });

  describe('formatting tables', () => {
    test('removes figure and adds tabindex to table markup from CKEditor', () => {
      const html = `
        <figure class="table">
          <table>
            <tbody>
            <tr>
              <td>Test 1</td>
              <td>Test 2</td>
            </tr>
            </tbody>
          </table>
        </figure>
      `;

      const { container } = render(<ContentHtml html={html} />);

      expect(container.innerHTML).toMatchInlineSnapshot(`
      <div class="dfe-content">
        <div class="tableContainer"
             tabindex="0"
        >
          <table>
            <tbody>
              <tr>
                <td>
                  Test 1
                </td>
                <td>
                  Test 2
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      `);
    });

    test('fixes inaccessible table caption markup from CKEditor', () => {
      const html = `
        <figure class="table">
          <table>
            <tbody>
            <tr>
              <td>Test 1</td>
              <td>Test 2</td>
            </tr>
            </tbody>
          </table>
          <figcaption>
            Test <strong>bold</strong> caption
          </figcaption>
        </figure>
      `;

      const { container } = render(<ContentHtml html={html} />);

      expect(container.innerHTML).toMatchInlineSnapshot(`
        <div class="dfe-content">
          <div class="tableContainer"
               tabindex="0"
          >
            <table>
              <caption>
                Test
                <strong>
                  bold
                </strong>
                caption
              </caption>
              <tbody>
                <tr>
                  <td>
                    Test 1
                  </td>
                  <td>
                    Test 2
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      `);
    });
  });

  describe('formatting links', () => {
    test('encodes special characters in urls', async () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/TEST something">External link</a>' />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('External link (opens in a new tab)'),
        ).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', {
          name: 'External link (opens in a new tab)',
        }),
      ).toHaveAttribute('href', 'https://gov.uk/TEST%20something');
    });

    test('trims whitespace in urls', async () => {
      render(
        <ContentHtml html='<a href="   https://gov.uk/TEST  ">External link</a>">External link</a>' />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('External link (opens in a new tab)'),
        ).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', {
          name: 'External link (opens in a new tab)',
        }),
      ).toHaveAttribute('href', 'https://gov.uk/TEST');
    });

    test('lower cases internal urls, excluding query params', async () => {
      render(
        <ContentHtml html='<a href="https://explore-education-statistics.service.gov.uk/Find-Statistics?testParam=Something">Internal link</a>' />,
      );

      await waitFor(() => {
        expect(screen.getByText('Internal link')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', {
          name: 'Internal link',
        }),
      ).toHaveAttribute(
        'href',
        'https://explore-education-statistics.service.gov.uk/find-statistics?testParam=Something',
      );
    });

    test('does not lower case external urls', async () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/TEST">External link</a>' />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('External link (opens in a new tab)'),
        ).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', {
          name: 'External link (opens in a new tab)',
        }),
      ).toHaveAttribute('href', 'https://gov.uk/TEST');
    });

    test('opens external links in a new tab', async () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/">External link</a>' />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('External link (opens in a new tab)'),
        ).toBeInTheDocument();
      });

      const link = screen.getByRole('link', {
        name: 'External link (opens in a new tab)',
      });
      expect(link).toHaveAttribute('href', 'https://gov.uk/');
      expect(link).toHaveAttribute('target', '_blank');
      expect(link).toHaveAttribute('rel', 'noopener noreferrer');
    });

    test('opens internal links in the same tab', async () => {
      render(
        <ContentHtml html='<a href="https://explore-education-statistics.service.gov.uk/">Internal link</a>' />,
      );

      await waitFor(() => {
        expect(screen.getByText('Internal link')).toBeInTheDocument();
      });

      const link = screen.getByRole('link', {
        name: 'Internal link',
      });
      expect(link).toHaveAttribute(
        'href',
        'https://explore-education-statistics.service.gov.uk/',
      );
      expect(link).not.toHaveAttribute('target', '_blank');
      expect(link).not.toHaveAttribute('rel', 'noopener noreferrer');
    });

    test('does not format links when `formatLinks` is false', async () => {
      render(
        <ContentHtml
          formatLinks={false}
          html={` 
          <a href="https://explore-education-statistics.service.gov.uk/Find-Statistics?testParam=Something">
          Internal link</a> 
          <a href="  https://gov.uk/TEST something  ">External link</a>`}
        />,
      );

      await waitFor(() => {
        expect(screen.getByText('External link')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', {
          name: 'Internal link',
        }),
      ).toHaveAttribute(
        'href',
        'https://explore-education-statistics.service.gov.uk/Find-Statistics?testParam=Something',
      );

      const externalLink = screen.getByRole('link', {
        name: 'External link',
      });
      expect(externalLink).toHaveAttribute(
        'href',
        '  https://gov.uk/TEST something  ',
      );
      expect(externalLink).not.toHaveAttribute('target', '_blank');
      expect(externalLink).not.toHaveAttribute('rel', 'noopener noreferrer');
    });
  });
});
