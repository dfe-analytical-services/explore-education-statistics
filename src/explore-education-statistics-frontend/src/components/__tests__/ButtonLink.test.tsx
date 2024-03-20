import React from 'react';
import { render } from '@testing-library/react';
import ButtonLink from '../ButtonLink';

describe('ButtonLink', () => {
  test('renders correctly with required props', () => {
    const { container } = render(<ButtonLink to="/">Test button</ButtonLink>);

    expect(container.innerHTML).toMatchSnapshot();
  });
});
