import EditableBlockRenderer from '@admin/components/editable/EditableBlockRenderer';
import { EditingContext as EditingContext1 } from '@admin/contexts/EditingContext';
import ManageReleaseContext from '@admin/pages/release/ManageReleaseContext';
import { BasicPublicationDetails } from '@admin/services/common/types';
import { render } from '@testing-library/react';
import React from 'react';

describe('EditableBlockRenderer', () => {
  test('renders non-editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext1
        value={{
          isEditing: true,
        }}
      >
        <ManageReleaseContext.Provider
          value={{
            releaseId: '1',
            publication: {} as BasicPublicationDetails,
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
        </ManageReleaseContext.Provider>
      </EditingContext1>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders editable Markdown block correctly', () => {
    const { container } = render(
      <EditingContext1
        value={{
          isEditing: true,
        }}
      >
        <ManageReleaseContext.Provider
          value={{
            releaseId: '1',
            publication: {} as BasicPublicationDetails,
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
        </ManageReleaseContext.Provider>
      </EditingContext1>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
