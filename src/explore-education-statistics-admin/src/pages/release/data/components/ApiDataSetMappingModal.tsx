import {
  FilterCandidateWithKey,
  FilterMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import styles from '@admin/pages/release/data/components/ApiDataSetMappingModal.module.scss';
import ApiDataSetMappingForm from '@admin/pages/release/data/components/ApiDataSetMappingForm';
import {
  LocationCandidate,
  PendingMappingUpdate,
} from '@admin/services/apiDataSetVersionService';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

interface Props {
  candidate?: FilterCandidateWithKey | LocationCandidateWithKey;
  groupKey: string;
  label: string;
  mapping: FilterMappingWithKey | LocationMappingWithKey;
  newItems: FilterCandidateWithKey[] | LocationCandidateWithKey[];
  onSubmit: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappingModal({
  candidate,
  groupKey,
  label,
  mapping,
  newItems,
  onSubmit,
}: Props) {
  const [isOpen, toggleOpen] = useToggle(false);

  return (
    <Modal
      className={styles.modal}
      open={isOpen}
      title={`Map existing ${label}`}
      triggerButton={
        <ButtonText className="govuk-!-margin-left-2" onClick={toggleOpen.on}>
          Edit
          <VisuallyHidden> mapping for {mapping.source.label}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>{`Current data set ${label}`}</h3>
      <SummaryList>
        <SummaryListItem term="Label">{mapping.source.label}</SummaryListItem>
        <LocationDetails location={mapping.source} />

        <SummaryListItem term="Identifier">{mapping.publicId}</SummaryListItem>
      </SummaryList>
      <ApiDataSetMappingForm
        candidate={candidate}
        groupKey={groupKey}
        label={label}
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

function LocationDetails({ location }: { location: LocationCandidate }) {
  const { code, oldCode, urn, laEstab, ukprn } = location;
  return (
    <>
      {code && <SummaryListItem term="Code">{code}</SummaryListItem>}
      {oldCode && <SummaryListItem term="Old code">{oldCode}</SummaryListItem>}
      {urn && <SummaryListItem term="URN">{urn}</SummaryListItem>}
      {laEstab && <SummaryListItem term="LAESTAB">{laEstab}</SummaryListItem>}
      {ukprn && <SummaryListItem term="UKPRN">{ukprn}</SummaryListItem>}
    </>
  );
}
