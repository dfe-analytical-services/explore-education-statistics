import {
  generateEditableContentBlock,
  generateEditableDataBlock,
  generateEditableEmbedBlock,
} from '@admin-test/generators/contentGenerators';
import generateReleaseContent from '@admin-test/generators/releaseContentGenerators';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { AuthContextTestProvider } from '@admin/contexts/AuthContext';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { ReleaseContentHubContextProvider } from '@admin/contexts/ReleaseContentHubContext';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContentBlockLockEvent } from '@admin/services/hubs/releaseContentHub';
import connectionMock from '@admin/services/hubs/utils/__mocks__/connectionMock';
import { GlobalPermissions } from '@admin/services/permissionService';
import _releaseContentCommentService from '@admin/services/releaseContentCommentService';
import _releaseContentService, {
  ReleaseContent as ReleaseContentType,
} from '@admin/services/releaseContentService';
import { UserDetails } from '@admin/services/types/user';
import mockDate from '@common-test/mockDate';
import { HubConnectionState } from '@microsoft/signalr';
import {
  act,
  render as baseRender,
  RenderResult,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import { ReactNode } from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/hubs/utils/createConnection');
jest.mock('@admin/services/releaseContentService');
jest.mock('@admin/services/releaseContentCommentService');
jest.mock('@admin/services/dataBlockService');

const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;
const releaseContentCommentService =
  _releaseContentCommentService as jest.Mocked<
    typeof _releaseContentCommentService
  >;

describe('ReleaseEditableBlock', () => {
  afterEach(() => {
    jest.useRealTimers();
  });

  const testReleaseContent = generateReleaseContent({});

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

  test('renders HTML block', async () => {
    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({})}
      />,
    );

    expect(
      await screen.findByText('Content block body', { selector: 'p' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();
  });

  test('renders HTML block with correct image urls', async () => {
    const image1SrcSet =
      '/api/releases/release-1/images/some-image-id-100 100w, ' +
      '/api/releases/release-1/images/some-image-id-200 200w, ' +
      '/api/releases/release-1/images/some-image-id-300 300w';

    const image2SrcSet =
      'https://test/some-image-url-100.jpg 100w, ' +
      'https://test/some-image-url-200.jpg 200w, ' +
      'https://test/some-image-url-300.jpg 300w';

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          body: `
          <h3>Test heading</h3>
          <p>Some test content</p>
          <img alt="Test image 1" src="/api/releases/release-1/images/some-image-id" srcset="${image1SrcSet}" />
          <img alt="Test image 2" src="https://test/some-image-url.jpg" srcset="${image2SrcSet}" />
          `,
        })}
      />,
    );

    expect(await screen.findByAltText('Test image 1')).toHaveAttribute(
      'src',
      '/api/releases/release-1/images/some-image-id',
    );
    expect(screen.getByAltText('Test image 1')).toHaveAttribute(
      'srcset',
      '/api/releases/release-1/images/some-image-id-100 100w, ' +
        '/api/releases/release-1/images/some-image-id-200 200w, ' +
        '/api/releases/release-1/images/some-image-id-300 300w',
    );

    expect(screen.getByAltText('Test image 2')).toHaveAttribute(
      'src',
      'https://test/some-image-url.jpg',
    );
    expect(screen.getByAltText('Test image 2')).toHaveAttribute(
      'srcset',
      image2SrcSet,
    );
  });

  test('renders editing state for current user', async () => {
    mockDate.set('2022-02-16T12:05:00Z');

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        })}
      />,
    );

    expect(await screen.findByRole('textbox')).toBeInTheDocument();

    expect(
      await screen.findByRole('button', { name: 'Save & close' }),
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
          id: 'content-block-id',
          releaseVersionId: 'release-1',
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
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({})}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Edit block' }));

    await waitFor(() => {
      expect(screen.getByText('Save & close')).toBeInTheDocument();
    });

    expect(screen.getByRole('textbox')).toBeInTheDocument();

    expect(
      await screen.findByRole('button', { name: 'Save & close' }),
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
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        })}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(screen.getByText('Save & close')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Save & close' }));

    await waitFor(() => {
      expect(screen.getByText('Edit block')).toBeInTheDocument();
    });

    expect(
      await screen.findByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      await screen.findByRole('button', { name: 'Remove block' }),
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
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testCurrentUser,
        })}
      />,
    );

    expect(await screen.findByText('Save & close')).toBeInTheDocument();

    await userEvent.type(screen.getByRole('textbox'), 'test');

    await userEvent.click(screen.getByRole('button', { name: 'Save & close' }));

    await waitFor(() => {
      expect(screen.getByText('Edit block')).toBeInTheDocument();
    });

    expect(
      await screen.findByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      await screen.findByRole('button', { name: 'Remove block' }),
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
      await screen.findByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      await screen.findByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();
  });

  test('renders locked state when already locked by other user', async () => {
    mockDate.set('2022-02-16T12:09:00Z');

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        })}
      />,
    );

    expect(
      await screen.findByText(
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

  test("renders unlocked state when other user's lock has already expired", async () => {
    // Lock has already expired
    mockDate.set('2022-02-16T12:11:00Z');

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        })}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      await screen.findByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      await screen.findByRole('button', { name: 'Remove block' }),
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
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        })}
      />,
    );

    expect(
      await screen.findByText(
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

      expect(
        screen.getByRole('button', { name: 'Edit block' }),
      ).not.toBeAriaDisabled();
    });

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

    jest.useFakeTimers({
      doNotFake: ['Date'],
    });
    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:20:00Z',
          lockedBy: testCurrentUser,
        })}
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
      await screen.findByRole('button', { name: 'Save & close' }),
    ).toBeInTheDocument();

    expect(connectionMock.invoke).not.toHaveBeenCalled();

    // User has been idle and lock is removed
    jest.advanceTimersByTime(600_000);

    mockDate.set('2022-02-16T12:10:00Z');

    await waitFor(() => {
      expect(connectionMock.invoke).toHaveBeenCalledTimes(1);
      expect(connectionMock.invoke).toHaveBeenLastCalledWith<
        Parameters<typeof connectionMock.invoke>
      >('UnlockContentBlock', { id: 'content-block-id' });
    });

    await waitFor(() => {
      expect(screen.getByText('Edit block')).toBeInTheDocument();
    });

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(
      await screen.findByRole('button', { name: 'Edit block' }),
    ).not.toBeDisabled();
    expect(
      await screen.findByRole('button', { name: 'Remove block' }),
    ).not.toBeDisabled();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Save & close' }),
    ).not.toBeInTheDocument();
  });

  test('re-renders locked state when other user renews their lock', async () => {
    mockDate.set('2022-02-16T12:00:00Z');

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    let onContentBlockLocked: (event: ReleaseContentBlockLockEvent) => void =
      noop;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ContentBlockLocked') {
        onContentBlockLocked = callback;
      }
    });

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:10:00Z',
          lockedBy: testOtherUser,
        })}
      />,
    );

    expect(
      await screen.findByText(
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

    await act(async () =>
      // Simulates lock being renewed
      onContentBlockLocked({
        id: 'content-block-id',
        releaseVersionId: 'release-1',
        sectionId: 'section-1',
        locked: '2022-02-16T12:08:00Z',
        lockedUntil: '2022-02-16T12:18:00Z',
        lockedBy: testOtherUser,
      }),
    );

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
    const user = userEvent.setup({ advanceTimers: jest.advanceTimersByTime });

    // Lock is about to expire
    mockDate.set('2022-02-16T12:09:00Z');

    jest.useFakeTimers({
      doNotFake: ['Date'],
    });

    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    connectionMock.invoke.mockImplementation(methodName => {
      if (methodName === 'LockContentBlock') {
        return Promise.resolve<ReleaseContentBlockLockEvent>({
          id: 'content-block-id',
          releaseVersionId: 'release-1',
          sectionId: 'section-1',
          locked: '2022-02-16T12:00:00Z',
          lockedUntil: '2022-02-16T12:20:00Z',
          lockedBy: testCurrentUser,
        });
      }

      return Promise.resolve();
    });

    await act(async () =>
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableContentBlock({
            locked: '2022-02-16T12:00:00Z',
            lockedUntil: '2022-02-16T12:10:00Z',
            lockedBy: testCurrentUser,
          })}
        />,
      ),
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
    await user.type(screen.getByRole('textbox'), 'Test text');

    await act(async () => {
      jest.advanceTimersByTime(60_000);
    });

    await waitFor(() => {
      expect(connectionMock.invoke).toHaveBeenCalledTimes(1);
      expect(connectionMock.invoke).toHaveBeenLastCalledWith<
        Parameters<typeof connectionMock.invoke>
      >('LockContentBlock', { id: 'content-block-id' });
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
  });

  test('renders locked state when other user starts editing', async () => {
    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);

    let onContentBlockLocked: (event: ReleaseContentBlockLockEvent) => void =
      noop;

    connectionMock.on.mockImplementation((methodName, callback) => {
      if (methodName === 'ContentBlockLocked') {
        onContentBlockLocked = callback;
      }
    });

    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({})}
      />,
    );

    expect(
      screen.queryByText(/This block was locked for editing/),
    ).not.toBeInTheDocument();

    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();

    mockDate.set('2022-02-16T12:00:00Z');

    await act(async () => {
      // Simulates locking of block by other user
      onContentBlockLocked({
        id: 'content-block-id',
        releaseVersionId: 'release-1',
        sectionId: 'section-1',
        locked: '2022-02-16T12:00:00Z',
        lockedUntil: '2022-02-16T12:10:00Z',
        lockedBy: testOtherUser,
      });
    });

    expect(
      await screen.findByText(
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

  test('clicking `Remove block` button shows confirmation modal', async () => {
    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({})}
      />,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByText(/Are you sure you want to remove this block/),
    ).toBeInTheDocument();
  });

  test('confirming removal of block calls service to delete block', async () => {
    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableContentBlock({})}
      />,
    );

    expect(
      releaseContentService.deleteContentSectionBlock,
    ).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    const modal = within(screen.getByRole('dialog'));

    await userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    expect(
      releaseContentService.deleteContentSectionBlock,
    ).toHaveBeenCalledTimes(1);
    expect(
      releaseContentService.deleteContentSectionBlock,
    ).toHaveBeenCalledWith<
      Parameters<typeof releaseContentService.deleteContentSectionBlock>
    >('release-1', 'section-1', 'content-block-id');
  });

  test('renders Embed block', async () => {
    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableEmbedBlock({})}
      />,
    );

    expect(await screen.findByTitle('Embed block title')).toHaveAttribute(
      'src',
      'https://department-for-education.shinyapps.io/test-dashboard',
    );
  });

  test('renders data block', async () => {
    render(
      <ReleaseEditableBlock
        label="Test block"
        publicationId="publication-1"
        releaseVersionId="release-1"
        sectionId="section-1"
        sectionKey="content"
        block={generateEditableDataBlock({})}
      />,
    );

    expect(await screen.findByText('Edit data block')).toBeInTheDocument();
  });

  describe('data block comments', () => {
    test('renders comments correctly', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableDataBlock({ comments: testComments })}
        />,
      );

      expect(
        await screen.findByRole('button', { name: 'Add comment' }),
      ).toBeInTheDocument();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      expect(unresolvedComments).toHaveLength(3);
      expect(unresolvedComments[0]).toHaveTextContent('Comment 2 content');
      expect(unresolvedComments[1]).toHaveTextContent('Comment 3 content');
      expect(unresolvedComments[2]).toHaveTextContent('Comment 4 content');

      const resolvedComments = within(
        screen.getByTestId('comments-resolved'),
      ).getAllByTestId('comment');

      expect(resolvedComments).toHaveLength(2);
      expect(resolvedComments[0]).toHaveTextContent('Comment 1 content');
      expect(resolvedComments[1]).toHaveTextContent('Comment 5 content');
    });

    test('calls `deleteContentSectionComment` when a comment delete button is clicked', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableDataBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.deleteContentSectionComment,
      ).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      await userEvent.click(
        within(unresolvedComments[0]).getByRole('button', {
          name: 'Delete',
        }),
      );

      await waitFor(() => {
        expect(
          releaseContentCommentService.deleteContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.deleteContentSectionComment,
        ).toHaveBeenCalledWith(testComments[1].id);
      });
    });

    test('calls `updateContentSectionComment` when a comment is edited', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableDataBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.updateContentSectionComment,
      ).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      const comment = within(unresolvedComments[0]);

      await userEvent.click(comment.getByRole('button', { name: 'Edit' }));
      await userEvent.clear(comment.getByRole('textbox'));
      await userEvent.type(
        comment.getByRole('textbox'),
        'Test updated content',
      );

      await userEvent.click(comment.getByRole('button', { name: 'Update' }));

      await waitFor(() => {
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledWith({
          ...testComments[1],
          content: 'Test updated content',
        });
      });
    });

    test('calls `updateContentSectionComment` when a comment is resolved', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableDataBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.updateContentSectionComment,
      ).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      const comment = within(unresolvedComments[0]);

      await userEvent.click(comment.getByRole('button', { name: 'Resolve' }));

      await waitFor(() => {
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledWith({
          ...testComments[1],
          setResolved: true,
        });
      });
    });

    test('calls `updateContentSectionComment` when a comment is unresolved', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableDataBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.updateContentSectionComment,
      ).not.toHaveBeenCalled();

      const resolvedComments = within(
        screen.getByTestId('comments-resolved'),
      ).getAllByTestId('comment');

      const comment = within(resolvedComments[0]);

      await userEvent.click(
        comment.getByRole('button', { name: 'Unresolve', hidden: true }),
      );

      await waitFor(() => {
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledWith({
          ...testComments[0],
          setResolved: false,
        });
      });
    });

    test('calls `addContentSectionComment` when a comment is added', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableDataBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.addContentSectionComment,
      ).not.toHaveBeenCalled();

      await userEvent.click(
        screen.getByRole('button', { name: 'Add comment' }),
      );

      await userEvent.type(
        screen.getByRole('textbox', {
          name: 'Comment',
        }),
        'I am a comment',
      );

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Add comment',
        }),
      );

      await waitFor(() => {
        expect(
          releaseContentCommentService.addContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.addContentSectionComment,
        ).toHaveBeenCalledWith('release-1', 'section-1', 'data-block-id', {
          content: 'I am a comment',
        });
      });
    });
  });

  describe('embed block comments', () => {
    test('renders comments correctly', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableEmbedBlock({ comments: testComments })}
        />,
      );

      expect(
        await screen.findByRole('button', { name: 'Add comment' }),
      ).toBeInTheDocument();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      expect(unresolvedComments).toHaveLength(3);
      expect(unresolvedComments[0]).toHaveTextContent('Comment 2 content');
      expect(unresolvedComments[1]).toHaveTextContent('Comment 3 content');
      expect(unresolvedComments[2]).toHaveTextContent('Comment 4 content');

      const resolvedComments = within(
        screen.getByTestId('comments-resolved'),
      ).getAllByTestId('comment');

      expect(resolvedComments).toHaveLength(2);
      expect(resolvedComments[0]).toHaveTextContent('Comment 1 content');
      expect(resolvedComments[1]).toHaveTextContent('Comment 5 content');
    });

    test('calls `deleteContentSectionComment` when a comment delete button is clicked', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableEmbedBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.deleteContentSectionComment,
      ).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      await userEvent.click(
        within(unresolvedComments[0]).getByRole('button', {
          name: 'Delete',
        }),
      );

      await waitFor(() => {
        expect(
          releaseContentCommentService.deleteContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.deleteContentSectionComment,
        ).toHaveBeenCalledWith(testComments[1].id);
      });
    });

    test('calls `updateContentSectionComment` when a comment is edited', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableEmbedBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.updateContentSectionComment,
      ).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      const comment = within(unresolvedComments[0]);

      await userEvent.click(comment.getByRole('button', { name: 'Edit' }));
      await userEvent.clear(comment.getByRole('textbox'));
      await userEvent.type(
        comment.getByRole('textbox'),
        'Test updated content',
      );

      await userEvent.click(comment.getByRole('button', { name: 'Update' }));

      await waitFor(() => {
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledWith({
          ...testComments[1],
          content: 'Test updated content',
        });
      });
    });

    test('calls `updateContentSectionComment` when a comment is resolved', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableEmbedBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.updateContentSectionComment,
      ).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('comments-unresolved'),
      ).getAllByTestId('comment');

      const comment = within(unresolvedComments[0]);

      await userEvent.click(comment.getByRole('button', { name: 'Resolve' }));

      await waitFor(() => {
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledWith({
          ...testComments[1],
          setResolved: true,
        });
      });
    });

    test('calls `updateContentSectionComment` when a comment is unresolved', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableEmbedBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.updateContentSectionComment,
      ).not.toHaveBeenCalled();

      const resolvedComments = within(
        screen.getByTestId('comments-resolved'),
      ).getAllByTestId('comment');

      const comment = within(resolvedComments[0]);

      await userEvent.click(
        comment.getByRole('button', { name: 'Unresolve', hidden: true }),
      );

      await waitFor(() => {
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.updateContentSectionComment,
        ).toHaveBeenCalledWith({
          ...testComments[0],
          setResolved: false,
        });
      });
    });

    test('calls `addContentSectionComment` when a comment is added', async () => {
      render(
        <ReleaseEditableBlock
          label="Test block"
          publicationId="publication-1"
          releaseVersionId="release-1"
          sectionId="section-1"
          sectionKey="content"
          block={generateEditableEmbedBlock({ comments: testComments })}
        />,
      );

      expect(
        releaseContentCommentService.addContentSectionComment,
      ).not.toHaveBeenCalled();

      await userEvent.click(
        screen.getByRole('button', { name: 'Add comment' }),
      );

      await userEvent.type(
        screen.getByRole('textbox', {
          name: 'Comment',
        }),
        'I am a comment',
      );

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Add comment',
        }),
      );

      await waitFor(() => {
        expect(
          releaseContentCommentService.addContentSectionComment,
        ).toHaveBeenCalledTimes(1);
        expect(
          releaseContentCommentService.addContentSectionComment,
        ).toHaveBeenCalledWith('release-1', 'section-1', 'embed-block-id', {
          content: 'I am a comment',
        });
      });
    });
  });

  function render(
    child: ReactNode,
    releaseContent: ReleaseContentType = testReleaseContent,
  ): RenderResult {
    return baseRender(
      <TestConfigContextProvider>
        <AuthContextTestProvider
          user={{
            id: testCurrentUser.id,
            name: testCurrentUser.displayName,
            permissions: {} as GlobalPermissions,
          }}
        >
          unattachedDataBlocks: [],
          <ReleaseContentHubContextProvider
            releaseVersionId={releaseContent.release.id}
          >
            <ReleaseContentProvider
              value={{
                ...releaseContent,
                canUpdateRelease: true,
              }}
            >
              <MemoryRouter>{child}</MemoryRouter>
            </ReleaseContentProvider>
          </ReleaseContentHubContextProvider>
        </AuthContextTestProvider>
      </TestConfigContextProvider>,
    );
  }
});
