import React from 'react';
import { render } from 'react-testing-library';
import Button from '../Button';

describe('Button', () => {
  test('renders correctly with required props', () => {
    const { container } = render(<Button>Test button</Button>);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly sets disabled attributes', () => {
    const { getByText } = render(<Button disabled>Test button</Button>);

    const button = getByText('Test button');

    expect(button).toHaveAttribute('disabled');
    expect(button).toHaveAttribute('aria-disabled', 'true');
  });
});
