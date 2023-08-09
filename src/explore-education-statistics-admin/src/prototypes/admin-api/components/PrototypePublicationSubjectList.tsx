import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import classNames from 'classnames';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Link from '@admin/components/Link';
import React, { useState } from 'react';
import { useParams } from 'react-router-dom';
import capitalize from 'lodash/capitalize';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import FormattedDate from '@common/components/FormattedDate';
import { formatPartialDate } from '@common/utils/date/partialDate';
import {
  subjectsForRelease1,
  PublicationSubject,
  subjectsForRelease2,
} from '../PrototypePublicationSubjects';
import PreviewExample from './PrototypePreviewExample';
import PrototypeChangeStatusForm, {
  StatusFormValues,
} from './PrototypeChangeStatusForm';

interface Props {
  isCurrentReleasePublished?: boolean;
  publicationSubjects: PublicationSubject[];
  onEditSubject: (publicationSubject: PublicationSubject) => void;
}

const PrototypePublicationSubjectList = ({
  isCurrentReleasePublished,
  publicationSubjects,
  onEditSubject,
}: Props) => {
  const [archiveList, setArchiveList] = useState(false);
  const [statusModal, toggleStatusModal] = useToggle(false);
  const [statusNotesModal, toggleStatusNotesModal] = useToggle(false);
  const [changelogModal, toggleChangelogModal] = useToggle(false);
  const [selectedVersion, setSelectedVersion] = useState('');

  const [previewInitialDataset, setPreviewInitialDataset] = useState(false);
  const [previewDataset, setPreviewDataset] = useState(false);

  const [version1Status, setVersion1Status] = useState<StatusFormValues>({
    status: 'live',
  });
  const [version2Status, setVersion2Status] = useState<StatusFormValues>({
    status: 'live',
  });
  const [version3Status, setVersion3Status] = useState<StatusFormValues>({
    status: 'live',
  });

  function getStatus() {
    if (selectedVersion === '2.1') {
      return version1Status;
    }
    if (selectedVersion === '2.0') {
      return version2Status;
    }
    return version3Status;
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const params: any = useParams();

  const currentRelease =
    params.id && params.id === '2022-23'
      ? 'Academic Year 2022/23'
      : 'Academic Year 2021/22';

  return (
    <>
      {publicationSubjects.length > 0 && (
        <>
          <h3 className="govuk-!-margin-top-9">Selected API data </h3>
          <Accordion id="ps">
            {publicationSubjects.map(publicationSubject => {
              const subject = subjectsForRelease1.find(
                s => s.id === publicationSubject.subjectId,
              );
              const nextSubject = publicationSubject.nextSubjectId
                ? subjectsForRelease2.find(
                    s => s.id === publicationSubject.nextSubjectId,
                  )
                : undefined;
              if (subject) {
                return (
                  <AccordionSection
                    goToTop={false}
                    heading={
                      isCurrentReleasePublished && nextSubject
                        ? nextSubject.title
                        : subject.title
                    }
                    headingTag="h3"
                    open
                    key={publicationSubject.subjectId}
                  >
                    <SummaryList
                      className={
                        nextSubject
                          ? 'govuk-!-margin-bottom-9'
                          : 'govuk-!-margin-bottom-0'
                      }
                    >
                      <SummaryListItem
                        term={
                          subject.release === currentRelease
                            ? 'Next data set to publish'
                            : 'Current data set (live)'
                        }
                      >
                        {isCurrentReleasePublished && nextSubject
                          ? nextSubject.title
                          : subject.title}
                      </SummaryListItem>
                      <SummaryListItem
                        term={
                          subject.release === currentRelease
                            ? 'Next release to publish'
                            : 'Current release (live)'
                        }
                      >
                        {isCurrentReleasePublished && nextSubject
                          ? nextSubject.release
                          : subject.release}
                      </SummaryListItem>
                      <SummaryListItem
                        term={
                          subject.release === currentRelease
                            ? 'Next API data set version'
                            : 'Current API data set version'
                        }
                      >
                        {isCurrentReleasePublished && nextSubject
                          ? nextSubject.version
                          : subject.version}{' '}
                        {subject.release !== currentRelease && (
                          <div className="govuk-tag govuk-!-margin-right-3 govuk-!-margin-left-1">
                            Live
                          </div>
                        )}
                        {isCurrentReleasePublished && nextSubject && (
                          <a
                            href="#"
                            onClick={e => {
                              e.preventDefault();
                              setArchiveList(!archiveList);
                            }}
                          >
                            {archiveList ? 'Hide' : 'Show'} version history
                          </a>
                        )}
                        {subject.release === currentRelease && (
                          <>
                            <div className="govuk-tag govuk-tag--yellow govuk-!-margin-right-3 govuk-!-margin-left-1">
                              Staging - unpublished
                            </div>{' '}
                          </>
                        )}
                      </SummaryListItem>
                    </SummaryList>
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
                        </SummaryListItem>
                        <SummaryListItem term="API data set version status">
                          <div className="govuk-tag govuk-tag--yellow govuk-!-margin-right-3  ">
                            Staging - unpublished
                          </div>
                        </SummaryListItem>
                        <SummaryListItem term="Notifications">
                          0
                        </SummaryListItem>
                      </SummaryList>
                    )}

                    {!isCurrentReleasePublished && (
                      <>
                        <SummaryList
                          noBorder
                          className="govuk-margin-!-bottom-9"
                        >
                          <SummaryListItem term="Actions">
                            <ButtonGroup className="dfe-justify-content--flex-start">
                              {subject.release === currentRelease && (
                                <ButtonText
                                  className="govuk-!-margin-right-3 govuk-!-margin-left-0"
                                  onClick={() => {
                                    setPreviewInitialDataset(
                                      !previewInitialDataset,
                                    );
                                  }}
                                >
                                  {!previewInitialDataset
                                    ? 'Preview and notifications'
                                    : 'Close preview and notifications'}{' '}
                                  ({subject.version})
                                </ButtonText>
                              )}

                              {subject.release === currentRelease && (
                                <ButtonText
                                  onClick={() =>
                                    onEditSubject(publicationSubject)
                                  }
                                >
                                  Change data set to be published
                                </ButtonText>
                              )}
                              {nextSubject && (
                                <>
                                  <ButtonText
                                    className="govuk-!-margin-left-0 govuk-!-margin-right-6"
                                    onClick={e => {
                                      e.preventDefault();
                                      setPreviewDataset(!previewDataset);
                                      setArchiveList(false);
                                    }}
                                  >
                                    {!previewDataset
                                      ? 'Preview and notifications'
                                      : 'Close preview and notifications'}{' '}
                                  </ButtonText>
                                  <ButtonText
                                    onClick={e => {
                                      e.preventDefault();
                                      setPreviewDataset(false);
                                      setArchiveList(!archiveList);
                                    }}
                                  >
                                    {!archiveList
                                      ? 'View version history'
                                      : 'Close version history'}{' '}
                                  </ButtonText>
                                  <ButtonText className="govuk-!-margin-left-6 govuk-!-margin-right-6">
                                    Edit next data set
                                  </ButtonText>
                                  <div style={{ marginLeft: 'auto' }}>
                                    <ButtonText variant="warning">
                                      Remove next data set
                                    </ButtonText>
                                  </div>
                                </>
                              )}

                              {/* <ButtonText
                              onClick={() => onEditTitle(publicationSubject)}
                            >
                              Edit dataset title
                            </ButtonText> */}

                              {subject.release !== currentRelease &&
                                !nextSubject && (
                                  <div style={{ marginLeft: 'auto' }}>
                                    <Link
                                      className="govuk-button"
                                      to={`./2022-23/prepare-subject/${publicationSubject.subjectId}`}
                                    >
                                      Create new API data set version
                                    </Link>
                                  </div>
                                )}

                              {subject.release === currentRelease && (
                                <div style={{ marginLeft: 'auto' }}>
                                  <ButtonText
                                    variant="warning"
                                    className="govuk-!-margin-left-6"
                                  >
                                    Delete
                                  </ButtonText>
                                </div>
                              )}
                            </ButtonGroup>
                          </SummaryListItem>
                        </SummaryList>
                        {previewInitialDataset && (
                          <div className="govuk-!-margin-bottom-9">
                            <h4 className="govuk-heading-m">
                              Preview version 1.0
                            </h4>
                            <PreviewExample initialVersion />
                          </div>
                        )}
                        {previewDataset && (
                          <div className="govuk-!-margin-bottom-9">
                            <h4 className="govuk-heading-m">
                              Preview and notifications for version 1.1
                            </h4>
                            <PreviewExample />
                            <ButtonText
                              onClick={e => {
                                e.preventDefault();
                                setPreviewDataset(false);
                              }}
                            >
                              Close preview
                            </ButtonText>
                          </div>
                        )}
                      </>
                    )}
                    {archiveList && (
                      <>
                        <table className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">
                          <caption className="govuk-!-margin-bottom-3 govuk-table__caption govuk-table__caption--m">
                            Version history **Example set in the future**
                          </caption>
                          <thead>
                            <tr>
                              <th style={{ width: '30%' }}>
                                API data set version
                              </th>
                              <th>Related release</th>
                              <th>Status</th>
                              <th
                                className="govuk-table__header--numeric"
                                colSpan={2}
                              >
                                Actions
                              </th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr>
                              <td>2.1</td>
                              <td>Academic year 2023/24</td>
                              <td>
                                <div
                                  className={classNames(
                                    'govuk-tag',
                                    version1Status.status === 'deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {capitalize(version1Status.status)}
                                </div>{' '}
                              </td>
                              <td className="govuk-table__header--numeric">
                                {version1Status.status === 'deprecated' && (
                                  <>
                                    {' '}
                                    <a
                                      href="#"
                                      onClick={e => {
                                        e.preventDefault();
                                        setSelectedVersion('2.1');
                                        toggleStatusNotesModal.on();
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                                {version1Status.status !== 'deprecated' && (
                                  <ButtonText onClick={toggleChangelogModal.on}>
                                    View changelog
                                  </ButtonText>
                                )}
                              </td>
                              <td className="govuk-table__header--numeric">
                                <a
                                  href="#"
                                  onClick={e => {
                                    e.preventDefault();
                                    setSelectedVersion('2.1');
                                    toggleStatusModal.on();
                                  }}
                                >
                                  Edit status
                                </a>
                              </td>
                            </tr>
                            <tr>
                              <td>2.0</td>
                              <td>Academic year 2022/23</td>
                              <td>
                                <div
                                  className={classNames(
                                    'govuk-tag',
                                    version2Status.status === 'deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {version2Status.status}
                                </div>{' '}
                              </td>
                              <td className="govuk-table__header--numeric">
                                {version2Status.status === 'deprecated' && (
                                  <>
                                    {' '}
                                    <a
                                      href="#"
                                      onClick={e => {
                                        e.preventDefault();
                                        setSelectedVersion('2.0');
                                        toggleStatusNotesModal.on();
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                                {version2Status.status !== 'deprecated' && (
                                  <ButtonText onClick={toggleChangelogModal.on}>
                                    View changelog
                                  </ButtonText>
                                )}
                              </td>
                              <td className="govuk-table__header--numeric">
                                <a
                                  href="#"
                                  onClick={e => {
                                    e.preventDefault();
                                    setSelectedVersion('2.0');
                                    toggleStatusModal.on();
                                  }}
                                >
                                  Edit status
                                </a>
                              </td>
                            </tr>
                            <tr>
                              <td>1.0</td>
                              <td>Academic year 2021/22</td>
                              <td>
                                <div
                                  className={classNames(
                                    'govuk-tag',
                                    version3Status.status === 'deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {version3Status.status}
                                </div>{' '}
                              </td>
                              <td className="govuk-table__header--numeric">
                                {version3Status.status === 'deprecated' && (
                                  <>
                                    {' '}
                                    <a
                                      href="#"
                                      onClick={e => {
                                        e.preventDefault();
                                        setSelectedVersion('1.0');
                                        toggleStatusNotesModal.on();
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                                {version3Status.status !== 'deprecated' && (
                                  <ButtonText onClick={toggleChangelogModal.on}>
                                    View changelog
                                  </ButtonText>
                                )}
                              </td>
                              <td className="govuk-table__header--numeric">
                                <a
                                  href="#"
                                  onClick={e => {
                                    e.preventDefault();
                                    setSelectedVersion('1.0');
                                    toggleStatusModal.on();
                                  }}
                                >
                                  Edit status
                                </a>
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        <Modal
                          open={statusModal}
                          title={`API data set version ${selectedVersion}`}
                          className="govuk-!-width-one-half"
                        >
                          <PrototypeChangeStatusForm
                            selectedStatus={getStatus()}
                            onSubmit={values => {
                              if (selectedVersion === '2.1') {
                                setVersion1Status(values);
                              } else if (selectedVersion === '2.0') {
                                setVersion2Status(values);
                              } else {
                                setVersion3Status(values);
                              }
                              toggleStatusModal.off();
                            }}
                          />
                        </Modal>

                        <Modal
                          open={statusNotesModal}
                          title={`Deprecated API data set, version ${selectedVersion}`}
                          className="govuk-!-width-one-half"
                        >
                          <h3 className="govuk-heading-s">Notes</h3>
                          <div
                            className="govuk-!-margin-bottom-9"
                            style={{ whiteSpace: 'pre-wrap' }}
                          >
                            {selectedVersion === '2.1' && (
                              <>
                                <p>{version1Status.notes}</p>
                                <h3 className="govuk-heading-s">
                                  Expiry date for this data set
                                </h3>

                                {version1Status.date && (
                                  <p>
                                    {version1Status.date.day ? (
                                      <FormattedDate format="d MMM yyyy">
                                        {
                                          new Date(
                                            `${version1Status.date.month}/${version1Status.date.day}/${version1Status.date.year}`,
                                          )
                                        }
                                      </FormattedDate>
                                    ) : (
                                      <time>
                                        {formatPartialDate(version1Status.date)}
                                      </time>
                                    )}
                                  </p>
                                )}
                              </>
                            )}
                            {selectedVersion === '2.0' && (
                              <>
                                <p>{version2Status.notes}</p>
                                <h3 className="govuk-heading-s">
                                  Deprecation date
                                </h3>

                                {version2Status.date && (
                                  <p>
                                    {version2Status.date.day ? (
                                      <FormattedDate format="d MMM yyyy">
                                        {
                                          new Date(
                                            `${version2Status.date.month}/${version2Status.date.day}/${version2Status.date.year}`,
                                          )
                                        }
                                      </FormattedDate>
                                    ) : (
                                      <time>
                                        {formatPartialDate(version2Status.date)}
                                      </time>
                                    )}
                                  </p>
                                )}
                              </>
                            )}
                            {selectedVersion === '1.0' && (
                              <>
                                <p>{version3Status.notes}</p>
                                <h3 className="govuk-heading-s">
                                  Deprecation date
                                </h3>

                                {version3Status.date && (
                                  <p>
                                    {version3Status.date.day ? (
                                      <FormattedDate format="d MMM yyyy">
                                        {
                                          new Date(
                                            `${version3Status.date.month}/${version3Status.date.day}/${version3Status.date.year}`,
                                          )
                                        }
                                      </FormattedDate>
                                    ) : (
                                      <time>
                                        {formatPartialDate(version3Status.date)}
                                      </time>
                                    )}
                                  </p>
                                )}
                              </>
                            )}
                          </div>
                          <Button
                            onClick={() => {
                              toggleStatusNotesModal(false);
                            }}
                          >
                            Close
                          </Button>
                        </Modal>

                        <Modal
                          open={changelogModal}
                          title="Changelog"
                          className="govuk-!-width-one-half"
                          onExit={toggleChangelogModal.off}
                        >
                          <>
                            <h3>Version notes</h3>
                            <p>
                              This is a minor update on the previous version,
                              some new locations, filters and indicators have
                              been added to the data set since the previous
                              release, please see the details in the changelog
                              below.
                            </p>

                            <Button onClick={toggleChangelogModal.off}>
                              Close
                            </Button>
                          </>
                        </Modal>
                      </>
                    )}
                  </AccordionSection>
                );
              }
              return null;
            })}
          </Accordion>
          {/*
          {!isCurrentReleasePublished && (
            <div className="govuk-!-margin-top-9">
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setArchiveList(!archiveList);
                }}
              >
                {archiveList ? 'Hide' : 'Show'} example archive list
              </a>
            </div>
          )}
              */}
        </>
      )}
    </>
  );
};

export default PrototypePublicationSubjectList;
