import React from 'react';
import { render } from 'react-testing-library';

import EditableContentSubBlockRenderer from '../EditableContentSubBlockRenderer';

describe('EditableContentSubBlockRenderer', () => {
  test('Renders non-editable Markdown block correctly', () => {
    const { container } = render(
      <EditableContentSubBlockRenderer
        id="test"
        index={1}
        block={{
          id: 'block-000',
          comments: [],
          type: 'MarkDownBlock',
          body: 'test',
        }}
      />,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('Renders editable Markdown block correctly', () => {
    const { container } = render(
      <EditableContentSubBlockRenderer
        id="test"
        index={1}
        editable
        block={{
          id: 'block-000',
          comments: [],
          type: 'MarkDownBlock',
          body: 'test',
        }}
      />,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
