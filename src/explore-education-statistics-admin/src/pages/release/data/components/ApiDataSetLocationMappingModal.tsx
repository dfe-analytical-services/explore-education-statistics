import {
  LocationMappingWithKey,
  LocationCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { PendingLocationMappingUpdate } from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import ApiDataSetLocationMappingForm from '@admin/pages/release/data/components/ApiDataSetLocationMappingForm';
import styles from '@admin/pages/release/data/components/ApiDataSetLocationMappingModal.module.scss';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import React from 'react';

interface Props {
  candidate?: LocationCandidateWithKey;
  level: LocationLevelKey;
  mapping: LocationMappingWithKey;
  newLocations: LocationCandidateWithKey[];
  onSubmit: (update: PendingLocationMappingUpdate) => Promise<void>;
}

export default function ApiDataSetLocationMappingModal({
  candidate,
  level,
  mapping,
  newLocations = [],
  onSubmit,
}: Props) {
  const [isOpen, toggleOpen] = useToggle(false);

  return (
    <Modal
      className={styles.modal}
      open={isOpen}
      title="Map existing location"
      triggerButton={
        <ButtonText className="govuk-!-margin-left-2" onClick={toggleOpen.on}>
          Edit
          <VisuallyHidden> mapping for {mapping.source.label}</VisuallyHidden>
        </ButtonText>
      }
    >
      <h3>Current data set location</h3>
      <SummaryList>
        <SummaryListItem term="Label">{mapping.source.label}</SummaryListItem>
        {mapping.source.code && (
          <SummaryListItem term="Code">{mapping.source.code}</SummaryListItem>
        )}
        {mapping.source.oldCode && (
          <SummaryListItem term="Old code">
            {mapping.source.oldCode}
          </SummaryListItem>
        )}
        {mapping.source.urn && (
          <SummaryListItem term="URN">{mapping.source.urn}</SummaryListItem>
        )}
        {mapping.source.laEstab && (
          <SummaryListItem term="LAESTAB">
            {mapping.source.laEstab}
          </SummaryListItem>
        )}
        {mapping.source.ukprn && (
          <SummaryListItem term="UKPRN">{mapping.source.ukprn}</SummaryListItem>
        )}
        <SummaryListItem term="Identifier">{mapping.publicId}</SummaryListItem>
      </SummaryList>

      <ApiDataSetLocationMappingForm
        candidate={candidate}
        level={level}
        mapping={mapping}
        newLocations={newLocations}
        onCancel={toggleOpen.off}
        onSubmit={async update => {
          await onSubmit(update);
          toggleOpen.off();
        }}
      />
    </Modal>
  );
}
