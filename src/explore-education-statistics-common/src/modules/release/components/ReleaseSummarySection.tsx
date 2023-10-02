import FormattedDate from '@common/components/FormattedDate';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import { Release } from '@common/services/publicationService';
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
  release: Release;
  releaseDate: string;
  renderReleaseNotes: ReactNode;
  renderStatusTags: ReactNode;
  renderSubscribeLink: ReactNode;
  onShowReleaseTypeModal?: () => void;
}

export default function ReleaseSummarySection({
  isEditing,
  lastUpdated,
  release,
  releaseDate,
  renderReleaseNotes,
  renderStatusTags,
  renderSubscribeLink,
  onShowReleaseTypeModal,
}: Props) {
  return (
    <>
      <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between govuk-!-margin-bottom-3">
        <div>{renderStatusTags}</div>
        {releaseTypesToIcons[release.type] && (
          <img
            src={releaseTypesToIcons[release.type]?.url}
            alt={releaseTypesToIcons[release.type]?.altText}
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
        {release.latestRelease &&
          isValidPartialDate(release.nextReleaseDate) && (
            <SummaryListItem term="Next update">
              <time>{formatPartialDate(release.nextReleaseDate)}</time>
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
            title={releaseTypes[release.type]}
            triggerButton={
              <ButtonText
                onClick={() => {
                  onShowReleaseTypeModal?.();
                }}
              >
                {releaseTypes[release.type]}{' '}
                <InfoIcon
                  description={`Information on ${releaseTypes[release.type]}`}
                />
              </ButtonText>
            }
          >
            <ReleaseTypeSection showHeading={false} type={release.type} />
          </Modal>
        </SummaryListItem>

        <SummaryListItem term="Receive updates">
          {renderSubscribeLink}
        </SummaryListItem>
      </SummaryList>
    </>
  );
}
