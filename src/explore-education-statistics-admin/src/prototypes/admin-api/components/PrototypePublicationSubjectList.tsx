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
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import {
  subjectsForRelease1,
  PublicationSubject,
  subjectsForRelease2,
} from '../PrototypePublicationSubjects';
import PreviewExample from './PrototypePreviewExample';

interface Props {
  isCurrentReleasePublished?: boolean;
  publicationSubjects: PublicationSubject[];
  // onEditTitle: (publicationSubject: PublicationSubject) => void;
  onEditSubject: (publicationSubject: PublicationSubject) => void;
}

const PrototypePublicationSubjectList = ({
  isCurrentReleasePublished,
  publicationSubjects,
  // onEditTitle,
  onEditSubject,
}: Props) => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [archiveList, setArchiveList] = useState(false);
  const [statusModal, toggleStatusModal] = useToggle(false);
  const [statusNotesModal, toggleStatusNotesModal] = useToggle(false);
  const [selectedVersion, setSelectedVersion] = useState('');
  const [showDeprecationNotes, setShowDeprecationNotes] = useState(false);
  const [selectedVersionStatus1, setSelectedVersionStatus1] = useState('Live');

  const [selectedVersionStatus2, setSelectedVersionStatus2] = useState('Live');

  const [selectedVersionStatus3, setSelectedVersionStatus3] = useState('Live');
  const [deprecationNotes1, setDeprecationNotes1] = useState('');
  const [deprecationNotes2, setDeprecationNotes2] = useState('');
  const [deprecationNotes3, setDeprecationNotes3] = useState('');

  const [previewInitialDataset, setPreviewInitialDataset] = useState(false);
  const [previewDataset, setPreviewDataset] = useState(false);

  const [deprecationNotesValue, setDeprecationNotesValue] = useToggle(false);

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
                    key={publicationSubject.title}
                  >
                    <SummaryList className="govuk-!-margin-bottom-9">
                      <SummaryListItem
                        term={
                          subject.release === currentRelease
                            ? 'Next data set to publish'
                            : 'Current data set'
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
                            : 'Current release'
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
                          <>
                            <div className="govuk-tag govuk-!-margin-right-3 govuk-!-margin-left-1">
                              Live
                            </div>
                          </>
                        )}
                        {isCurrentReleasePublished && nextSubject && (
                          <>
                            <a
                              href="#"
                              onClick={e => {
                                e.preventDefault();
                                setArchiveList(!archiveList);
                              }}
                            >
                              {archiveList ? 'Hide' : 'Show'} version history
                            </a>
                          </>
                        )}
                        {subject.release === currentRelease && (
                          <>
                            <div className="govuk-tag govuk-tag--yellow govuk-!-margin-right-3 govuk-!-margin-left-1">
                              Staging
                            </div>{' '}
                            <a
                              href="#"
                              onClick={e => {
                                e.preventDefault();
                                setPreviewInitialDataset(true);
                              }}
                            >
                              Preview initial version
                            </a>
                          </>
                        )}
                      </SummaryListItem>
                    </SummaryList>
                    {nextSubject && !isCurrentReleasePublished && (
                      <SummaryList>
                        <SummaryListItem term="Next API data set to publish">
                          {nextSubject.title}
                        </SummaryListItem>
                        <SummaryListItem term="Next release to publish">
                          {nextSubject.release}
                        </SummaryListItem>
                        <SummaryListItem term="Next API data set version">
                          {nextSubject.version}{' '}
                          <div className="govuk-tag govuk-tag--yellow govuk-!-margin-right-3 govuk-!-margin-left-1 ">
                            Staging
                          </div>{' '}
                          <a
                            href="#"
                            onClick={e => {
                              e.preventDefault();
                              setPreviewDataset(true);
                            }}
                          >
                            Preview this version
                          </a>
                        </SummaryListItem>
                      </SummaryList>
                    )}
                    {archiveList && (
                      <>
                        <table className="govuk-!-margin-bottom-9">
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
                              <th className="govuk-table__header--numeric">
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
                                    selectedVersionStatus1 === 'Deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {selectedVersionStatus1}
                                </div>{' '}
                                {selectedVersionStatus1 === 'Deprecated' && (
                                  <>
                                    {' '}
                                    <a
                                      href="#"
                                      onClick={e => {
                                        e.preventDefault();
                                        setSelectedVersion('2.1');
                                        toggleStatusNotesModal(true);
                                        setDeprecationNotesValue(
                                          deprecationNotes1,
                                        );
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                              </td>
                              <td className="govuk-table__header--numeric">
                                <a
                                  href="#"
                                  onClick={e => {
                                    e.preventDefault();
                                    setSelectedVersion('2.1');
                                    toggleStatusModal(true);
                                    setDeprecationNotesValue(deprecationNotes1);
                                  }}
                                >
                                  Set status
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
                                    selectedVersionStatus2 === 'Deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {selectedVersionStatus2}
                                </div>{' '}
                                {selectedVersionStatus2 === 'Deprecated' && (
                                  <>
                                    {' '}
                                    <a
                                      href="#"
                                      onClick={e => {
                                        e.preventDefault();
                                        setSelectedVersion('2.0');
                                        toggleStatusNotesModal(true);
                                        setDeprecationNotesValue(
                                          deprecationNotes2,
                                        );
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                              </td>
                              <td className="govuk-table__header--numeric">
                                <a
                                  href="#"
                                  onClick={e => {
                                    e.preventDefault();
                                    setSelectedVersion('2.0');
                                    toggleStatusModal(true);
                                    setDeprecationNotesValue(deprecationNotes2);
                                  }}
                                >
                                  Set status
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
                                    selectedVersionStatus3 === 'Deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {selectedVersionStatus3}
                                </div>{' '}
                                {selectedVersionStatus3 === 'Deprecated' && (
                                  <>
                                    {' '}
                                    <a
                                      href="#"
                                      onClick={e => {
                                        e.preventDefault();
                                        setSelectedVersion('1.0');
                                        toggleStatusNotesModal(true);
                                        setDeprecationNotesValue(
                                          deprecationNotes3,
                                        );
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                              </td>
                              <td className="govuk-table__header--numeric">
                                <a
                                  href="#"
                                  onClick={e => {
                                    e.preventDefault();
                                    setSelectedVersion('1.0');
                                    toggleStatusModal(true);
                                    setDeprecationNotesValue(deprecationNotes3);
                                  }}
                                >
                                  Set status
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
                          <form action="#">
                            <fieldset className="govuk-fieldset govuk-!-margin-top-9 govuk-!-margin-bottom-9">
                              <legend className="govuk-legend govuk-fieldset__legend">
                                <h3 className="govuk-heading-m govuk-!-margin-bottom-2">
                                  Change status of this API data set
                                </h3>
                              </legend>
                              <div className="govuk-radios">
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="versionStatus"
                                    id="versionActive"
                                    checked={
                                      (selectedVersion === '2.1' &&
                                        selectedVersionStatus1 === 'Live') ||
                                      (selectedVersion === '2.0' &&
                                        selectedVersionStatus2 === 'Live') ||
                                      (selectedVersion === '1.0' &&
                                        selectedVersionStatus3 === 'Live')
                                    }
                                    onClick={() => {
                                      setShowDeprecationNotes(false);
                                      if (selectedVersion === '2.1') {
                                        setSelectedVersionStatus1('Live');
                                      }
                                      if (selectedVersion === '2.0') {
                                        setSelectedVersionStatus2('Live');
                                      }
                                      if (selectedVersion === '1.0') {
                                        setSelectedVersionStatus3('Live');
                                      }
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="version-minor"
                                  >
                                    Live
                                  </label>
                                </div>

                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="versionStatus"
                                    id="versionDeprecated"
                                    checked={
                                      (selectedVersion === '2.1' &&
                                        selectedVersionStatus1 ===
                                          'Deprecated') ||
                                      (selectedVersion === '2.0' &&
                                        selectedVersionStatus2 ===
                                          'Deprecated') ||
                                      (selectedVersion === '1.0' &&
                                        selectedVersionStatus3 === 'Deprecated')
                                    }
                                    onClick={() => {
                                      setShowDeprecationNotes(true);
                                      if (selectedVersion === '2.1') {
                                        setSelectedVersionStatus1('Deprecated');
                                      }
                                      if (selectedVersion === '2.0') {
                                        setSelectedVersionStatus2('Deprecated');
                                      }
                                      if (selectedVersion === '1.0') {
                                        setSelectedVersionStatus3('Deprecated');
                                      }
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="version-major"
                                  >
                                    Deprecate
                                  </label>
                                </div>
                                {showDeprecationNotes && (
                                  <div className="govuk-!-margin-top-9">
                                    {' '}
                                    <label htmlFor="deprecationNotes">
                                      Notes
                                      <span className="govuk-hint">
                                        Explain why and when this data set is
                                        being deprecrated
                                      </span>
                                    </label>
                                    <textarea
                                      className="govuk-textarea"
                                      id="deprectationNotes"
                                      onChange={e => {
                                        if (selectedVersion === '2.1') {
                                          setDeprecationNotes1(e.target.value);
                                        }
                                        if (selectedVersion === '2.0') {
                                          setDeprecationNotes2(e.target.value);
                                        }
                                        if (selectedVersion === '1.0') {
                                          setDeprecationNotes3(e.target.value);
                                        }
                                      }}
                                    >
                                      {deprecationNotesValue}
                                    </textarea>
                                  </div>
                                )}
                              </div>
                            </fieldset>
                          </form>
                          <Button
                            onClick={() => {
                              toggleStatusModal(false);
                            }}
                          >
                            Update
                          </Button>
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
                              <>{deprecationNotes1}</>
                            )}
                            {selectedVersion === '2.0' && (
                              <>{deprecationNotes2}</>
                            )}
                            {selectedVersion === '1.0' && (
                              <>{deprecationNotes3}</>
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
                      </>
                    )}
                    {!isCurrentReleasePublished && (
                      <>
                        <SummaryList
                          noBorder
                          className="govuk-margin-!-bottom-9"
                        >
                          <SummaryListItem term="Actions">
                            <ButtonGroup className="dfe-justify-content--flex-end">
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
                                  {/* <ButtonText
                                    className="govuk-!-margin-left-6"
                                    onClick={e => {
                                      e.preventDefault();
                                      setPreviewDataset(true);
                                    }}
                                  >
                                    Preview API data set to be publishe
                                  </ButtonText> */}
                                  <ButtonText
                                    onClick={e => {
                                      e.preventDefault();
                                      setArchiveList(!archiveList);
                                    }}
                                  >
                                    View and manage previous versions
                                  </ButtonText>
                                  <ButtonText className="govuk-!-margin-left-6 govuk-!-margin-right-6">
                                    Edit next data set
                                  </ButtonText>
                                  <ButtonText>Remove next data set</ButtonText>
                                </>
                              )}

                              {/* <ButtonText
                              onClick={() => onEditTitle(publicationSubject)}
                            >
                              Edit dataset title
                            </ButtonText> */}

                              {subject.release !== currentRelease &&
                                !nextSubject && (
                                  <Link
                                    className="govuk-button"
                                    to={`./2022-23/prepare-subject/${publicationSubject.subjectId}`}
                                  >
                                    Create new API data set version
                                  </Link>
                                )}

                              {subject.release === currentRelease && (
                                <ButtonText
                                  variant="warning"
                                  className="govuk-!-margin-left-6"
                                >
                                  Delete
                                </ButtonText>
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
                            <ButtonText
                              onClick={e => {
                                e.preventDefault();
                                setPreviewInitialDataset(false);
                              }}
                            >
                              Hide preview
                            </ButtonText>
                          </div>
                        )}
                        {previewDataset && (
                          <div className="govuk-!-margin-bottom-9">
                            <h4 className="govuk-heading-m">
                              Preview version 1.1
                            </h4>
                            <PreviewExample />
                            <ButtonText
                              onClick={e => {
                                e.preventDefault();
                                setPreviewDataset(false);
                              }}
                            >
                              Hide preview
                            </ButtonText>
                          </div>
                        )}
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
