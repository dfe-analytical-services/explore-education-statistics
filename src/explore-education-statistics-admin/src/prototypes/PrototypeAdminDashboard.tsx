import PageTitle from '@admin/components/PageTitle';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import Tag from '@common/components/Tag';
import TabsSection from '@common/components/TabsSection';
import RelatedInformation from '@common/components/RelatedInformation';
import Button from '@common/components/Button';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import ModalContent from '@admin/prototypes/components/PrototypeModalContent';

const PrototypeManageUsers = () => {
  const [showCreatePub, setShowCreatePub] = useState(true);
  const [showBau, setShowBau] = useState(false);
  const [showHelpStatusModal, toggleHelpStatusModal] = useToggle(false);
  const [showHelpIssuesModal, toggleHelpIssuesModal] = useToggle(false);
  const [showScheduledStatusModal, toggleScheduledStatusModal] =
    useToggle(false);
  const [showScheduledStagesModal, toggleScheduledStagesModal] =
    useToggle(false);

  return (
    <PrototypePage
      wide
      breadcrumbs={[{ name: 'Dashboard', link: '/dashboard' }]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle title="Dashboard" caption="Welcome Bau1" />
          <p className="govuk-body-s">
            Logged in as <strong>Bau1</strong>. Not you?{' '}
            <a className="govuk-link govuk-link" href="/authentication/logout">
              Sign out
            </a>
          </p>
          <p className="govuk-hint govuk-!-margin-bottom-6">
            This is your administration dashboard, here you can manage
            publications, releases and methodologies.
          </p>
          {showBau && (
            <ul className="govuk-!-margin-bottom-6">
              <li>
                <a href="#">manage themes and topics</a>
              </li>
            </ul>
          )}
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/contact-us" target="_blank">
                  Contact us
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <Tabs id="manage-release-users">
        <TabsSection title="Your publications (5)">
          <h2>View and manage your publications</h2>
          <p>Select a publication to:</p>
          <ul className="govuk-list--bullet">
            <li>create new releases and methodologies</li>
            <li>edit existing releases and methodologies</li>
            <li>view and sign-off releases and methodologies</li>
          </ul>
          {showBau && (
            <div className="govuk-form-group">
              <div className="dfe-flex dfe-flex-wrap">
                <div className="govuk-!-margin-right-4 govuk-!-width-one-third">
                  <label htmlFor="theme" className="govuk-label">
                    Select theme
                  </label>
                  <select
                    name="theme"
                    id="theme"
                    className="govuk-select govuk-!-width-full"
                  >
                    <option value="">Pupils and schools</option>
                  </select>
                </div>
                <div className="govuk-!-width-one-third">
                  <label htmlFor="topic" className="govuk-label">
                    Select topic
                  </label>
                  <select
                    name="topic"
                    id="topic"
                    className="govuk-select govuk-select govuk-!-width-full"
                  >
                    <option value="">Pupil absence</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {!showBau && (
            <>
              <hr />
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <h3>Pupils and schools / exclusions</h3>
                  <ul className="govuk-list">
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Permanent and fixed-period exclusions in England
                      </Link>
                    </li>
                  </ul>
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <div className="dfe-align--right">
                    {showCreatePub && (
                      <a href="#" className="govuk-button">
                        Create new publication
                      </a>
                    )}
                  </div>
                </div>
              </div>
            </>
          )}

          <hr />
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
              <h3>Pupils and schools / pupil absence</h3>
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/admin-publication">
                    Pupil absence in schools in England
                  </Link>
                </li>
                <li>
                  <Link to="/prototypes/admin-publication">
                    Pupil absence in schools in England: autumn and spring
                  </Link>
                </li>
                <li>
                  <Link to="/prototypes/admin-publication">
                    Pupil absence in schools in England: autumn term
                  </Link>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-one-quarter">
              <div className="dfe-align--right">
                {showCreatePub && (
                  <a href="#" className="govuk-button">
                    Create new publication
                  </a>
                )}
              </div>
            </div>
          </div>

          {!showBau && (
            <>
              <hr />
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <h3>Pupils and schools / school applications</h3>
                  <ul className="govuk-list">
                    <li>
                      <Link to="/prototypes/admin-publication">
                        Secondary and primary school applications and offers
                      </Link>
                    </li>
                  </ul>
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <div className="dfe-align--right">
                    {showCreatePub && (
                      <a href="#" className="govuk-button">
                        Create new publication
                      </a>
                    )}
                  </div>
                </div>
              </div>
            </>
          )}
        </TabsSection>
        <TabsSection title="Draft releases (4)">
          <h2>Draft releases</h2>
          <p className="govuk-hint govuk-!-width-three-quarters">
            Here you can view and edit any of your releases that are currently
            in 'Draft' or 'In review' and also 'Amendments' that are being made
            to a published release. You can also view a summary of any
            outstanding issues that may need to be resolved.
          </p>
          <div style={{ width: '100%', overflow: 'auto' }}>
            <table className="govuk-table">
              <caption className="govuk-visually-hidden">
                Table showing your draft releases
              </caption>
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th style={{ width: '35%' }}>Publication / Release period</th>
                  <th style={{ width: '8%' }}>
                    Status
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
                  <th style={{ width: '47%' }}>
                    Issues
                    <a
                      href="#"
                      className="govuk-!-margin-left-1"
                      onClick={() => {
                        toggleHelpIssuesModal(true);
                      }}
                    >
                      <InfoIcon description="What are issues?" />
                    </a>
                  </th>
                  <th className="govuk-table__cell--numeric">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England
                  </th>
                </tr>
                <tr>
                  <td>Academic year 2020/21 (Not live)</td>
                  <td style={{ width: '12%' }}>
                    <Tag>Draft</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (9)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex dfe-justify-content--space-between">
                        <li>
                          <Tag colour="red">3 Errors</Tag>
                        </li>
                        <li>
                          <Tag colour="yellow">3 Warnings</Tag>
                        </li>
                        <li>
                          <Tag colour="grey">3 Unresolved comments</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
                <tr>
                  <td>Academic year 2019/20 (Not live)</td>
                  <td>
                    <Tag>In review</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (1)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex dfe-justify-content--space-between">
                        <li>
                          <Tag colour="yellow">1 Warning</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England: autumn and spring
                  </th>
                </tr>
                <tr>
                  <td>Academic year 2020/21 (Not live)</td>
                  <td>
                    <Tag>Draft</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (9)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex dfe-justify-content--space-between">
                        <li>
                          <Tag colour="red">3 Errors</Tag>
                        </li>
                        <li>
                          <Tag colour="yellow">3 Warnings</Tag>
                        </li>
                        <li>
                          <Tag colour="grey">3 Unresolved comments</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-9">
                    Pupil absence in schools in England: autumn term
                  </th>
                </tr>
                <tr>
                  <td>Academic year 2020/21 (Not live)</td>
                  <td>
                    <Tag>Amendment</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View issues (3)"
                      className="govuk-!-margin-bottom-0"
                    >
                      <ul className="govuk-list dfe-flex">
                        <li>
                          <Tag colour="yellow">3 Warnings</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td className="govuk-table__cell--numeric">
                    <Link to="/prototypes/admin-release-summary">
                      Edit{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20, Pupil absence in schools in
                        England
                      </span>
                    </Link>
                  </td>
                  <td />
                </tr>
              </tbody>
            </table>
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
              open={showHelpIssuesModal}
              title="Issues guidance"
              className="govuk-!-width-one-half"
            >
              <ModalContent contentType="helpIssuesModal" />
              <Button
                onClick={() => {
                  toggleHelpIssuesModal(false);
                }}
              >
                Close
              </Button>
            </Modal>
          </div>
        </TabsSection>
        <TabsSection title="Approved scheduled releases (3)">
          <h2>Approved scheduled releases</h2>
          <p className="govuk-hint govuk-!-width-three-quarters govuk-!-margin-bottom-6">
            Here you can view releases that have been approved and are now
            scheduled for publication. You can also check the progress of any
            releases that are currently being published to live.
          </p>
          <div style={{ width: '100%', overflow: 'auto' }}>
            <table className="govuk-table">
              <caption className="govuk-visually-hidden">
                View approved scheduled releases
              </caption>
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th style={{ width: '38%' }}>Publication / Release period</th>
                  <th>
                    Status
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
                  <th style={{ width: '315px' }}>
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
                  <th>Scheduled publish date</th>
                  <th colSpan={2} className="govuk-table__cell--numeric">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <th colSpan={7} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England
                  </th>
                </tr>
                <tr>
                  <td>
                    Academic year 2020/21 (Not live) <Tag>Amendment</Tag>
                  </td>
                  <td>
                    <Tag colour="orange">Validating</Tag>
                  </td>
                  <td>-</td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>

                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England: autumn and spring
                  </th>
                </tr>
                <tr>
                  <td>Academic year 2020/21 (Not live)</td>
                  <td>
                    <Tag colour="blue">Scheduled</Tag>
                  </td>
                  <td>-</td>
                  <td>20 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>

                <tr>
                  <th colSpan={6} scope="col" className="govuk-!-padding-top-6">
                    Pupil absence in schools in England: autumn term
                  </th>
                </tr>
                <tr>
                  <td>Academic year 2018/19 (Not live)</td>
                  <td>
                    <Tag colour="orange">STARTED</Tag>
                  </td>
                  <td>
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
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>
                <tr>
                  <td>Academic year 2019/20 (Not live)</td>
                  <td>
                    <Tag colour="green">COMPLETE</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View stages"
                      className="govuk-!-margin-bottom-0"
                    >
                      <h4>Publishing complete</h4>
                      <ul className="govuk-list">
                        <li>
                          <Tag colour="green">Data complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Content complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Files complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Publishing complete ✓</Tag>
                        </li>
                      </ul>
                    </Details>
                  </td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
                </tr>
                <tr>
                  <td>Academic year 2021/22 (Not live)</td>
                  <td>
                    <Tag colour="red">FAILED</Tag>
                  </td>
                  <td>
                    <Details
                      summary="View stages"
                      className="govuk-!-margin-bottom-0"
                    >
                      <h4>Publishing cancelled</h4>
                      <ul className="govuk-list">
                        <li>
                          <Tag colour="red">Data failed ✖</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Content complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="green">Files complete ✓</Tag>
                        </li>
                        <li>
                          <Tag colour="red">Publishing cancelled ✖</Tag>
                        </li>
                      </ul>
                      <h5 className="govuk-!-margin-0">Help and guidance</h5>
                      <p className="govuk-body-s">
                        For extra help and guidance to help rectify this issue
                        please email:
                        <br />
                        <a href="#">explore.statistics@education.gov.uk</a>
                      </p>
                    </Details>
                  </td>
                  <td>10 January 2022</td>
                  <td className="govuk-table__cell--numeric">
                    <a href="#">
                      View{' '}
                      <span className="govuk-visually-hidden">
                        Academic year 2019/20 (Not live)
                      </span>
                    </a>
                  </td>
                  <td />
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
        </TabsSection>
      </Tabs>
      <div className="dfe-align--right govuk-!-margin-top-9">
        <ul className="govuk-list">
          <li>
            {showCreatePub ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowCreatePub(false);
                }}
              >
                Remove create role
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowCreatePub(true);
                }}
              >
                Add create role
              </a>
            )}
          </li>
          <li>
            {showBau ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowBau(false);
                }}
              >
                Remove BAU role
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowBau(true);
                }}
              >
                Add BAU role
              </a>
            )}
          </li>
        </ul>
      </div>
    </PrototypePage>
  );
};

export default PrototypeManageUsers;
