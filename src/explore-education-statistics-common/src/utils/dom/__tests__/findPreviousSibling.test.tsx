import { render } from '@testing-library/react';
import React from 'react';
import findSibling from '../findPreviousSibling';

describe('findPreviousSibling', () => {
  test('matches id', () => {
    const { getByText } = render(
      <div>
        <p id="test">Me</p>
        <p>Not me</p>
        <p>Start</p>
      </div>,
    );

    expect(findSibling(getByText('Start'), '#test')).toHaveAttribute(
      'id',
      'test',
    );
  });

  test('matches class', () => {
    const { getByText } = render(
      <div>
        <p className="test">Me</p>
        <p>Not me</p>
        <p>Start</p>
      </div>,
    );

    expect(findSibling(getByText('Start'), '.test')).toHaveClass('test');
  });

  test('matches first matching sibling', () => {
    const { getByText } = render(
      <div>
        <p id="test">Me</p>
        <p className="test">Not me</p>
        <p>Start</p>
      </div>,
    );

    expect(findSibling(getByText('Start'), '.test, #test')).toHaveClass('test');
  });

  test('matches no siblings', () => {
    const { getByText } = render(
      <div>
        <p className="test">Not me</p>
        <p>Start</p>
      </div>,
    );

    expect(findSibling(getByText('Start'), '.test-2')).toBeNull();
  });
});
