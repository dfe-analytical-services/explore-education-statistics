import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import styles from '@common/modules/release/components/ReleaseSummaryBlock.module.scss';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import {
  PartialDate,
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import classNames from 'classnames';
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
  children?: ReactNode;
  term: string;
  testId?: string;
  termHidden?: boolean;
}

const ListItem = ({
  children,
  term,
  testId = term,
  termHidden = false,
}: ListItemProps) => {
  return (
    <div className={styles.listItem} data-testid={testId}>
      <dt
        className={classNames(
          styles.listItemTerm,
          termHidden && styles.listItemTermHidden,
        )}
        data-testid={`${testId}-key`}
      >
        {term}
      </dt>

      {typeof children !== 'undefined' && (
        <dd
          className={styles.listItemDefinition}
          data-testid={`${testId}-value`}
        >
          {children}
        </dd>
      )}
    </div>
  );
};

interface Props {
  isEditing?: boolean;
  lastUpdated?: Date;
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
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
  latestRelease,
  nextReleaseDate,
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
    >
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
        {latestRelease && isValidPartialDate(nextReleaseDate) && (
          <ListItem term="Next update">
            <time>{formatPartialDate(nextReleaseDate)}</time>
          </ListItem>
        )}

        {(isEditing || lastUpdated) && (
          <ListItem term="Last updated">
            {lastUpdated && <FormattedDate>{lastUpdated}</FormattedDate>}
          </ListItem>
        )}

        {renderUpdatesLink && (
          <ListItem term="Updates" termHidden>
            {renderUpdatesLink}
          </ListItem>
        )}
      </dl>

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
