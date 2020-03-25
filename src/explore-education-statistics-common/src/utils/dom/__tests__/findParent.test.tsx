import { render } from '@testing-library/react';
import React from 'react';
import findParent from '../findParent';

describe('findParent', () => {
  test('matches id', () => {
    const { getByText } = render(
      <div id="test">
        <div className="test">
          <p>Start</p>
        </div>
      </div>,
    );

    expect(findParent(getByText('Start'), '#test')).toHaveAttribute(
      'id',
      'test',
    );
  });

  test('matches class', () => {
    const { getByText } = render(
      <div className="test">
        <div id="test">
          <p>Start</p>
        </div>
      </div>,
    );

    expect(findParent(getByText('Start'), '.test')).toHaveClass('test');
  });

  test('matches first matching parent', () => {
    const { getByText } = render(
      <div id="test">
        <div className="test">
          <p>Start</p>
        </div>
      </div>,
    );

    expect(findParent(getByText('Start'), '.test, #test')).toHaveClass('test');
  });

  test('matches no parent', () => {
    const { getByText } = render(
      <div>
        <p>Start</p>
      </div>,
    );

    expect(findParent(getByText('Start'), '.test')).toBeNull();
  });
});
