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

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const releaseTypesToIcons: Partial<Record<ReleaseType, ReleaseTypeIcon>> = {
  NationalStatistics: {
    url: '/assets/images/UKSA-quality-mark.jpg',
    altText: 'UK statistics authority quality mark',
  },
};

interface Props {
  isEditing?: boolean;
  lastUpdated?: Date;
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
  releaseDate?: string;
  releaseType: ReleaseType;
  renderReleaseNotes: ReactNode;
  renderStatusTags: ReactNode;
  renderSubscribeLink: ReactNode;
  trackScroll?: boolean;
  onShowReleaseTypeModal?: () => void;
}

export default function ReleaseSummarySection({
  isEditing,
  lastUpdated,
  latestRelease,
  nextReleaseDate,
  releaseDate,
  releaseType,
  renderReleaseNotes,
  renderStatusTags,
  renderSubscribeLink,
  trackScroll = false,
  onShowReleaseTypeModal,
}: Props) {
  return (
    <div data-scroll={trackScroll ? true : undefined} id="summary-section">
      <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between govuk-!-margin-bottom-3">
        <div>{renderStatusTags}</div>
        {releaseTypesToIcons[releaseType] && (
          <img
            src={releaseTypesToIcons[releaseType]?.url}
            alt={releaseTypesToIcons[releaseType]?.altText}
            height="60"
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
            <ReleaseTypeSection showHeading={false} type={releaseType} />
          </Modal>
        </SummaryListItem>

        <SummaryListItem term="Receive updates">
          {renderSubscribeLink}
        </SummaryListItem>
      </SummaryList>
    </div>
  );
}
