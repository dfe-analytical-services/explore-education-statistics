import FormattedDate from '@common/components/FormattedDate';
import {
  PartialDate,
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import Modal from '@common/components/Modal';
import { parseISO } from 'date-fns';
import React, { ReactNode } from 'react';
import classNames from 'classnames';
import { Organisation } from '@common/services/types/organisation';

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

interface Props {
  isEditing?: boolean;
  lastUpdated?: Date;
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
  publishingOrganisations?: Organisation[];
  releaseDate?: string;
  releaseType: ReleaseType;
  renderReleaseNotes: ReactNode;
  renderStatusTags: ReactNode;
  renderSubscribeLink: ReactNode;
  renderProducerLink: ReactNode;
  trackScroll?: boolean;
  onShowReleaseTypeModal?: () => void;
}

export default function ReleaseSummarySection({
  isEditing,
  lastUpdated,
  latestRelease,
  nextReleaseDate,
  publishingOrganisations,
  releaseDate,
  releaseType,
  renderReleaseNotes,
  renderStatusTags,
  renderSubscribeLink,
  renderProducerLink,
  trackScroll = false,
  onShowReleaseTypeModal,
}: Props) {
  return (
    <div data-scroll={trackScroll ? true : undefined} id="summary-section">
      <div
        className={classNames(
          'dfe-flex dfe-align-items--center dfe-justify-content--space-between',
          { 'govuk-!-margin-bottom-3': !releaseTypesToIcons[releaseType] },
        )}
      >
        <div>{renderStatusTags}</div>
        {releaseTypesToIcons[releaseType] && (
          <img
            src={releaseTypesToIcons[releaseType]?.url}
            alt={releaseTypesToIcons[releaseType]?.altText}
            height="76"
            width="60"
          />
        )}
      </div>

      <SummaryList>
        <SummaryListItem term="Published">
          {releaseDate ? (
            <FormattedDate>{parseISO(releaseDate)}</FormattedDate>
          ) : (
            <p>TBA</p>
          )}
        </SummaryListItem>
        {latestRelease && isValidPartialDate(nextReleaseDate) && (
          <SummaryListItem term="Next update">
            <time>{formatPartialDate(nextReleaseDate)}</time>
          </SummaryListItem>
        )}

        {(isEditing || lastUpdated) && (
          <SummaryListItem term="Last updated">
            {lastUpdated && <FormattedDate>{lastUpdated}</FormattedDate>}
            {renderReleaseNotes}
          </SummaryListItem>
        )}

        <SummaryListItem term="Release type">
          <Modal
            showClose
            title={releaseTypes[releaseType]}
            triggerButton={
              <ButtonText
                onClick={() => {
                  onShowReleaseTypeModal?.();
                }}
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
        </SummaryListItem>

        <SummaryListItem term="Receive updates">
          {renderSubscribeLink}
        </SummaryListItem>

        <SummaryListItem term="Produced by">
          {renderProducerLink}
        </SummaryListItem>
      </SummaryList>
    </div>
  );
}
