import { render } from '@testing-library/react';
import React from 'react';
import findAllParents from '../findAllParents';

describe('findAllParents', () => {
  test('matches different parents in correct order', () => {
    const { getByText } = render(
      <div className="test-1">
        <div>
          <div className="test-2">
            <p>Start</p>
          </div>
        </div>
      </div>,
    );

    const parents = findAllParents(getByText('Start'), '.test-1, .test-2');

    expect(parents).toHaveLength(2);
    expect(parents[0]).toHaveClass('test-2');
    expect(parents[1]).toHaveClass('test-1');
  });

  test('matches multiple parents of same type in correct order', () => {
    const { getByText } = render(
      <div className="test" id="el-1">
        <div>
          <div className="test" id="el-2">
            <p>Start</p>
          </div>
        </div>
      </div>,
    );

    const parents = findAllParents(getByText('Start'), '.test');

    expect(parents).toHaveLength(2);
    expect(parents[0]).toHaveAttribute('id', 'el-2');
    expect(parents[1]).toHaveAttribute('id', 'el-1');
  });

  test('matches single parent', () => {
    const { getByText } = render(
      <div className="test-1">
        <div>
          <div className="test-2">
            <p>Start</p>
          </div>
        </div>
      </div>,
    );

    const parents = findAllParents(getByText('Start'), '.test-1');

    expect(parents).toHaveLength(1);
    expect(parents[0]).toHaveClass('test-1');
  });

  test('matches no parents', () => {
    const { getByText } = render(
      <div>
        <div>
          <div>
            <p>Start</p>
          </div>
        </div>
      </div>,
    );

    const parents = findAllParents(getByText('Start'), '.test');

    expect(parents).toHaveLength(0);
  });
});
