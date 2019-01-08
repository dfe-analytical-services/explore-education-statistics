import React from 'react';
import { render } from 'react-testing-library';
import { ChangeText } from '../ChangeText';

describe('ChangeText', () => {
  test('text is correct for a positive change', () => {
    const { getByText } = render(
      <span>
        <ChangeText description="16/17" units="ppt" value={10} />
      </span>,
    );

    expect(getByText('\u2BC5 +10ppt higher than 16/17')).toBeDefined();
  });

  test('text is correct for a negative change', () => {
    const { getByText } = render(
      <span>
        <ChangeText description="16/17" units="ppt" value={-20} />
      </span>,
    );

    expect(getByText('\u2BC6 -20ppt lower than 16/17')).toBeDefined();
  });
});
