import React, { useState } from 'react';
import Button from '@common/components/Button';
import Tag from '@common/components/Tag';
import Details from '@common/components/Details';
import WarningMessage from '@common/components/WarningMessage';
import Link from '@admin/components/Link';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import ModalContent from '@admin/prototypes/components/PrototypeModalContent';

const PrototypePublicationReleaseList = () => {
  const [showMore, setShowMore] = useState(false);
  const [showScheduled, setShowScheduled] = useState(false);
  const [showReleases, setShowReleases] = useState(true);
  const [showDraft, setShowDraft] = useState(true);
  const [showHelpStatusModal, toggleHelpStatusModal] = useToggle(false);
  const [showHelpStatusPublishedModal, toggleHelpStatusPublishedModal] =
    useToggle(false);
  const [showScheduledStatusModal, toggleScheduledStatusModal] =
    useToggle(false);
  const [showScheduledStagesModal, toggleScheduledStagesModal] =
    useToggle(false);
  const [showReleaseIssuesModal, toggleReleaseIssuesModal] = useToggle(false);

  return (
    <>
      <h3 className="govuk-heading-l">Manage releases</h3>
      <ModalContent contentType="draftReleases" />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters govuk-!-margin-bottom-6">
          <p className="govuk-hint govuk-!-margin-bottom-6">
            View, edit or amend releases contained within this publication.
          </p>
        </div>
        {showReleases && (
          <div className="govuk-grid-column-one-quarter dfe-align--right">
            <Button>Create new release</Button>
          </div>
        )}
      </div>
      {!showReleases && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <WarningMessage className="govuk-!-margin-bottom-2">
              No releases created in this publication
            </WarningMessage>
          </div>
          <div className="govuk-grid-column-one-quarter dfe-align--right govuk-!-margin-top-3">
            <Button>Create new release</Button>
          </div>
        </div>
      )}

      {showReleases && (
        <>
          {showScheduled && (
            <div style={{ width: '100%', overflow: 'auto' }}>
              <table className="govuk-table govuk-!-margin-bottom-9">
                <caption className="govuk-table__caption--m">
                  Scheduled releases
                </caption>
                <thead className="govuk-table__head">
                  <tr className="govuk-table__row">
                    <th style={{ width: '38%' }}>Release period</th>
                    <th style={{ width: '15%' }}>
                      State
                      <a
                        href="#"
                        className="govuk-!-margin-left-1"
                        onClick={() => {
                          toggleScheduledStatusModal(true);
                        }}
                      >
                        <InfoIcon description="What are the publication stages?" />
                      </a>
                    </th>
                    <th style={{ width: '310px' }} colSpan={3}>
                      Stages checklist
                      <a
                        href="#"
                        className="govuk-!-margin-left-1"
                        onClick={() => {
                          toggleScheduledStagesModal(true);
                        }}
                      >
                        <InfoIcon description="What is status?" />
                      </a>
                    </th>
                    <th>Publish date</th>
                    <th colSpan={2} className="govuk-table__cell--numeric">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>Academic year 2020/21 (Not live)</td>
                    <td>
                      <Tag colour="red">Started</Tag>
                    </td>
                    <td colSpan={3}>
                      <Details
                        summary="View stages"
                        className="govuk-!-margin-bottom-0"
                      >
                        <h4>Release process started</h4>
                        <ul className="govuk-list">
                          <li>
                            <Tag colour="orange">Data Started</Tag>
                          </li>
                          <li>
                            <Tag colour="blue">Content not started</Tag>
                          </li>
                          <li>
                            <Tag colour="green">Files complete ✓</Tag>
                          </li>
                          <li>
                            <Tag colour="blue">Publishing not started</Tag>
                          </li>
                        </ul>
                      </Details>
                    </td>
                    <td>12 January 22</td>
                    <td />
                    <td className="govuk-table__cell--numeric">
                      <a href="#">
                        View{' '}
                        <span className="govuk-visually-hidden">
                          Academic year 2019/20 (Not live)
                        </span>
                      </a>
                    </td>
                  </tr>
                </tbody>
              </table>
              <Modal
                open={showScheduledStatusModal}
                title="Status guidance"
                className="govuk-!-width-one-half"
              >
                <ModalContent contentType="scheduledStatusModal" />
                <Button
                  onClick={() => {
                    toggleScheduledStatusModal(false);
                  }}
                >
                  Close
                </Button>
              </Modal>
              <Modal
                open={showScheduledStagesModal}
                title="Publication stages guidance"
                className="govuk-!-width-one-half"
              >
                <ModalContent contentType="scheduledStagesModal" />
                <Button
                  onClick={() => {
                    toggleScheduledStagesModal(false);
                  }}
                >
                  Close
                </Button>
              </Modal>
            </div>
          )}

          {showDraft && (
            <div style={{ width: '100%', overflow: 'auto' }}>
              <table className="govuk-table govuk-!-margin-bottom-9">
                <caption className="govuk-table__caption--m">
                  Draft releases
                </caption>
                <thead className="govuk-table__head">
                  <tr className="govuk-table__row">
                    <th style={{ width: '38%' }}>Release period</th>
                    <th style={{ width: '15%' }}>
                      State
                      <a
                        href="#"
                        className="govuk-!-margin-left-1"
                        onClick={() => {
                          toggleHelpStatusModal(true);
                        }}
                      >
                        <InfoIcon description="What is status?" />
                      </a>
                    </th>
                    <th style={{ width: '9%' }}>
                      Errors
                      <a
                        href="#"
                        className="govuk-!-margin-left-1"
                        onClick={() => {
                          toggleReleaseIssuesModal(true);
                        }}
                      >
                        <InfoIcon description="What is status?" />
                      </a>
                    </th>
                    <th style={{ width: '10%' }}>
                      Warnings
                      <a
                        href="#"
                        className="govuk-!-margin-left-1"
                        onClick={() => {
                          toggleReleaseIssuesModal(true);
                        }}
                      >
                        <InfoIcon description="What is status?" />
                      </a>
                    </th>
                    <th style={{ width: '20%' }}>
                      Unresolved comments
                      <a
                        href="#"
                        className="govuk-!-margin-left-1"
                        onClick={() => {
                          toggleReleaseIssuesModal(true);
                        }}
                      >
                        <InfoIcon description="What is status?" />
                      </a>
                    </th>
                    <th className="govuk-table__cell--numeric">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>Academic year 2021/22 (Not live)</td>
                    <td>
                      <Tag>Draft</Tag>
                    </td>
                    <td>3</td>
                    <td>3</td>
                    <td>3</td>
                    <td className="govuk-table__cell--numeric">
                      <Link to="/prototypes/admin-release-summary">
                        Edit{' '}
                        <span className="govuk-visually-hidden">
                          Academic year 2021/22
                        </span>
                      </Link>
                    </td>
                  </tr>
                  <tr>
                    <td>Academic year 2020/21 (Not live)</td>
                    <td>
                      <Tag>in review</Tag>
                    </td>
                    <td>0</td>
                    <td>1</td>
                    <td>0</td>
                    <td className="govuk-table__cell--numeric">
                      <Link to="/prototypes/admin-release-summary">
                        Edit{' '}
                        <span className="govuk-visually-hidden">
                          Academic year 2020/21 (Not live)
                        </span>
                      </Link>
                    </td>
                  </tr>
                  <tr>
                    <td>Academic year 2019/20 (Not live)</td>
                    <td>
                      <Tag>Amendment</Tag>
                    </td>
                    <td>0</td>
                    <td>1</td>
                    <td>0</td>
                    <td className="govuk-table__cell--numeric">
                      <Link to="/prototypes/admin-release-summary">
                        Edit{' '}
                        <span className="govuk-visually-hidden">
                          Academic year 2020/21 (Not live)
                        </span>
                      </Link>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          )}

          <Modal
            open={showHelpStatusModal}
            title="Draft status guidance"
            className="govuk-!-width-one-half"
          >
            <ModalContent contentType="helpStatusModal" />
            <Button
              onClick={() => {
                toggleHelpStatusModal(false);
              }}
            >
              Close
            </Button>
          </Modal>

          <Modal
            open={showReleaseIssuesModal}
            title="Issues guidance"
            className="govuk-!-width-one-half"
          >
            <ModalContent contentType="releaseIssuesModal" />
            <Button
              onClick={() => {
                toggleReleaseIssuesModal(false);
              }}
            >
              Close
            </Button>
          </Modal>

          <Modal
            open={showHelpStatusPublishedModal}
            title="Published status guidance"
            className="govuk-!-width-one-half"
          >
            <ModalContent contentType="helpPublishedModal" />
            <Button
              onClick={() => {
                toggleHelpStatusPublishedModal(false);
              }}
            >
              Close
            </Button>
          </Modal>

          <div style={{ width: '100%', overflow: 'auto' }}>
            <table className="govuk-table govuk-!-margin-bottom-9">
              <caption className="govuk-table__caption--m">
                Published releases ({showMore ? '10' : '5'} of 10)
              </caption>
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th style={{ width: '38%' }}>Release period</th>
                  <th style={{ width: '15%' }}>
                    State
                    <a
                      href="#"
                      className="govuk-!-margin-left-1"
                      onClick={() => {
                        toggleHelpStatusPublishedModal(true);
                      }}
                    >
                      <InfoIcon description="What is status?" />
                    </a>
                  </th>
                  <th style={{ width: '35%' }}>Published date</th>
                  <th
                    style={{ width: '120px' }}
                    colSpan={2}
                    className="dfe-align--centre"
                  >
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Academic year 2019/20 (Live - Latest release)</td>
                  <td>
                    <Tag colour="green">Published</Tag>
                  </td>
                  <td>25 September 2020</td>
                  <td>
                    <a href="#">Amend</a>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">View</a>
                  </td>
                </tr>
                <tr>
                  <td>Academic year 2018/19 (Live)</td>
                  <td>
                    <Tag colour="green">Published</Tag>
                  </td>
                  <td>25 September 2019</td>
                  <td>
                    <a href="#">Amend</a>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">View</a>
                  </td>
                </tr>
                <tr>
                  <td>Academic year 2017/18 (Live)</td>
                  <td>
                    <Tag colour="green">Published</Tag>
                  </td>
                  <td>25 September 2018</td>
                  <td>
                    <a href="#">Amend</a>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">View</a>
                  </td>
                </tr>
                <tr>
                  <td>Academic year 2015/16 (Live)</td>
                  <td>
                    <Tag colour="green">Published</Tag>
                  </td>
                  <td>25 September 2016</td>
                  <td>
                    <a href="#">Amend</a>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">View</a>
                  </td>
                </tr>
                <tr>
                  <td>Academic year 2014/15 (Live)</td>
                  <td>
                    <Tag colour="green">Published</Tag>
                  </td>
                  <td>25 September 2015</td>
                  <td>
                    <a href="#">Amend</a>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">View</a>
                  </td>
                </tr>
                {showMore && (
                  <>
                    <tr>
                      <td>Academic year 2013/14 (Live)</td>
                      <td>
                        <Tag colour="green">Published</Tag>
                      </td>

                      <td>25 September 2014</td>
                      <td>
                        <a href="#">Amend</a>
                      </td>
                      <td className="govuk-table__cell--numeric">
                        <a href="#">View</a>
                      </td>
                    </tr>
                    <tr>
                      <td>Academic year 2016/17 (Live)</td>
                      <td>
                        <Tag colour="green">Published</Tag>
                      </td>

                      <td>25 September 2017</td>
                      <td>
                        <a href="#">Amend</a>
                      </td>
                      <td className="govuk-table__cell--numeric">
                        <a href="#">View</a>
                      </td>
                    </tr>
                    <tr>
                      <td>Academic year 2015/16 (Live)</td>
                      <td>
                        <Tag colour="green">Published</Tag>
                      </td>

                      <td>25 September 2016</td>
                      <td>
                        <a href="#">Amend</a>
                      </td>
                      <td className="govuk-table__cell--numeric">
                        <a href="#">View</a>
                      </td>
                    </tr>
                    <tr>
                      <td>Academic year 2014/15 (Live)</td>
                      <td>
                        <Tag colour="green">Published</Tag>
                      </td>

                      <td>25 September 2015</td>
                      <td>
                        <a href="#">Amend</a>
                      </td>
                      <td className="govuk-table__cell--numeric">
                        <a href="#">View</a>
                      </td>
                    </tr>
                    <tr>
                      <td>Academic year 2013/14 (Live)</td>
                      <td>
                        <Tag colour="green">Published</Tag>
                      </td>

                      <td>25 September 2014</td>
                      <td>
                        <a href="#">Amend</a>
                      </td>
                      <td className="govuk-table__cell--numeric">
                        <a href="#">View</a>
                      </td>
                    </tr>
                  </>
                )}
              </tbody>
            </table>

            {!showMore ? (
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setShowMore(true);
                }}
              >
                Show next 5 published releases
              </a>
            ) : (
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setShowMore(false);
                }}
              >
                Show next 5 published releases
              </a>
            )}
          </div>
        </>
      )}
      <div className="dfe-align--right govuk-!-margin-top-9">
        <ul className="govuk-list">
          <li>
            {showScheduled ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowScheduled(false);
                }}
              >
                Remove scheduled
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowScheduled(true);
                }}
              >
                Show scheduled
              </a>
            )}
          </li>
          <li>
            {showDraft ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowDraft(false);
                }}
              >
                Remove draft releases
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowDraft(true);
                }}
              >
                Show draft releases
              </a>
            )}
          </li>
          <li>
            {showReleases ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowReleases(false);
                }}
              >
                Remove releases
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowReleases(true);
                }}
              >
                Show releases
              </a>
            )}
          </li>
        </ul>
      </div>
    </>
  );
};

export default PrototypePublicationReleaseList;
