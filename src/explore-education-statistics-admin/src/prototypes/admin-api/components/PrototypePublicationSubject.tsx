import ButtonText from '@common/components/ButtonText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ButtonGroup from '@common/components/ButtonGroup';
import useToggle from '@common/hooks/useToggle';
import {
  PrototypeNotification,
  PrototypeSubject,
  PublicationSubject,
} from '@admin/prototypes/admin-api/PrototypePublicationSubjects';
import PrototypeVersionHistory from '@admin/prototypes/admin-api/components/PrototypeVersionHistory';
import PreviewExample from '@admin/prototypes/admin-api/components/PrototypePreviewExample';
import PrototypeNotificationsList from '@admin/prototypes/admin-api/components/PrototypeNotificationsList';
import { Link } from 'react-router-dom';
import React from 'react';

interface Props {
  currentRelease: string;
  isCurrentReleasePublished?: boolean;
  nextSubject?: PrototypeSubject;
  notifications: PrototypeNotification[];
  publicationSubject: PublicationSubject;
  subject: PrototypeSubject;
  onEditSubject: (publicationSubject: PublicationSubject) => void;
}

const PrototypePublicationSubject = ({
  currentRelease,
  isCurrentReleasePublished,
  nextSubject,
  notifications,
  publicationSubject,
  subject,
  onEditSubject,
}: Props) => {
  const [showVersionHistory, toggleVersionHistory] = useToggle(false);
  const [showNextVersionHistory, toggleNextVersionHistory] = useToggle(false);
  const [previewInitialDataset, togglePreviewInitialDataset] = useToggle(false);
  const [previewNextDataSet, togglePreviewNextDataSet] = useToggle(false);
  const [showNotificationsList, toggleNotificationsList] = useToggle(false);
  // const [previewPublicPage, onTogglePreview] = useToggle(false);

  const showVersionHistoryLink =
    (isCurrentReleasePublished && nextSubject) ||
    (!isCurrentReleasePublished && subject.version !== '1.0');

  const isLive = subject.release !== currentRelease;

  return (
    <>
      <SummaryList
        className={
          nextSubject ? 'govuk-!-margin-bottom-9' : 'govuk-!-margin-bottom-0'
        }
      >
        <SummaryListItem
          term={
            !isLive ? 'Next data set to publish' : 'Current data set (live)'
          }
        >
          {isCurrentReleasePublished && nextSubject
            ? nextSubject.title
            : subject.title}
        </SummaryListItem>
        <SummaryListItem
          term={!isLive ? 'Next release to publish' : 'Current release (live)'}
        >
          {isCurrentReleasePublished && nextSubject
            ? nextSubject.release
            : subject.release}
        </SummaryListItem>
        <SummaryListItem
          term={
            !isLive
              ? 'Next API data set version'
              : 'Current API data set version'
          }
        >
          {isCurrentReleasePublished && nextSubject
            ? nextSubject.version
            : subject.version}{' '}
          {isLive ? (
            <div className="govuk-tag  govuk-!-margin-left-1">Live</div>
          ) : (
            <>
              <div className="govuk-tag govuk-tag--yellow  govuk-!-margin-left-1">
                Unpublished
              </div>{' '}
            </>
          )}
          {showVersionHistoryLink && (
            <ButtonText
              className="govuk-!-margin-left-2"
              onClick={toggleVersionHistory}
            >
              {showVersionHistory ? 'Hide' : 'Show'} version history
            </ButtonText>
          )}
          {showVersionHistory && <PrototypeVersionHistory />}
        </SummaryListItem>

        {isLive ? (
          <>
            {notifications.length > 0 && (
              <SummaryListItem term="Notifications">
                <ButtonText
                  variant="secondary"
                  onClick={toggleNotificationsList}
                >
                  {`${showNotificationsList ? 'Hide' : 'Show'} notifications (${
                    notifications.length
                  })`}
                </ButtonText>

                {showNotificationsList && (
                  <PrototypeNotificationsList notifications={notifications} />
                )}
              </SummaryListItem>
            )}
            <SummaryListItem term="View">
              <a href="../../data-selected5" target="_blank">
                View live API data set page (opens in a new tab)
              </a>
            </SummaryListItem>
          </>
        ) : (
          <SummaryListItem term="Preview">
            <ButtonGroup>
              <ButtonText
                className="govuk-!-margin-right-3 govuk-!-margin-left-0"
                onClick={togglePreviewInitialDataset}
              >
                {!previewInitialDataset
                  ? 'Preview API data set'
                  : 'Close API data set preview'}
              </ButtonText>
              {/* 
               <ButtonText
                className="govuk-!-margin-right-3 govuk-!-margin-left-0"
                onClick={() => onTogglePreview(subject)}
              >
                Preview public API data set page
              </ButtonText>
              */}
            </ButtonGroup>

            {previewInitialDataset && (
              <div className="govuk-!-margin-bottom-4 govuk-!-margin-top-4">
                <h4 className="govuk-heading-m">
                  Preview API data set for version 1.0
                </h4>
                <PreviewExample />
              </div>
            )}
          </SummaryListItem>
        )}

        {/* First version - unpublished */}
        {!isLive && !nextSubject && (
          <SummaryListItem term="Actions">
            <ButtonGroup>
              <ButtonText onClick={() => onEditSubject(publicationSubject)}>
                Change data set to be published
              </ButtonText>
              <ButtonText variant="warning" className="govuk-!-margin-left-3">
                Delete
              </ButtonText>
            </ButtonGroup>
          </SummaryListItem>
        )}

        {/* First version - published, no next version */}
        {isLive && !nextSubject && (
          <SummaryListItem term="Actions">
            <Link
              className="govuk-button"
              to={`./2022-23/prepare-subject/${publicationSubject.subjectId}`}
            >
              Create new API data set version
            </Link>
          </SummaryListItem>
        )}
      </SummaryList>

      {/* Next version */}
      {nextSubject && !isCurrentReleasePublished && (
        <SummaryList className="govuk-!-margin-bottom-0">
          <SummaryListItem term="Next API data set to publish">
            {nextSubject.title}
          </SummaryListItem>
          <SummaryListItem term="Next release to publish">
            {nextSubject.release}
          </SummaryListItem>
          <SummaryListItem term="Next API data set version">
            {nextSubject.version}{' '}
            <ButtonText
              className="govuk-!-margin-left-2"
              onClick={toggleNextVersionHistory}
            >
              {showNextVersionHistory ? 'Hide' : 'Show'} version history
            </ButtonText>
            {showNextVersionHistory && <PrototypeVersionHistory />}
          </SummaryListItem>
          <SummaryListItem term="API data set version status">
            <div className="govuk-tag govuk-tag--yellow govuk-!-margin-right-3  ">
              Unpublished
            </div>
          </SummaryListItem>
          <SummaryListItem term="Preview">
            <ButtonGroup>
              <ButtonText
                className="govuk-!-margin-right-3"
                onClick={togglePreviewNextDataSet}
              >
                {!previewNextDataSet
                  ? 'Preview API data set'
                  : 'Close API data set preview'}
              </ButtonText>
              {/* 
              <ButtonText
                className="govuk-!-margin-right-3"
                onClick={() => onTogglePreview(subject)}
              >
                Preview public API data set page
              </ButtonText>
              */}
            </ButtonGroup>
            {previewNextDataSet && (
              <div className="govuk-!-margin-bottom-4 govuk-!-margin-top-4">
                <h4 className="govuk-heading-m">
                  Preview API data set for version 1.1
                </h4>
                <PreviewExample />
                <ButtonText
                  onClick={e => {
                    e.preventDefault();
                    togglePreviewNextDataSet.off();
                  }}
                >
                  Close preview
                </ButtonText>
              </div>
            )}
          </SummaryListItem>
          <SummaryListItem term="Actions">
            <ButtonGroup>
              <ButtonText className="govuk-!-margin-right-3">
                Edit next data set
              </ButtonText>
              <ButtonText variant="warning">Remove next data set</ButtonText>
            </ButtonGroup>
          </SummaryListItem>
        </SummaryList>
      )}
    </>
  );
};

export default PrototypePublicationSubject;
