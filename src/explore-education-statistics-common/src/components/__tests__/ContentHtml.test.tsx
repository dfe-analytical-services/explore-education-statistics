import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ContentHtml from '@common/components/ContentHtml';
import { GlossaryEntry } from '@common/services/types/glossary';
import { within } from '@testing-library/dom';

describe('ContentHtml', () => {
  test('renders correctly with required props', () => {
    render(<ContentHtml html="<p>Test text</p>" />);

    expect(screen.getByText('Test text')).toBeInTheDocument();
  });

  describe('rendering glossary links', () => {
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
  });

  describe('rendering featured table links', () => {
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

    test('does not transform featured table links when `transformFeaturedTableLinks` is not provided', () => {
      render(
        <ContentHtml html="<a href='/data-table/fast-track/124356575?featuredTable=true' data-featured-table>Featured table</a>" />,
      );

      expect(screen.queryByText('I am transformed')).not.toBeInTheDocument();
      expect(
        screen.getByRole('link', { name: 'Featured table' }),
      ).toBeInTheDocument();
    });
  });

  describe('rendering tables', () => {
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

  describe('rendering links', () => {
    test('encodes special characters in urls', () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/TEST something">External link</a>' />,
      );

      expect(
        screen.getByRole('link', {
          name: 'External link (opens in new tab)',
        }),
      ).toHaveAttribute('href', 'https://gov.uk/TEST%20something');
    });

    test('trims whitespace in urls', () => {
      render(
        <ContentHtml html='<a href="   https://gov.uk/TEST  ">External link</a>">External link</a>' />,
      );

      expect(
        screen.getByRole('link', {
          name: 'External link (opens in new tab)',
        }),
      ).toHaveAttribute('href', 'https://gov.uk/TEST');
    });

    test('lower cases internal urls, excluding query params', () => {
      render(
        <ContentHtml html='<a href="https://explore-education-statistics.service.gov.uk/Find-Statistics?testParam=Something">Internal link</a>' />,
      );

      expect(
        screen.getByRole('link', {
          name: 'Internal link',
        }),
      ).toHaveAttribute(
        'href',
        'https://explore-education-statistics.service.gov.uk/find-statistics?testParam=Something',
      );
    });

    test('does not lower case external urls', () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/TEST">External link</a>' />,
      );
      expect(
        screen.getByRole('link', {
          name: 'External link (opens in new tab)',
        }),
      ).toHaveAttribute('href', 'https://gov.uk/TEST');
    });

    test('opens external links in a new tab', () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/">External link</a>' />,
      );
      const link = screen.getByRole('link', {
        name: 'External link (opens in new tab)',
      });
      expect(link).toHaveAttribute('href', 'https://gov.uk/');
      expect(link).toHaveAttribute('target', '_blank');
    });

    test('opens internal links in the same tab', () => {
      render(
        <ContentHtml html='<a href="https://explore-education-statistics.service.gov.uk/">Internal link</a>' />,
      );
      const link = screen.getByRole('link', {
        name: 'Internal link',
      });
      expect(link).toHaveAttribute(
        'href',
        'https://explore-education-statistics.service.gov.uk/',
      );
      expect(link).not.toHaveAttribute('target', '_blank');
    });

    test("appends '(opens in new tab)' to the name for external links", () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/">External link</a>' />,
      );

      const link = screen.getByRole('link');

      expect(link.textContent).toBe('External link (opens in new tab)');
    });

    test("does not append '(opens in new tab)' to any 'mailto:' links", () => {
      render(
        <ContentHtml html='<a href="mailto:explore.statistics@education.gov.uk">Mailto link</a>' />,
      );

      const link = screen.getByRole('link');

      expect(link.textContent).toBe('Mailto link');
    });

    test('adds the correct rel attributes for trusted external links', () => {
      render(
        <ContentHtml html='<a href="https://gov.uk/">External link</a>' />,
      );

      const link = screen.getByRole('link');
      expect(link).toHaveAttribute('rel', 'noopener noreferrer nofollow');
    });

    test('adds the correct rel attributes for untrusted external links', () => {
      render(
        <ContentHtml html='<a href="https://example.com/">External link</a>' />,
      );

      const link = screen.getByRole('link');
      expect(link).toHaveAttribute(
        'rel',
        'noopener noreferrer nofollow external',
      );
    });

    test('does not add rel attributes on internal links', () => {
      render(
        <ContentHtml html='<a href="https://explore-education-statistics.service.gov.uk/">Internal link</a>' />,
      );

      const link = screen.getByRole('link');
      expect(link).not.toHaveAttribute('rel');
    });

    test('does not format links when `formatLinks` is false', () => {
      render(
        <ContentHtml
          formatLinks={false}
          html={` 
          <a href="https://explore-education-statistics.service.gov.uk/Find-Statistics?testParam=Something">
          Internal link</a> 
          <a href="  https://gov.uk/TEST something  ">External link</a>`}
        />,
      );

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
      expect(externalLink).not.toHaveAttribute(
        'rel',
        'noopener noreferrer nofollow',
      );
    });
  });
});
