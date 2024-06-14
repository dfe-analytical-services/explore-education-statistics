import render from '@common-test/render';
import DataSetFileFootnotes from '@frontend/modules/data-catalogue/components/DataSetFileFootnotes';
import { testDataSetFootnotes } from '@frontend/modules/data-catalogue/__data__/testDataSets';
import { screen } from '@testing-library/react';
import React from 'react';

describe('DataSetFileFootnotes', () => {
  test('renders correctly', () => {
    render(<DataSetFileFootnotes footnotes={testDataSetFootnotes} />);

    expect(
      screen.getByRole('heading', { name: 'Footnotes' }),
    ).toBeInTheDocument();

    const footnotes = screen.getAllByRole('listitem');
    expect(footnotes).toHaveLength(2);
    expect(footnotes[0]).toHaveTextContent('Footnote 1');
    expect(footnotes[1]).toHaveTextContent('Footnote 2');
  });
});
