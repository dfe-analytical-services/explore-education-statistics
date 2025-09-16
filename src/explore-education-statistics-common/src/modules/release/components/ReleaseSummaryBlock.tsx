import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import styles from '@common/modules/release/components/ReleaseSummaryBlock.module.scss';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import { parseISO } from 'date-fns';
import React, { ReactNode } from 'react';

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const releaseTypesToIcons: Partial<Record<ReleaseType, ReleaseTypeIcon>> = {
  AccreditedOfficialStatistics: {
    url: '/assets/images/accredited-official-statistics-logo.svg',
    altText: 'UK statistics authority quality mark',
  },
};

interface ListItemProps {
  children: ReactNode;
  term: string;
  testId?: string;
}

const ListItem = ({ children, term, testId = term }: ListItemProps) => {
  return (
    <div className={styles.listItem} data-testid={testId}>
      <dt className={styles.listItemTerm} data-testid={`${testId}-key`}>
        {term}
      </dt>

      <dd className={styles.listItemDefinition} data-testid={`${testId}-value`}>
        {children}
      </dd>
    </div>
  );
};

interface Props {
  isEditing?: boolean;
  lastUpdated?: string;
  releaseDate?: string;
  releaseType: ReleaseType;
  renderProducerLink: ReactNode;
  renderUpdatesLink?: ReactNode;
  trackScroll?: boolean;
  onShowReleaseTypeModal?: () => void;
}

export default function ReleaseSummaryBlock({
  isEditing,
  lastUpdated,
  releaseDate,
  releaseType,
  renderProducerLink,
  renderUpdatesLink,
  trackScroll = false,
  onShowReleaseTypeModal,
}: Props) {
  return (
    <div
      data-scroll={trackScroll ? true : undefined}
      id="summary-section"
      className={styles.container}
      data-testid="release-summary-block"
    >
      <div className={styles.innerWrap}>
        <dl className={styles.list}>
          <ListItem term="Release type">
            <Modal
              showClose
              title={releaseTypes[releaseType]}
              triggerButton={
                <ButtonText
                  onClick={() => {
                    onShowReleaseTypeModal?.();
                  }}
                  underline={false}
                >
                  {releaseTypes[releaseType]}{' '}
                  <InfoIcon
                    description={`Information on ${releaseTypes[releaseType]}`}
                  />
                </ButtonText>
              }
            >
              <ReleaseTypeSection showHeading={false} type={releaseType} />
            </Modal>
          </ListItem>

          <ListItem term="Produced by">{renderProducerLink}</ListItem>
          <ListItem term="Published">
            {releaseDate ? (
              <FormattedDate>{parseISO(releaseDate)}</FormattedDate>
            ) : (
              <p>TBA</p>
            )}
          </ListItem>

          {(isEditing || lastUpdated) && (
            <ListItem term="Last updated">
              {lastUpdated && <FormattedDate>{lastUpdated}</FormattedDate>}
            </ListItem>
          )}
        </dl>

        {renderUpdatesLink && renderUpdatesLink}
      </div>
      {releaseTypesToIcons[releaseType] && (
        <img
          src={releaseTypesToIcons[releaseType].url}
          alt={releaseTypesToIcons[releaseType].altText}
          height="76"
          width="60"
        />
      )}
    </div>
  );
}
