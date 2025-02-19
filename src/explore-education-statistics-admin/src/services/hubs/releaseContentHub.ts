import createConnection from '@admin/services/hubs/utils/createConnection';
import { EditableBlock } from '@admin/services/types/content';
import { UserDetails } from '@admin/services/types/user';
import Hub, { Subscription } from './utils/Hub';

export interface ReleaseContentBlockLockEvent {
  id: string;
  sectionId: string;
  releaseVersionId: string;
  locked: string;
  lockedUntil: string;
  lockedBy: UserDetails;
}

export interface ReleaseContentBlockUnlockEvent {
  id: string;
  sectionId: string;
  releaseVersionId: string;
}

export class ReleaseContentHub extends Hub {
  async joinReleaseGroup(releaseVersionId: string): Promise<void> {
    await this.send('JoinReleaseGroup', { id: releaseVersionId });
  }

  async leaveReleaseGroup(releaseVersionId: string): Promise<void> {
    await this.send('LeaveReleaseGroup', { id: releaseVersionId });
  }

  lockContentBlock(
    contentBlockId: string,
    force?: boolean,
  ): Promise<ReleaseContentBlockLockEvent> {
    return this.invoke('LockContentBlock', {
      id: contentBlockId,
      force,
    });
  }

  unlockContentBlock(contentBlockId: string, force?: boolean): Promise<void> {
    return this.invoke('UnlockContentBlock', {
      id: contentBlockId,
      force,
    });
  }

  onContentBlockLocked(
    callback: (event: ReleaseContentBlockLockEvent) => void,
  ): Subscription {
    return this.subscribe('ContentBlockLocked', callback);
  }

  onContentBlockUnlocked(
    callback: (event: ReleaseContentBlockUnlockEvent) => void,
  ): Subscription {
    return this.subscribe('ContentBlockUnlocked', callback);
  }

  onContentBlockUpdated(
    callback: (block: EditableBlock) => void,
  ): Subscription {
    return this.subscribe('ContentBlockUpdated', callback);
  }
}

export default function releaseContentHub() {
  return new ReleaseContentHub(createConnection('/hubs/release-content'));
}
