import React from 'react';
import { render } from 'react-testing-library';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import EditableContentSubBlockRenderer from '../EditableContentSubBlockRenderer';

describe('EditableContentSubBlockRenderer', () => {
  test('Renders non-editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext.Provider value={{ isEditing: true, releaseId: '' }}>
        <EditableContentSubBlockRenderer
          id="test"
          index={1}
          block={{
            id: 'block-000',
            comments: [],
            type: 'MarkDownBlock',
            body: 'test',
          }}
        />
      </EditingContext.Provider>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('Renders editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext.Provider value={{ isEditing: true, releaseId: '' }}>
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
        />
      </EditingContext.Provider>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
