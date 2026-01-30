import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import { releaseTypesToIcons } from '@common/modules/release/components/ReleaseSummaryBlock';
import styles from '@common/modules/release/components/ReleaseSummaryBlockMobile.module.scss';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import { Organisation } from '@common/services/types/organisation';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import React, { ReactNode } from 'react';

interface Props {
  isEditing?: boolean;
  lastUpdated?: string | Date;
  publishingOrganisations?: Organisation[];
  releaseDate?: string;
  releaseType: ReleaseType;
  renderProducerLink: ReactNode;
  renderSubscribeLink?: ReactNode;
  renderUpdatesLink?: ReactNode;
  onShowReleaseTypeModal?: () => void;
}

export default function ReleaseSummaryBlockMobile({
  isEditing,
  lastUpdated,
  publishingOrganisations,
  releaseDate,
  releaseType,
  renderProducerLink,
  renderSubscribeLink,
  renderUpdatesLink,
  onShowReleaseTypeModal,
}: Props) {
  return (
    <div
      id="summary-section-mobile"
      className={styles.container}
      data-testid="release-summary-block-mobile"
    >
      <div className={styles.innerWrap}>
        <div className={styles.innerContent}>
          <Modal
            showClose
            title={releaseTypes[releaseType]}
            triggerButton={
              <ButtonText
                onClick={() => {
                  onShowReleaseTypeModal?.();
                }}
                underline={false}
                className={styles.modalTriggerButton}
              >
                {releaseTypes[releaseType]}{' '}
                <InfoIcon
                  description={`Information on ${releaseTypes[releaseType]}`}
                />
              </ButtonText>
            }
          >
            <ReleaseTypeSection
              publishingOrganisations={publishingOrganisations}
              showHeading={false}
              type={releaseType}
            />
          </Modal>

          <div className={styles.updatesContainer}>
            {isEditing || lastUpdated ? (
              <p>
                <span className="dfe-colour--dark-grey">Last updated</span>{' '}
                {isEditing && !lastUpdated && 'Currently editing'}
                {lastUpdated && <FormattedDate>{lastUpdated}</FormattedDate>}
              </p>
            ) : (
              <p>
                <span className="dfe-colour--dark-grey">Published</span>{' '}
                {releaseDate ? (
                  <FormattedDate>{releaseDate}</FormattedDate>
                ) : (
                  <span>TBA</span>
                )}
              </p>
            )}

            {renderUpdatesLink && renderUpdatesLink}
          </div>

          <p>
            <span className="dfe-colour--dark-grey">Produced by</span>{' '}
            {renderProducerLink}
          </p>
        </div>
        {releaseTypesToIcons[releaseType] && (
          <img
            src={releaseTypesToIcons[releaseType].url}
            alt={releaseTypesToIcons[releaseType].altText}
            height="62"
            width="47"
          />
        )}
      </div>

      {renderSubscribeLink && (
        <div className={styles.subscribeContainer}>{renderSubscribeLink}</div>
      )}
    </div>
  );
}
