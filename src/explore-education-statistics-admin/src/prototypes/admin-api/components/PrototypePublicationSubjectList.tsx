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
  onEditTitle: (publicationSubject: PublicationSubject) => void;
  onEditSubject: (publicationSubject: PublicationSubject) => void;
  onPreviewSubject: (publicationSubject: PublicationSubject) => void;
}

const PrototypePublicationSubjectList = ({
  isCurrentReleasePublished,
  publicationSubjects,
  onEditTitle,
  onEditSubject,
  onPreviewSubject,
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
  const [deprecationDate1, setDeprecationDate1] = useState('');
  const [deprecationDate2, setDeprecationDate2] = useState('');
  const [deprecationDate3, setDeprecationDate3] = useState('');

  const [previewInitialDataset, setPreviewInitialDataset] = useState(false);
  const [previewDataset, setPreviewDataset] = useState(false);

  const [deprecationNotesValue, setDeprecationNotesValue] = useState('');
  const [deprecationDateValue, setDeprecationDateValue] = useState('');

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
                              Staging - unpublished
                            </div>{' '}
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
                            Staging - unpublished
                          </div>{' '}
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
                                  onClick={e => {
                                    e.preventDefault();
                                    setPreviewInitialDataset(
                                      !previewInitialDataset,
                                    );
                                    // onPreviewSubject(publicationSubject);
                                  }}
                                >
                                  {!previewInitialDataset
                                    ? 'Preview staged data set'
                                    : 'Close preview for staged data set'}{' '}
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
                                      ? 'Preview staged data set'
                                      : 'Close preview'}{' '}
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
                              Preview version 1.1
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
                                    selectedVersionStatus1 === 'Deprecated'
                                      ? 'govuk-tag--red'
                                      : '',
                                  )}
                                >
                                  {selectedVersionStatus1}
                                </div>{' '}
                              </td>
                              <td className="govuk-table__header--numeric">
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
                                        setDeprecationDateValue(
                                          deprecationDate1,
                                        );
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                                {selectedVersionStatus1 !== 'Deprecated' && (
                                  <a href="#">View changelog</a>
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
                                    setDeprecationDateValue(deprecationDate1);
                                    if (
                                      selectedVersionStatus1 === 'Deprecated'
                                    ) {
                                      setShowDeprecationNotes(true);
                                    } else {
                                      setShowDeprecationNotes(false);
                                    }
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
                              </td>
                              <td className="govuk-table__header--numeric">
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
                                        setDeprecationDateValue(
                                          deprecationDate2,
                                        );
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                                {selectedVersionStatus2 !== 'Deprecated' && (
                                  <a href="#">View changelog</a>
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
                                    setDeprecationDateValue(deprecationDate2);
                                    if (
                                      selectedVersionStatus2 === 'Deprecated'
                                    ) {
                                      setShowDeprecationNotes(true);
                                    } else {
                                      setShowDeprecationNotes(false);
                                    }
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
                              </td>
                              <td className="govuk-table__header--numeric">
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
                                        setDeprecationDateValue(
                                          deprecationDate3,
                                        );
                                      }}
                                    >
                                      View notes
                                    </a>
                                  </>
                                )}
                                {selectedVersionStatus3 !== 'Deprecated' && (
                                  <a href="#">View changelog</a>
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
                                    setDeprecationDateValue(deprecationDate3);

                                    if (
                                      selectedVersionStatus3 === 'Deprecated'
                                    ) {
                                      setShowDeprecationNotes(true);
                                    } else {
                                      setShowDeprecationNotes(false);
                                    }
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
                            <fieldset className="govuk-fieldset govuk-!-margin-top-9">
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
                                      if (selectedVersion === '2.1') {
                                        setSelectedVersionStatus1('Deprecated');
                                        setShowDeprecationNotes(true);
                                      }
                                      if (selectedVersion === '2.0') {
                                        setSelectedVersionStatus2('Deprecated');
                                        setShowDeprecationNotes(true);
                                      }
                                      if (selectedVersion === '1.0') {
                                        setSelectedVersionStatus3('Deprecated');
                                        setShowDeprecationNotes(true);
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
                                  <div className="govuk-radios__conditional">
                                    {' '}
                                    <label htmlFor="deprecationNotes">
                                      Notes
                                      <span className="govuk-hint">
                                        These notes will be appended to the
                                        published API dataset. They are used to
                                        explain to the public users why this
                                        data set is being deprecated.
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
                                    <fieldset className="govuk-fieldset govuk-!-margin-top-9">
                                      <legend className="govuk-legend govuk-fieldset__legend">
                                        <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                                          Date of deprecation
                                        </h3>
                                      </legend>
                                      <p className="govuk-hint">
                                        The date helps give users advance
                                        warning of when the data set will be
                                        deprecated. If you don't yet know a
                                        precise date, just add an estimated
                                        month leaving the day blank.
                                      </p>
                                      <div className="govuk-date-input">
                                        <div className="govuk-date-input__item">
                                          <div className="govuk-form-group">
                                            <label
                                              htmlFor="date-day"
                                              className="govuk-label govuk-date-input__label"
                                            >
                                              Day
                                            </label>
                                            <input
                                              type="number"
                                              className="govuk-input govuk-date-input__input govuk-input--width-2"
                                              name="date-day"
                                              id="date-day"
                                              inputMode="numeric"
                                            />
                                          </div>
                                        </div>
                                        <div className="govuk-date-input__item">
                                          <div className="govuk-form-group">
                                            <label
                                              htmlFor="date-month-year"
                                              className="govuk-label govuk-date-input__label"
                                            >
                                              Month / Year
                                            </label>
                                            <input
                                              type="month"
                                              className="govuk-input govuk-date-input__input"
                                              name="date-month-year"
                                              id="date-month-year"
                                              inputMode="numeric"
                                              defaultValue={
                                                deprecationDateValue
                                              }
                                              onChange={e => {
                                                if (selectedVersion === '2.1') {
                                                  setDeprecationDate1(
                                                    e.target.value,
                                                  );
                                                }
                                                if (selectedVersion === '2.0') {
                                                  setDeprecationDate2(
                                                    e.target.value,
                                                  );
                                                }
                                                if (selectedVersion === '1.0') {
                                                  setDeprecationDate3(
                                                    e.target.value,
                                                  );
                                                }
                                              }}
                                            />
                                          </div>
                                        </div>
                                      </div>
                                    </fieldset>
                                  </div>
                                )}
                              </div>
                            </fieldset>
                          </form>
                          <Button
                            className="govuk-!-margin-top-9"
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
                              <>
                                <p>{deprecationNotes1}</p>
                                <h3 className="govuk-heading-s">
                                  Deprecation date
                                </h3>
                                <input
                                  style={{ border: 'none' }}
                                  type="month"
                                  readOnly
                                  value={deprecationDate1}
                                />
                              </>
                            )}
                            {selectedVersion === '2.0' && (
                              <>
                                <p>{deprecationNotes2}</p>
                                <h3 className="govuk-heading-s">
                                  Deprecation date
                                </h3>
                                <input
                                  style={{ border: 'none' }}
                                  type="month"
                                  readOnly
                                  value={deprecationDate2}
                                />
                              </>
                            )}
                            {selectedVersion === '1.0' && (
                              <>
                                <p>{deprecationNotes3}</p>
                                <h3 className="govuk-heading-s">
                                  Deprecation date
                                </h3>
                                <input
                                  style={{ border: 'none' }}
                                  type="month"
                                  readOnly
                                  value={deprecationDate3}
                                />
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
