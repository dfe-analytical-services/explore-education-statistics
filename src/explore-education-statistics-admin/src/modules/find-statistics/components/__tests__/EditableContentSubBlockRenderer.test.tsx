import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { render } from '@testing-library/react';
import React from 'react';
import EditableContentSubBlockRenderer from '../../../editable-components/EditableContentSubBlockRenderer';

describe('EditableContentSubBlockRenderer', () => {
  test('Renders non-editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext.Provider
        value={{
          isEditing: true,
        }}
      >
        <EditableContentSubBlockRenderer
          block={{
            id: 'block-000',
            comments: [],
            type: 'MarkDownBlock',
            body: 'test',
          }}
          onDelete={() => {}}
          onContentChange={async () => {}}
        />
      </EditingContext.Provider>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('Renders editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext.Provider
        value={{
          isEditing: true,
        }}
      >
        <EditableContentSubBlockRenderer
          canDelete
          editable
          block={{
            id: 'block-000',
            comments: [],
            type: 'MarkDownBlock',
            body: 'test',
          }}
          onDelete={() => {}}
          onContentChange={async () => {}}
        />
      </EditingContext.Provider>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
