import React from 'react';
import { fireEvent, render } from 'react-testing-library';

describe('toHaveScrolledIntoView', () => {
  test('asserts that element scrolls into view successfully', () => {
    const { getByText } = render(
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

    const element = getByText('Test element');

    expect(element).not.toHaveScrolledIntoView();

    fireEvent.click(getByText('Scroll to element'));

    expect(element).toHaveScrolledIntoView();
  });

  test('throws when assertions are incorrect', () => {
    const { getByText } = render(
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

    const element = getByText('Test element');

    expect(() => expect(element).toHaveScrolledIntoView()).toThrow();

    fireEvent.click(getByText('Scroll to element'));

    expect(() => expect(element).not.toHaveScrolledIntoView()).toThrow();
  });
});
