import { render } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import ButtonLink from '../ButtonLink';

describe('ButtonLink', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <MemoryRouter>
        <ButtonLink to="/">Test button</ButtonLink>
      </MemoryRouter>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly set disabled attributes', () => {
    const { getByText } = render(
      <MemoryRouter>
        <ButtonLink to="/">Test button</ButtonLink>
      </MemoryRouter>,
    );

    const button = getByText('Test button');

    expect(button).not.toHaveAttribute('disabled');
    expect(button).toHaveAttribute('aria-disabled', 'false');
  });
});
