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
  });

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

    userEvent.click(screen.getByRole('button', { name: 'See all notes (3)' }));

    const notes = within(screen.getByTestId('notes')).getAllByRole('listitem');

    expect(notes).toHaveLength(3);

    expect(within(notes[0]).getByTestId('note-displayDate')).toHaveTextContent(
      '15 September 2021',
    );
    expect(within(notes[0]).getByTestId('note-content')).toHaveTextContent(
      'Latest note',
    );

    expect(within(notes[1]).getByTestId('note-displayDate')).toHaveTextContent(
      '19 April 2021',
    );
    expect(within(notes[1]).getByTestId('note-content')).toHaveTextContent(
      'Other note',
    );

    expect(within(notes[2]).getByTestId('note-displayDate')).toHaveTextContent(
      '1 March 2021',
    );
    expect(within(notes[2]).getByTestId('note-content')).toHaveTextContent(
      'Earliest note',
    );
  });
});
