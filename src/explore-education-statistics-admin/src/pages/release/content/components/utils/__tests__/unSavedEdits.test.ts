import {
  addUnsavedEdit,
  removeUnsavedEdit,
} from '@admin/pages/release/content/components/utils/unsavedEdits';

describe('Unsaved edits', () => {
  describe('Adding an unsaved edit', () => {
    test('Adding a new unsaved edit', () => {
      const result = addUnsavedEdit([], 'section-1', 'block-1');
      expect(result).toEqual([
        {
          sectionId: 'section-1',
          blockIds: ['block-1'],
        },
      ]);
    });

    test('Adding a new unsaved edit for a section with other unsaved edits', () => {
      const edits = [
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ];
      const result = addUnsavedEdit(edits, 'section-1', 'block-3');
      expect(result).toEqual([
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2', 'block-3'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ]);
    });

    test('Duplicate edits are not added', () => {
      const edits = [
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ];
      const result = addUnsavedEdit(edits, 'section-1', 'block-2');
      expect(result).toEqual([
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ]);
    });
  });

  describe('Removing an unsaved edit', () => {
    test('Removing an unsaved edit', () => {
      const edits = [
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ];
      const result = removeUnsavedEdit(edits, 'section-2', 'block-4');
      expect(result).toEqual([
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
      ]);
    });

    test('Removing an unsaved edit from a section with multiple unsaved edits', () => {
      const edits = [
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ];
      const result = removeUnsavedEdit(edits, 'section-1', 'block-1');
      expect(result).toEqual([
        {
          sectionId: 'section-1',
          blockIds: ['block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ]);
    });

    test('Do nothing if the unsaved edit does not exist', () => {
      const edits = [
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ];
      const result = removeUnsavedEdit(edits, 'section-3', 'block-5');
      expect(result).toEqual([
        {
          sectionId: 'section-1',
          blockIds: ['block-1', 'block-2'],
        },
        {
          sectionId: 'section-2',
          blockIds: ['block-4'],
        },
      ]);
    });
  });
});
