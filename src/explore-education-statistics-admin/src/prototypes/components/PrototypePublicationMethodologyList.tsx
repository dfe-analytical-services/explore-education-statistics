import React, { useState } from 'react';
import Button from '@common/components/Button';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ModalContent from '@admin/prototypes/components/PrototypeModalContent';

const PrototypePublicationMethodologyList = () => {
  const [showMethodology, setShowMethodology] = useState(true);
  const [showHelpTypeModal, toggleHelpTypeModal] = useToggle(false);
  const [showHelpStatusModal, toggleHelpStatusModal] = useToggle(false);

  const dfeLinkWarning = {
    color: '#d4351c',
  };

  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters govuk-!-margin-bottom-6">
          <h3 className="govuk-heading-l">Manage methodology</h3>
          <p className="govuk-hint">
            Create new methodology, view or amend existing methodology, select
            existing methodology used in another publication or link to an
            external file that contains methodology details.
          </p>
        </div>
        <div className="govuk-grid-column-one-quarter" />
      </div>
      {!showMethodology && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <WarningMessage className="govuk-!-margin-bottom-2">
              No releases created in this publication
            </WarningMessage>
          </div>
          <div className="govuk-grid-column-one-quarter dfe-align--right govuk-!-margin-top-3">
            <Button>Create new methodology</Button>
          </div>
        </div>
      )}

      {showMethodology && (
        <div style={{ width: '100%', overflow: 'auto' }}>
          <table className="govuk-table">
            <caption className="govuk-table__caption--m">
              Methodologies associated to releases in this publication
            </caption>
            <thead className="govuk-table__head">
              <tr className="govuk-table__row">
                <th>Methodology</th>
                <th>
                  Type
                  <a
                    href="#"
                    className="govuk-!-margin-left-1"
                    onClick={() => {
                      toggleHelpTypeModal(true);
                    }}
                  >
                    <InfoIcon description="What is type?" />
                  </a>
                </th>
                <th>
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
                <th>Publish date</th>
                <th colSpan={3}>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Pupil absence statistics</td>
                <td>Owned</td>
                <td>
                  <Tag>Approved</Tag>
                </td>
                <td>Not yet published</td>
                <td className="dfe-align--left">
                  <a href="#">Amend</a>
                </td>
                <td className="dfe-align--centre">
                  <a href="#">View</a>
                </td>
                <td />
              </tr>
              <tr>
                <td>Pupil exclusions statistics</td>
                <td>Adopted</td>
                <td>
                  <Tag colour="green">Published</Tag>
                </td>
                <td>28 March 2021</td>
                <td className="dfe-align--left">
                  <a href="#">Amend</a>
                </td>
                <td className="dfe-align--centre">
                  <a href="#">View</a>
                </td>
                <td className="dfe-align--right">
                  <a href="#" style={dfeLinkWarning}>
                    Remove
                  </a>
                </td>
              </tr>
            </tbody>
          </table>
          <Modal
            open={showHelpTypeModal}
            title="Methodology type guidance"
            className="govuk-!-width-one-half"
          >
            <p>
              Various types of methodology can be associated to this publication
            </p>
            <SummaryList>
              <SummaryListItem term="Owned">
                This is a methodology that belongs to and was created
                specifically for this publication
              </SummaryListItem>
              <SummaryListItem term="Adopted">
                This is a methodology that is owned by another publication, but
                can be selected to be shown with this publication
              </SummaryListItem>
              <SummaryListItem term="External">
                This is a link to an existing methodolgy that is hosted
                externally
              </SummaryListItem>
            </SummaryList>
            <Button
              onClick={() => {
                toggleHelpTypeModal(false);
              }}
            >
              Close
            </Button>
          </Modal>
          <Modal
            open={showHelpStatusModal}
            title="Methodology status guidance"
            className="govuk-!-width-one-half"
          >
            <ModalContent contentType="methodologyStatusModal" />
            <Button
              onClick={() => {
                toggleHelpStatusModal(false);
              }}
            >
              Close
            </Button>
          </Modal>
        </div>
      )}
      <h4>Other options</h4>

      <a href="#">Add external methodology</a>
      <p className="govuk-hint">
        This is a link to an existing methodolgy that is hosted externally
      </p>

      <a href="#">Adopt an existing methodology</a>
      <p className="govuk-hint">
        This is a methodology that is owned by another publication
      </p>

      <div className="dfe-align--right govuk-!-margin-top-9">
        <ul className="govuk-list">
          <li>
            {showMethodology ? (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowMethodology(false);
                }}
              >
                Remove methodology
              </a>
            ) : (
              <a
                href="#"
                className="govuk-body-s"
                onClick={e => {
                  e.preventDefault();
                  setShowMethodology(true);
                }}
              >
                Show methodology
              </a>
            )}
          </li>
        </ul>
      </div>
    </>
  );
};

export default PrototypePublicationMethodologyList;
