import {
  FilterOptionCandidateWithKey,
  FilterOptionMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import styles from '@admin/pages/release/data/components/ApiDataSetMappingModal.module.scss';
import ApiDataSetMappingForm from '@admin/pages/release/data/components/ApiDataSetMappingForm';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import {
  FilterOptionSource,
  LocationCandidate,
} from '@admin/services/apiDataSetVersionService';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode } from 'react';

interface Props {
  candidate?: FilterOptionCandidateWithKey | LocationCandidateWithKey;
  candidateHint?: (
    candidate: FilterOptionCandidateWithKey | LocationCandidateWithKey,
  ) => ReactNode;
  groupKey: string;
  itemLabel: string;
  mapping: FilterOptionMappingWithKey | LocationMappingWithKey;
  newItems: FilterOptionCandidateWithKey[] | LocationCandidateWithKey[];
  renderSourceDetails?: (
    source: FilterOptionSource | LocationCandidate,
  ) => ReactNode;
  onSubmit: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappingModal({
  candidate,
  candidateHint,
  groupKey,
  itemLabel,
  mapping,
  newItems,
  renderSourceDetails,
  onSubmit,
}: Props) {
  const [isOpen, toggleOpen] = useToggle(false);

  return (
    <Modal
      className={styles.modal}
      open={isOpen}
      title={`Map existing ${itemLabel}`}
      triggerButton={
        <ButtonText className="govuk-!-margin-left-2" onClick={toggleOpen.on}>
          Map option
          <VisuallyHidden> for {mapping.source.label}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>{`Current data set ${itemLabel}`}</h3>
      <SummaryList>
        <SummaryListItem term="Label">{mapping.source.label}</SummaryListItem>
        {renderSourceDetails?.(mapping.source)}

        <SummaryListItem term="Identifier">{mapping.publicId}</SummaryListItem>
      </SummaryList>
      <ApiDataSetMappingForm
        candidate={candidate}
        candidateHint={candidateHint}
        groupKey={groupKey}
        itemLabel={itemLabel}
        mapping={mapping}
        newItems={newItems}
        onCancel={toggleOpen.off}
        onSubmit={async update => {
          await onSubmit(update);
          toggleOpen.off();
        }}
      />
    </Modal>
  );
}
