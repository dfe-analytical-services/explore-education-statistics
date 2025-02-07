import { AuthContextTestProvider } from '@admin/contexts/AuthContext';
import EditableAccordion from '@admin/components/editable/EditableAccordion';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import ReleaseContentAccordionSection from '@admin/pages/release/content/components/ReleaseContentAccordionSection';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import { UserDetails } from '@admin/services/types/user';
import generateReleaseContent from '@admin-test/generators/releaseContentGenerators';
import {
  generateContentSection,
  generateEditableContentBlock,
} from '@admin-test/generators/contentGenerators';
import MockDate from '@common-test/mockDate';
import { getDescribedBy } from '@common-test/queries';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/hubs/utils/createConnection');

describe('ReleaseContentAccordionSection', () => {
  const testReleaseContent = generateReleaseContent({});

  test('renders correctly in editable mode', () => {
    render(
      <EditingContextProvider editingMode="edit">
        <ReleaseContentProvider
          value={{
            ...testReleaseContent,
            canUpdateRelease: true,
          }}
        >
          <ReleaseContentHubContextProvider
            releaseVersionId={testReleaseContent.release.id}
          >
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={testReleaseContent.release.content[0]}
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
              ...testReleaseContent,
              canUpdateRelease: true,
            }}
          >
            <ReleaseContentHubContextProvider
              releaseVersionId={testReleaseContent.release.id}
            >
              <EditableAccordion
                onAddSection={noop}
                id="test-accordion"
                onReorder={noop}
              >
                <ReleaseContentAccordionSection
                  id="test-section-1"
                  section={testReleaseContent.release.content[0]}
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
            ...testReleaseContent,
            canUpdateRelease: true,
          }}
        >
          <ReleaseContentHubContextProvider
            releaseVersionId={testReleaseContent.release.id}
          >
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={testReleaseContent.release.content[0]}
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
      <EditingContextProvider
        editingMode="edit"
        unsavedBlocks={['content-block-1']}
      >
        <ReleaseContentProvider
          value={{
            ...testReleaseContent,
            canUpdateRelease: true,
          }}
        >
          <ReleaseContentHubContextProvider
            releaseVersionId={testReleaseContent.release.id}
          >
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={testReleaseContent.release.content[0]}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.getByRole('heading', {
        name: /Content section 1 \(unsaved changes\)/,
      }),
    ).toBeInTheDocument();
  });

  test('renders heading with unsaved changes if there are any pending comment deletions', () => {
    render(
      <EditingContextProvider
        editingMode="edit"
        unsavedCommentDeletions={{
          'content-block-1': ['comment-1'],
        }}
      >
        <ReleaseContentProvider
          value={{
            ...testReleaseContent,
            canUpdateRelease: true,
          }}
        >
          <ReleaseContentHubContextProvider
            releaseVersionId={testReleaseContent.release.id}
          >
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={testReleaseContent.release.content[0]}
              />
            </EditableAccordion>
          </ReleaseContentHubContextProvider>
        </ReleaseContentProvider>
      </EditingContextProvider>,
    );

    expect(
      screen.getByRole('heading', {
        name: /Content section 1 \(unsaved changes\)/,
      }),
    ).toBeInTheDocument();
  });

  test('renders locked state if any child blocks are locked', () => {
    MockDate.set('2022-03-12T12:05:00Z');

    const testOtherUser: UserDetails = {
      id: 'user-2',
      displayName: 'Rob Rowe',
      email: 'rob@test.com',
    };

    const testSectionWithLockedBlock = generateContentSection({
      heading: 'Test section',
      content: [
        generateEditableContentBlock({ id: 'test-block' }),
        generateEditableContentBlock({
          id: 'test-block-locked',
          locked: '2022-03-12T12:00:00Z',
          lockedUntil: '2022-03-12T12:10:00Z',
          lockedBy: testOtherUser,
        }),
      ],
    });

    render(
      <EditingContextProvider editingMode="edit">
        <ReleaseContentProvider
          value={{
            ...testReleaseContent,
            canUpdateRelease: true,
          }}
        >
          <ReleaseContentHubContextProvider
            releaseVersionId={testReleaseContent.release.id}
          >
            <EditableAccordion
              onAddSection={noop}
              id="test-accordion"
              onReorder={noop}
            >
              <ReleaseContentAccordionSection
                id="test-section-1"
                section={testSectionWithLockedBlock}
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
