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

    userEvent.click(glossaryButton);

    await waitFor(() => {
      expect(getEntry).toHaveBeenCalled();
    });

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Absence heading' }),
    ).toBeInTheDocument();
    expect(modal.getByText('Absence body')).toBeInTheDocument();
    const closeButton = modal.getByRole('button', { name: 'Close' });
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
    userEvent.click(glossaryButton);

    await waitFor(() => {
      expect(getEntry).toHaveBeenCalled();
    });

    const modal = within(screen.getByRole('dialog'));
    const closeButton = modal.getByRole('button', { name: 'Close' });
    userEvent.click(closeButton);

    await waitFor(() => {
      expect(closeButton).not.toBeInTheDocument();
      expect(glossaryButton).toBeInTheDocument();
    });
  });

  test('fixes inaccessible table markup from CKEditor', () => {
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
        <div class="tableContainer">
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
