import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import React from 'react';
import capitalize from 'lodash/capitalize';
import { MapItem } from '../PrototypePrepareNextSubjectPage';
import styles from './PrototypeMapFacetModal.module.scss';

interface Props {
  item: MapItem;
  name: string;
  onClose: () => void;
  onSubmit: () => void;
}

const PrototypeRemoveNoMappingFacetModal = ({
  item,
  name,
  onClose,
  onSubmit,
}: Props) => {
  return (
    <Modal
      className="govuk-!-width-one-half"
      open
      title={`${capitalize(name)} with no mapping available`}
      onExit={onClose}
    >
      <div className={styles.inner}>
        <h3>Current data set {name}</h3>

        <SummaryList className="govuk-!-margin-bottom-5">
          <SummaryListItem term="Label">{item.label}</SummaryListItem>
          {item.group && (
            <SummaryListItem term="Group">{item.group}</SummaryListItem>
          )}
          {item.filter && (
            <SummaryListItem term="Filter">{item.filter}</SummaryListItem>
          )}
          {item.code && (
            <SummaryListItem term="Code">{item.code}</SummaryListItem>
          )}
          {item.level && (
            <SummaryListItem term="Level">{item.level}</SummaryListItem>
          )}
          {item.region && (
            <SummaryListItem term="Region">{item.region}</SummaryListItem>
          )}
          <SummaryListItem term="Identifier">{item.id}</SummaryListItem>
        </SummaryList>

        <h3>Next data set {name}</h3>

        <p>No mapping available.</p>
      </div>
      <Button
        className="govuk-!-margin-bottom-0 govuk-!-margin-top-4"
        type="button"
        onClick={onSubmit}
      >
        Reset {name}
      </Button>
    </Modal>
  );
};

export default PrototypeRemoveNoMappingFacetModal;
