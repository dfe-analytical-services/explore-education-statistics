import React from 'react';
import { MemoryRouter } from 'react-router';
import { render } from 'react-testing-library';
import KeyIndicator from '../KeyIndicator';

describe('KeyIndicator', () => {
  const defaultProps = {
    link: '#',
    reference: 'Test reference',
    referenceLink: '#',
    title: 'Test title',
    units: '%',
    value: 50,
  };

  test('text is correct for a positive change', () => {
    const { getByText } = render(
      <MemoryRouter>
        <KeyIndicator
          {...defaultProps}
          changes={[
            {
              description: '16/17',
              units: 'ppt',
              value: 10,
            },
          ]}
        />
      </MemoryRouter>,
    );

    expect(getByText('\u2BC5 +10ppt higher than 16/17')).toBeDefined();
  });

  test('text is correct for a negative change', () => {
    const { getByText } = render(
      <MemoryRouter>
        <KeyIndicator
          {...defaultProps}
          changes={[
            {
              description: '16/17',
              units: 'ppt',
              value: -20,
            },
          ]}
        />
      </MemoryRouter>,
    );

    expect(getByText('\u2BC6 -20ppt lower than 16/17')).toBeDefined();
  });
});
