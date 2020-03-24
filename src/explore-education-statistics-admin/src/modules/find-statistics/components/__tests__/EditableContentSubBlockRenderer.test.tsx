import EditableBlockRenderer from '@admin/components/editable/EditableBlockRenderer';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { render } from '@testing-library/react';
import React from 'react';

describe('EditableBlockRenderer', () => {
  test('Renders non-editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext.Provider
        value={{
          isEditing: true,
        }}
      >
        <EditableBlockRenderer
          block={{
            id: 'block-000',
            order: 0,
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
        <EditableBlockRenderer
          canDelete
          editable
          block={{
            id: 'block-000',
            order: 0,
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
