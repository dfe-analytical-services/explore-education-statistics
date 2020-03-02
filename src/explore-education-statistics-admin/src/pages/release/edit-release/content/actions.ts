import { EditableContentBlock } from '@admin/services/publicationService';
import { AbstractRelease } from '@common/services/publicationService';
import { State } from './ReleaseContext';

type PageError = { type: 'PAGE_ERROR'; payload: { pageError: string } };
type ClearState = { type: 'CLEAR_STATE' };
type SetNewState = { type: 'SET_STATE'; payload: State };
type SetAvailableDatablocks = {
  type: 'SET_AVAILABLE_DATABLOCKS';
  payload: Pick<State, 'availableDataBlocks'>;
};
export type RemoveBlockFromSection = {
  type: 'REMOVE_BLOCK_FROM_SECTION';
  payload: {
    meta: {
      sectionId: string;
      blockId: string;
      sectionKey: keyof Pick<
        AbstractRelease<EditableContentBlock>,
        | 'summarySection'
        | 'keyStatisticsSection'
        | 'keyStatisticsSecondarySection'
        | 'headlinesSection'
        | 'content'
      >;
    };
  };
};
export type UpdateBlockFromSection = {
  type: 'UPDATE_BLOCK_FROM_SECTION';
  payload: {
    block: EditableContentBlock;
    meta: RemoveBlockFromSection['payload']['meta'];
  };
};

type ReleaseDispatchAction =
  | PageError
  | ClearState
  | SetNewState
  | SetAvailableDatablocks
  | RemoveBlockFromSection
  | UpdateBlockFromSection;

// eslint-disable-next-line no-undef
export default ReleaseDispatchAction;
