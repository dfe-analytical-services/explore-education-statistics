import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('toHaveScrolledIntoView', () => {
  test('asserts that element scrolls into view successfully', () => {
    render(
      <div>
        <div id="test">Test element</div>

        <button
          type="button"
          onClick={() => {
            const el = document.querySelector('#test');

            if (el) {
              el.scrollIntoView();
            }
          }}
        >
          Scroll to element
        </button>
      </div>,
    );

    const element = screen.getByText('Test element');

    expect(element).not.toHaveScrolledIntoView();

    userEvent.click(screen.getByText('Scroll to element'));

    expect(element).toHaveScrolledIntoView();
  });

  test('throws when assertions are incorrect', () => {
    render(
      <div>
        <div id="test">Test element</div>

        <button
          type="button"
          onClick={() => {
            const el = document.querySelector('#test');

            if (el) {
              el.scrollIntoView();
            }
          }}
        >
          Scroll to element
        </button>
      </div>,
    );

    const element = screen.getByText('Test element');

    expect(() => expect(element).toHaveScrolledIntoView()).toThrow();

    userEvent.click(screen.getByText('Scroll to element'));

    expect(() => expect(element).not.toHaveScrolledIntoView()).toThrow();
  });
});
