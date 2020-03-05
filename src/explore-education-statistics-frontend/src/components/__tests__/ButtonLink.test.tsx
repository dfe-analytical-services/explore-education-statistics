import React from 'react';
import { render } from '@testing-library/react';
import ButtonLink from '../ButtonLink';

describe('ButtonLink', () => {
  test('renders correctly with required props', () => {
    const { container } = render(<ButtonLink to="/">Test button</ButtonLink>);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly set disabled attributes', () => {
    const { getByText } = render(<ButtonLink to="/">Test button</ButtonLink>);

    const button = getByText('Test button');

    expect(button).not.toHaveAttribute('disabled');
    expect(button).toHaveAttribute('aria-disabled', 'false');
  });
});
