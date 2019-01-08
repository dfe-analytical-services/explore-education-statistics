import React from 'react';
import { MemoryRouter } from 'react-router';
import { render } from 'react-testing-library';
import Button from '../Button';

describe('Button', () => {
  test('renders correctly as button element', () => {
    const { container } = render(<Button>Test button</Button>);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly as anchor element', () => {
    const { container } = render(
      <MemoryRouter>
        <Button to="#">Test button</Button>
      </MemoryRouter>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly sets disabled attributes as button element', () => {
    const { getByText } = render(<Button disabled>Test button</Button>);

    const button = getByText('Test button');

    expect(button).toHaveAttribute('disabled');
    expect(button).toHaveAttribute('aria-disabled', 'true');
  });

  test('correctly set disabled attributes as anchor element', () => {
    const { getByText } = render(
      <MemoryRouter>
        <Button to="#">Test button</Button>
      </MemoryRouter>,
    );

    const button = getByText('Test button');

    expect(button).not.toHaveAttribute('disabled');
    expect(button).toHaveAttribute('aria-disabled', 'false');
  });
});
