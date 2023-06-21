import React from 'react';
import { render } from '@testing-library/react';
import VisuallyHidden from '../VisuallyHidden';

describe('VisuallyHidden', () => {
  test('renders correctly with no `as` prop', () => {
    const { container } = render(<VisuallyHidden>Test</VisuallyHidden>);

    expect(container.querySelector('.govuk-visually-hidden')).toHaveTextContent(
      'Test',
    );

    expect(container.querySelector('.govuk-visually-hidden')).toHaveProperty(
      'tagName',
      'SPAN',
    );
  });

  test('renders correctly with `as` prop', () => {
    const { container } = render(<VisuallyHidden as="h1">Test</VisuallyHidden>);

    expect(container.querySelector('.govuk-visually-hidden')).toHaveTextContent(
      'Test',
    );

    expect(container.querySelector('.govuk-visually-hidden')).toHaveProperty(
      'tagName',
      'H1',
    );
  });
});
