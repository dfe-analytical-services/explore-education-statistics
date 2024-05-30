import { getAllDescribedBy, getDescribedBy } from '@common-test/queries';
import TaskListItem from '@common/components/TaskListItem';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('TaskListItem', () => {
  test('provides correct `aria-describedby` render prop when `hint` is set', () => {
    render(
      <TaskListItem hint="Test hint" id="test" status="Completed">
        {props => (
          // eslint-disable-next-line react/jsx-props-no-spreading
          <a {...props} href="/">
            Test
          </a>
        )}
      </TaskListItem>,
    );

    const descriptors = getAllDescribedBy(screen.getByRole('link'));

    expect(descriptors).toHaveLength(2);
    expect(descriptors[0]).toHaveTextContent('Completed');
    expect(descriptors[1]).toHaveTextContent('Test hint');
  });

  test('provides correct `aria-describedby` render prop when `hint` is not set', () => {
    render(
      <TaskListItem id="test" status="Completed">
        {props => (
          // eslint-disable-next-line react/jsx-props-no-spreading
          <a {...props} href="/">
            Test
          </a>
        )}
      </TaskListItem>,
    );

    expect(getDescribedBy(screen.getByRole('link'))).toHaveTextContent(
      'Completed',
    );
  });
});
