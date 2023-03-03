import { AuthContextTestProvider } from '@admin/contexts/AuthContext';
import EditableAccordion from '@admin/components/editable/EditableAccordion';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import { testEditableRelease } from '@admin/pages/release/__data__/testEditableRelease';
import ReleaseContentAccordionSection from '@admin/pages/release/content/components/ReleaseContentAccordionSection';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import {
  EditableBlock,
  EditableContentBlock,
} from '@admin/services/types/content';
import { UserDetails } from '@admin/services/types/user';
import MockDate from '@common-test/mockDate';
import { getDescribedBy } from '@common-test/queries';
import { ContentSection } from '@common/services/publicationService';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/hubs/utils/createConnection');

describe('ReleaseContentAccordionSection', () => {
  const testOtherUser: UserDetails = {
    id: 'user-2',
    displayName: 'Rob Rowe',
    email: 'rob@test.com',
  };

  const testBlock: EditableContentBlock = {
    id: 'block-1',
    order: 1,
    body: '<p>Test block content</p>',
    type: 'HtmlBlock',
    comments: [],
  };

  const testLockedBlock: EditableContentBlock = {
    id: 'block-2',
    order: 2,
    body: '<p>Test block content</p>',
    type: 'HtmlBlock',
    comments: [],
    locked: '2022-03-12T12:00:00Z',
    lockedUntil: '2022-03-12T12:10:00Z',
    lockedBy: testOtherUser,
  };

  const testSection: ContentSection<EditableBlock> = {
    id: 'section-1',
    order: 1,
    heading: 'Test section 1',
    content: [testBlock],
  };

  test('renders correctly in editable mode', () => {
    render(
      <EditingContextProvider editingMode="edit">
        <ReleaseContentProvider
          value={{
            release: testEditableRelease,
            canUpdateRelease: true,
            unattachedDataBlocks: [],
          }}
        >
          <ReleaseContentHubContextProvider releaseId={testEditableRelease.id}>
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={{
                  ...testSection,
                  content: [testBlock],
                }}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.getByRole('button', { name: 'Edit section title' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder this section' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove this section' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Add text block' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add data block' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Embed a URL' }),
    ).not.toBeInTheDocument();
  });

  test('renders the Embed a URL button for BAU users', () => {
    render(
      <AuthContextTestProvider
        user={{
          id: 'user-1',
          name: 'Jane Doe',
          permissions: {
            isBauUser: true,
          } as GlobalPermissions,
        }}
      >
        <EditingContextProvider editingMode="edit">
          <ReleaseContentProvider
            value={{
              release: testEditableRelease,
              canUpdateRelease: true,
              unattachedDataBlocks: [],
            }}
          >
            <ReleaseContentHubContextProvider
              releaseId={testEditableRelease.id}
            >
              <EditableAccordion
                onAddSection={noop}
                id="test-accordion"
                onReorder={noop}
              >
                <ReleaseContentAccordionSection
                  id="test-section-1"
                  section={{
                    ...testSection,
                    content: [testBlock],
                  }}
                />
              </EditableAccordion>
            </ReleaseContentHubContextProvider>
          </ReleaseContentProvider>
        </EditingContextProvider>
      </AuthContextTestProvider>,
    );

    expect(
      screen.getByRole('button', { name: 'Edit section title' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder this section' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove this section' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Add text block' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add data block' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Embed a URL' }),
    ).toBeInTheDocument();
  });

  test('renders correctly in preview mode', () => {
    render(
      <EditingContextProvider editingMode="preview">
        <ReleaseContentProvider
          value={{
            release: testEditableRelease,
            canUpdateRelease: true,
            unattachedDataBlocks: [],
          }}
        >
          <ReleaseContentHubContextProvider releaseId={testEditableRelease.id}>
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={{
                  ...testSection,
                  content: [testBlock],
                }}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.queryByRole('button', { name: 'Edit section title' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reorder this section' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove this section' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Add text block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Add data block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Embed a URL' }),
    ).not.toBeInTheDocument();
  });

  test('renders heading with unsaved changes if there are any unsaved blocks', () => {
    render(
      <EditingContextProvider editingMode="edit" unsavedBlocks={['block-1']}>
        <ReleaseContentProvider
          value={{
            release: testEditableRelease,
            canUpdateRelease: true,
            unattachedDataBlocks: [],
          }}
        >
          <ReleaseContentHubContextProvider releaseId={testEditableRelease.id}>
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={{
                  ...testSection,
                  content: [testBlock],
                }}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.getByRole('heading', { name: 'Test section 1 (unsaved changes)' }),
    ).toBeInTheDocument();
  });

  test('renders heading with unsaved changes if there are any pending comment deletions', () => {
    render(
      <EditingContextProvider
        editingMode="edit"
        unsavedCommentDeletions={{
          'block-1': ['comment-1'],
        }}
      >
        <ReleaseContentProvider
          value={{
            release: testEditableRelease,
            canUpdateRelease: true,
            unattachedDataBlocks: [],
          }}
        >
          <ReleaseContentHubContextProvider releaseId={testEditableRelease.id}>
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={{
                  ...testSection,
                  content: [testBlock],
                }}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.getByRole('heading', { name: 'Test section 1 (unsaved changes)' }),
    ).toBeInTheDocument();
  });

  test('renders locked state if any child blocks are locked', () => {
    MockDate.set('2022-03-12T12:05:00Z');

    render(
      <EditingContextProvider editingMode="edit">
        <ReleaseContentProvider
          value={{
            release: testEditableRelease,
            canUpdateRelease: true,
            unattachedDataBlocks: [],
          }}
        >
          <ReleaseContentHubContextProvider releaseId={testEditableRelease.id}>
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={{
                  ...testSection,
                  content: [testBlock, testLockedBlock],
                }}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.getByRole('button', { name: 'Edit section title' }),
    ).not.toBeAriaDisabled();

    const reorderButton = screen.getByRole('button', {
      name: 'Reorder this section',
    });
    expect(reorderButton).toBeAriaDisabled();
    expect(getDescribedBy(reorderButton)).toHaveTextContent(
      'This section is being edited and cannot be reordered',
    );

    const removeButton = screen.getByRole('button', {
      name: 'Remove this section',
    });
    expect(removeButton).toBeAriaDisabled();
    expect(getDescribedBy(removeButton)).toHaveTextContent(
      'This section is being edited and cannot be removed',
    );
  });
});
