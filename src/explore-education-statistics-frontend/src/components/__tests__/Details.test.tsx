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

  test('renders correctly with `open` prop set to true', () => {
    const { container } = render(
      <Details summary="Test summary" open>
        <p>Test details</p>
      </Details>,
    );

    expect(container.querySelector('[open]')).not.toBeNull();
    expect(container.innerHTML).toMatchSnapshot();
  });
});
