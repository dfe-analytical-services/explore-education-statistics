import { AuthContextTestProvider } from '@admin/contexts/AuthContext';
import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import { testEditableRelease } from '@admin/pages/release/__data__/testEditableRelease';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContentBlockLockEvent } from '@admin/services/hubs/releaseContentHub';
import connectionMock from '@admin/services/hubs/utils/__mocks__/connectionMock';
import { GlobalPermissions } from '@admin/services/permissionService';
import _releaseContentService, {
  EditableRelease,
} from '@admin/services/releaseContentService';
import { EditableBlock } from '@admin/services/types/content';
import { UserDetails } from '@admin/services/types/user';
import mockDate from '@common-test/mockDate';
import { HubConnectionState } from '@microsoft/signalr';
import {
  render as baseRender,
  RenderResult,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactNode } from 'react';

jest.mock('@admin/services/hubs/utils/createConnection');
jest.mock('@admin/services/releaseContentService');

const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;

describe('ReleaseEditableBlock', () => {
  const testHtmlBlock: EditableBlock = {
    id: 'block-1',
    type: 'HtmlBlock',
    body: `<h3>Test heading</h3>
           <p>Some test content</p>`,
    comments: [],
    order: 0,
  };

  const testEmbedBlock: EditableBlock = {
    comments: [],
    id: 'embed-block-id',
    order: 0,
    title: 'Dashboard title',
    type: 'EmbedBlockLink',
    url: 'https://department-for-education.shinyapps.io/test-dashboard',
  };

  const testCurrentUser: UserDetails = {
    id: 'user-1',
    displayName: 'Jane Doe',
    email: 'jane@test.com',
  };

  const testOtherUser: UserDetails = {
    id: 'user-2',
    displayName: 'Rob Rowe',
    email: 'rob@test.com',
  };

  test('renders HTML block', () => {
    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={testHtmlBlock}
      />,
    );

    expect(
      screen.getByText('Test heading', { selector: 'h3' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Some test content', { selector: 'p' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();
  });

  test('renders HTML block with correct image urls', () => {
    const image1SrcSet =
      '/api/releases/{releaseId}/images/some-image-id-100 100w, ' +
      '/api/releases/{releaseId}/images/some-image-id-200 200w, ' +
      '/api/releases/{releaseId}/images/some-image-id-300 300w';

    const image2SrcSet =
      'https://test/some-image-url-100.jpg 100w, ' +
      'https://test/some-image-url-200.jpg 200w, ' +
      'https://test/some-image-url-300.jpg 300w';

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          body: `
          <h3>Test heading</h3>
          <p>Some test content</p>
          <img alt="Test image 1" src="/api/releases/{releaseId}/images/some-image-id" srcset="${image1SrcSet}" />
          <img alt="Test image 2" src="https://test/some-image-url.jpg" srcset="${image2SrcSet}" />
          `,
        }}
      />,
    );

    expect(screen.getByRole('img', { name: 'Test image 1' })).toHaveAttribute(
      'src',
      '/api/releases/release-1/images/some-image-id',
    );
    expect(screen.getByRole('img', { name: 'Test image 1' })).toHaveAttribute(
      'srcset',
      '/api/releases/release-1/images/some-image-id-100 100w, ' +
        '/api/releases/release-1/images/some-image-id-200 200w, ' +
        '/api/releases/release-1/images/some-image-id-300 300w',
    );

    expect(screen.getByRole('img', { name: 'Test image 2' })).toHaveAttribute(
      'src',
      'https://test/some-image-url.jpg',
    );
    expect(screen.getByRole('img', { name: 'Test image 2' })).toHaveAttribute(
      'srcset',
      image2SrcSet,
    );
  });

  test('renders editing state for current user', () => {
    mockDate.set('2022-02-16T12:05:00Z');

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        }}
      />,
    );

    expect(screen.getByRole('textbox')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Save & close' }),
    ).toBeInTheDocument();

    // This message only shows for other users
    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();
  });

  test('clicking `Edit block` button changes the block to editing state', async () => {
    mockDate.set('2022-02-16T12:05:00Z');

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    connectionMock.invoke.mockImplementation(methodName => {
      if (methodName === 'LockContentBlock') {
        return Promise.resolve<ReleaseContentBlockLockEvent>({
          id: 'block-1',
          releaseId: 'release-1',
          sectionId: 'section-1',
          locked: '2022-02-16T12:05:00Z',
          lockedUntil: '2022-02-16T12:15:00Z',
          lockedBy: testCurrentUser,
        });
      }

      return Promise.resolve();
    });

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={testHtmlBlock}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Edit block' }));

    await waitFor(() => {
      expect(screen.getByText('Save & close')).toBeInTheDocument();
    });

    expect(screen.getByRole('textbox')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Save & close' }),
    ).toBeInTheDocument();

    // This message only shows for other users
    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();
  });

  test('clicking `Save and close` completes editing and changes block to read-only state', async () => {
    mockDate.set('2022-02-16T12:05:00Z');

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    connectionMock.invoke.mockResolvedValue(() => Promise.resolve());

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        }}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(screen.getByText('Save & close')).toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Save & close' }));

    await waitFor(() => {
      expect(screen.getByText('Edit block')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();
  });

  test('does not revert to editable state after editing block and clicking `Save and close`', async () => {
    mockDate.set('2022-02-16T12:05:00Z');

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    connectionMock.invoke.mockResolvedValue(() => Promise.resolve());

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        }}
      />,
    );

    expect(screen.getByText('Save & close')).toBeInTheDocument();

    await userEvent.type(screen.getByRole('textbox'), 'test');

    userEvent.click(screen.getByRole('button', { name: 'Save & close' }));

    await waitFor(() => {
      expect(screen.getByText('Edit block')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();

    jest.useFakeTimers();

    // After editing the block, another timeout triggers to refresh
    // the lock, but we shouldn't action this if the user has already
    // returned to the read-only state (as they have finished editing).
    jest.advanceTimersByTime(15_000);

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();

    jest.useRealTimers();
  });

  test('renders locked state when already locked by other user', () => {
    mockDate.set('2022-02-16T12:09:00Z');

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        }}
      />,
    );

    expect(
      screen.getByText(
        'Rob Rowe (rob@test.com) is currently editing this block (last updated 12:00)',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).toBeAriaDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeAriaDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();
  });

  test("renders unlocked state when other user's lock has already expired", () => {
    // Lock has already expired
    mockDate.set('2022-02-16T12:11:00Z');

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        }}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();
  });

  test("renders unlocked state when other user's lock expires", async () => {
    // Lock is about to expire
    mockDate.set('2022-02-16T12:08:00Z');

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        }}
      />,
    );

    expect(
      screen.getByText(
        'Rob Rowe (rob@test.com) is currently editing this block (last updated 12:00)',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).toBeAriaDisabled();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).toBeAriaDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();

    // Lock has now expired
    mockDate.set('2022-02-16T12:10:00Z');

    await waitFor(() => {
      expect(
        screen.queryByText(/This block was locked for editing/),
      ).not.toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).not.toBeAriaDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).not.toBeAriaDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();
  });

  test('renders unlocked state when current user is idle for too long', async () => {
    mockDate.set('2022-02-16T12:00:00Z');

    jest.useFakeTimers();

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:20:00Z',
          lockedBy: testCurrentUser,
        }}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();

    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save & close' }),
    ).toBeInTheDocument();

    expect(connectionMock.invoke).not.toHaveBeenCalled();

    // User has been idle and lock is removed
    jest.advanceTimersByTime(600_000);

    mockDate.set('2022-02-16T12:10:00Z');

    await waitFor(() => {
      expect(connectionMock.invoke).toHaveBeenCalledTimes(1);
      expect(connectionMock.invoke).toHaveBeenLastCalledWith<
        Parameters<typeof connectionMock.invoke>
      >('UnlockContentBlock', { id: testHtmlBlock.id });
    });

    await waitFor(() => {
      expect(screen.getByText('Edit block')).toBeInTheDocument();
    });

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();

    jest.useRealTimers();
  });

  test('re-renders locked state when other user renews their lock', async () => {
    mockDate.set('2022-02-16T12:00:00Z');

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    let onContentBlockLocked: (
      event: ReleaseContentBlockLockEvent,
    ) => void = noop;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ContentBlockLocked') {
        onContentBlockLocked = callback;
      }
    });

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        }}
      />,
    );

    expect(
      screen.getByText(
        'Rob Rowe (rob@test.com) is currently editing this block (last updated 12:00)',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).toBeAriaDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeAriaDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();

    // Lock is about to expire
    mockDate.set('2022-02-16T12:08:00Z');

    // Simulates lock being renewed
    onContentBlockLocked({
      id: 'block-1',
      releaseId: 'release-1',
      sectionId: 'section-1',
      locked: '2022-02-16T12:08:00Z',
      lockedUntil: '2022-02-16T12:18:00Z',
      lockedBy: testOtherUser,
    });

    await waitFor(() => {
      expect(
        screen.getByText(
          'Rob Rowe (rob@test.com) is currently editing this block (last updated 12:08)',
        ),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).toBeAriaDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeAriaDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();
  });

  test('re-renders locked state when current user interacts with block before expiry', async () => {
    // Lock is about to expire
    mockDate.set('2022-02-16T12:09:00Z');

    jest.useFakeTimers();

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    connectionMock.invoke.mockImplementation(methodName => {
      if (methodName === 'LockContentBlock') {
        return Promise.resolve<ReleaseContentBlockLockEvent>({
          id: 'block-1',
          releaseId: 'release-1',
          sectionId: 'section-1',
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:20:00Z',
          lockedBy: testCurrentUser,
        });
      }

      return Promise.resolve();
    });

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={{
          ...testHtmlBlock,
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        }}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();

    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save & close' }),
    ).toBeInTheDocument();

    expect(connectionMock.invoke).not.toHaveBeenCalled();

    // Interact with textbox
    await userEvent.type(screen.getByRole('textbox'), 'Test text');

    jest.advanceTimersByTime(60_000);

    await waitFor(() => {
      expect(connectionMock.invoke).toHaveBeenCalledTimes(1);
      expect(connectionMock.invoke).toHaveBeenLastCalledWith<
        Parameters<typeof connectionMock.invoke>
      >('LockContentBlock', { id: testHtmlBlock.id });
    });

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();

    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save & close' }),
    ).toBeInTheDocument();

    jest.useRealTimers();
  });

  test('renders locked state when other user starts editing', async () => {
    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    let onContentBlockLocked: (
      event: ReleaseContentBlockLockEvent,
    ) => void = noop;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ContentBlockLocked') {
        onContentBlockLocked = callback;
      }
    });

    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={testHtmlBlock}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();

    mockDate.set('2022-02-16T12:00:00Z');

    // Simulates locking of block by other user
    onContentBlockLocked({
      id: 'block-1',
      releaseId: 'release-1',
      sectionId: 'section-1',
      locked: '2022-02-16T12:00:00Z',
      lockedUntil: '2022-02-16T12:10:00Z',
      lockedBy: testOtherUser,
    });

    await waitFor(() => {
      expect(
        screen.getByText(
          'Rob Rowe (rob@test.com) is currently editing this block (last updated 12:00)',
        ),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).toBeAriaDisabled();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeAriaDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();
  });

  test('clicking `Remove block` button shows confirmation modal', () => {
    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={testHtmlBlock}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByText(/Are you sure you want to remove this block/),
    ).toBeInTheDocument();
  });

  test('confirming removal of block calls service to delete block', () => {
    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={testHtmlBlock}
      />,
    );

    expect(
      releaseContentService.deleteContentSectionBlock,
    ).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    expect(
      releaseContentService.deleteContentSectionBlock,
    ).toHaveBeenCalledTimes(1);
    expect(
      releaseContentService.deleteContentSectionBlock,
    ).toHaveBeenCalledWith<
      Parameters<typeof releaseContentService.deleteContentSectionBlock>
    >('release-1', 'section-1', 'block-1');
  });

  test('renders Embed block', () => {
    render(
      <ReleaseEditableBlock
        publicationId="publication-1"
        releaseId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={testEmbedBlock}
      />,
    );

    expect(screen.getByTitle('Dashboard title')).toHaveAttribute(
      'src',
      'https://department-for-education.shinyapps.io/test-dashboard',
    );
  });

  function render(
    child: ReactNode,
    release: EditableRelease = testEditableRelease,
  ): RenderResult {
    return baseRender(
      <AuthContextTestProvider
        user={{
          id: testCurrentUser.id,
          name: testCurrentUser.displayName,
          validToken: true,
          permissions: {} as GlobalPermissions,
        }}
      >
        <ReleaseContentHubContextProvider releaseId={release.id}>
          <ReleaseContentProvider
            value={{
              release,
              canUpdateRelease: true,
              unattachedDataBlocks: [],
            }}
          >
            {child}
          </ReleaseContentProvider>
        </ReleaseContentHubContextProvider>
      </AuthContextTestProvider>,
    );
  }
});
