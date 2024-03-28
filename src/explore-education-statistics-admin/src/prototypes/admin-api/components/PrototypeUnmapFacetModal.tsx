import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import React from 'react';
import { MapItem } from '../PrototypePrepareNextSubjectPage';
import styles from './PrototypeMapFacetModal.module.scss';

interface Props {
  itemToUnmap: [MapItem, MapItem];
  name: string;
  onClose: () => void;
  onSubmit: () => void;
}

const PrototypeUnmapFacetModal = ({
  itemToUnmap,
  name,
  onClose,
  onSubmit,
}: Props) => {
  return (
    <Modal
      className="govuk-!-width-one-half"
      open
      title={`Mapped ${name}`}
      onExit={onClose}
    >
      <div className={styles.inner}>
        <h3>Current data set {name}</h3>

        <SummaryList className="govuk-!-margin-bottom-5">
          <SummaryListItem term="Label">{itemToUnmap[0].label}</SummaryListItem>
          {itemToUnmap[0].group && (
            <SummaryListItem term="Group">
              {itemToUnmap[0].group}
            </SummaryListItem>
          )}
          {itemToUnmap[0].filter && (
            <SummaryListItem term="Filter">
              {itemToUnmap[0].filter}
            </SummaryListItem>
          )}
          {itemToUnmap[0].code && (
            <SummaryListItem term="Code">{itemToUnmap[0].code}</SummaryListItem>
          )}
          {itemToUnmap[0].level && (
            <SummaryListItem term="Level">
              {itemToUnmap[0].level}
            </SummaryListItem>
          )}
          {itemToUnmap[0].region && (
            <SummaryListItem term="Region">
              {itemToUnmap[0].region}
            </SummaryListItem>
          )}
          <SummaryListItem term="Identifier">
            {itemToUnmap[0].id}
          </SummaryListItem>
        </SummaryList>

        <h3>Next data set {name}</h3>

        <SummaryList className="govuk-!-margin-bottom-5">
          <SummaryListItem term="Label">{itemToUnmap[1].label}</SummaryListItem>
          {itemToUnmap[1].group && (
            <SummaryListItem term="Group">
              {itemToUnmap[1].group}
            </SummaryListItem>
          )}
          {itemToUnmap[1].filter && (
            <SummaryListItem term="Filter">
              {itemToUnmap[1].filter}
            </SummaryListItem>
          )}
          {itemToUnmap[1].code && (
            <SummaryListItem term="Code">{itemToUnmap[1].code}</SummaryListItem>
          )}
          {itemToUnmap[1].level && (
            <SummaryListItem term="Level">
              {itemToUnmap[1].level}
            </SummaryListItem>
          )}
          {itemToUnmap[1].region && (
            <SummaryListItem term="Region">
              {itemToUnmap[1].region}
            </SummaryListItem>
          )}
          <SummaryListItem term="Identifier">
            {itemToUnmap[1].id}
          </SummaryListItem>
        </SummaryList>
      </div>
      <Button
        className="govuk-!-margin-bottom-0 govuk-!-margin-top-4"
        variant="warning"
        type="button"
        onClick={onSubmit}
      >
        Remove mapping
      </Button>
    </Modal>
  );
};

export default PrototypeUnmapFacetModal;
