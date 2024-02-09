import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Modal, { ModalCloseButton } from '@common/components/Modal';
import styles from '@frontend/components/FiltersMobile.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  title: string;
  totalResults?: number;
}

export default function FiltersMobile({
  children,
  title,
  totalResults,
}: Props) {
  const totalResultsString = `${totalResults} ${
    totalResults !== 1 ? 'results' : 'result'
  }`;

  return (
    <Modal
      fullScreen
      hideTitle
      title={title}
      triggerButton={<Button>Filter results</Button>}
    >
      <div className={styles.container}>
        <div className={styles.header}>
          <p className="govuk-heading-l govuk-!-margin-bottom-0">
            {totalResultsString}
          </p>
          <ModalCloseButton>
            <ButtonText>Back to results</ButtonText>
          </ModalCloseButton>
        </div>
        {children}
        <ModalCloseButton>
          <Button>{`Show ${totalResultsString}`}</Button>
        </ModalCloseButton>
      </div>
    </Modal>
  );
}
