import React from 'react';
import { render } from 'react-testing-library';
import Details from '../Details';

describe('Details', () => {
  test('renders correctly', () => {
    const { container } = render(
      <Details summary="Test summary">
        <p>Test details</p>
      </Details>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
