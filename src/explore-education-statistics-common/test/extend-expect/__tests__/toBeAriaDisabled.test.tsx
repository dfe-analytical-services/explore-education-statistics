import { render, screen } from '@testing-library/react';
import React from 'react';

describe('toBeAriaDisabled', () => {
  test('does not throw asserting `aria-disabled` element is disabled', () => {
    render(
      <button aria-disabled type="button">
        Test button
      </button>,
    );

    expect(() =>
      expect(screen.getByRole('button')).toBeAriaDisabled(),
    ).not.toThrow();
  });

  test('throws asserting `aria-disabled` element is not disabled', () => {
    render(
      <button aria-disabled type="button">
        Test button
      </button>,
    );

    expect(() =>
      expect(screen.getByRole('button')).not.toBeAriaDisabled(),
    ).toThrow();
  });

  test('throws asserting element that is not `aria-disabled` is disabled', () => {
    render(<button type="button">Test button</button>);

    expect(() =>
      expect(screen.getByRole('button')).toBeAriaDisabled(),
    ).toThrow();
  });

  test('does not throw asserting element that is `aria-disabled` is not disabled', () => {
    render(<button type="button">Test button</button>);

    expect(() =>
      expect(screen.getByRole('button')).not.toBeAriaDisabled(),
    ).not.toThrow();
  });
});
