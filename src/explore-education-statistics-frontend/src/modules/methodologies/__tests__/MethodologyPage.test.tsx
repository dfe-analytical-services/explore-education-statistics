import MethodologyPage from '@frontend/pages/methodology/[methodology]';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { testMethodology } from './__data__/testMethodologyData';

describe('MethodologyPage', () => {
  test('renders methodology page basic details', async () => {
    render(<MethodologyPage data={testMethodology} />);

    expect(screen.getByTestId('page-title-caption')).toHaveTextContent(
      'Methodology',
    );

    expect(
      screen.getByRole('heading', {
        name: 'Pupil absence statistics: methodology',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('Published-value')).toHaveTextContent(
      '16 February 2021',
    );

    expect(
      screen.getByLabelText('Search in this methodology page.'),
    ).toBeInTheDocument();

    expect(
      screen.getAllByRole('button', { name: 'Print this page' }),
    ).toHaveLength(2);
  });

  test('renders the methodology content', () => {
    render(<MethodologyPage data={testMethodology} />);
    const contentAccordion = screen.getAllByTestId('accordion')[0];
    const contentAccordionSections = within(contentAccordion).getAllByTestId(
      'accordionSection',
    );

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

  test('renders the methodology annexes', () => {
    render(<MethodologyPage data={testMethodology} />);
    const annexAccordion = screen.getAllByTestId('accordion')[1];
    const annexAccordionSections = within(annexAccordion).getAllByTestId(
      'accordionSection',
    );

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
    test(`doesn't render 'Last updated' if there are no notes`, () => {
      render(
        <MethodologyPage
          data={{
            ...testMethodology,
            notes: [],
          }}
        />,
      );

      expect(screen.queryByTestId('Last updated')).not.toBeInTheDocument();
    });

    test(`renders 'Last updated' with the date of the most recent note`, () => {
      render(<MethodologyPage data={testMethodology} />);

      expect(screen.getByTestId('Last updated-value')).toHaveTextContent(
        '15 September 2021',
      );
    });

    test(`renders the list of all notes`, () => {
      render(<MethodologyPage data={testMethodology} />);

      userEvent.click(
        screen.getByRole('button', { name: 'See all notes (3)' }),
      );

      const notes = within(screen.getByTestId('notes')).getAllByRole(
        'listitem',
      );

      expect(notes).toHaveLength(3);

      expect(
        within(notes[0]).getByTestId('note-displayDate'),
      ).toHaveTextContent('15 September 2021');
      expect(within(notes[0]).getByTestId('note-content')).toHaveTextContent(
        'Latest note',
      );

      expect(
        within(notes[1]).getByTestId('note-displayDate'),
      ).toHaveTextContent('19 April 2021');
      expect(within(notes[1]).getByTestId('note-content')).toHaveTextContent(
        'Other note',
      );

      expect(
        within(notes[2]).getByTestId('note-displayDate'),
      ).toHaveTextContent('1 March 2021');
      expect(within(notes[2]).getByTestId('note-content')).toHaveTextContent(
        'Earliest note',
      );
    });
  });
});
