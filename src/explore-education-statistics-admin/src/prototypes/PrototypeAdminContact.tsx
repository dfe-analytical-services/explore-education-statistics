import PageTitle from '@admin/components/PageTitle';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Nav from '@admin/prototypes/components/PrototypeNavBarPublication';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import {
  FormGroup,
  FormTextInput,
  FormFieldset,
} from '@common/components/form';

const PrototypeManagePublication = () => {
  const [showForm, setShowForm] = useState(false);

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { name: 'Dashboard', link: '/prototypes/admin-dashboard' },
        { name: 'Manage publication', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            title="Pupil absence in schools in England"
            caption="Manage publication"
          />
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

      <Nav />

      {!showForm && (
        <>
          <h3 className="govuk-heading-l">Contact for this publication</h3>
          <p className="govuk-hint">
            They will be the main point of contact for data and methodology
            enquiries for this publication and its releases.
          </p>
          <SummaryList>
            <SummaryListItem term="Team">Prototyping team</SummaryListItem>
            <SummaryListItem term="Team email">
              <a href="mailto:#">ProtTeam@education.gov.uk</a>
            </SummaryListItem>
            <SummaryListItem term="Contact name">John Smith</SummaryListItem>
            <SummaryListItem term="Contact telephone">
              01234 0567891
            </SummaryListItem>
          </SummaryList>
          <div>
            <Button
              onClick={e => {
                e.preventDefault();
                setShowForm(true);
              }}
              variant="secondary"
            >
              Edit contact details
            </Button>
          </div>
        </>
      )}

      {showForm && (
        <>
          <FormFieldset
            id="contact"
            legend="Contact for this publication"
            legendSize="l"
            hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
          >
            <FormGroup>
              <FormTextInput
                id="team"
                name="team"
                label="Team name"
                className="govuk-!-width-one-half"
                value="Prototyping team"
              />
            </FormGroup>
            <FormGroup>
              <FormTextInput
                id="teamEmail"
                name="teamEmail"
                label="Team email"
                className="govuk-!-width-one-half"
                value="ProtTeam@education.gov.uk"
              />
            </FormGroup>
            <FormGroup>
              <FormTextInput
                id="teamContact"
                name="teamContact"
                label="Contact name"
                className="govuk-!-width-one-half"
                value="John Smith"
              />
            </FormGroup>
            <FormGroup>
              <FormTextInput
                id="teamPhone"
                name="teamPhone"
                label="Contact telephone"
                className="govuk-!-width-one-half"
                value="01234 567891"
              />
            </FormGroup>
          </FormFieldset>
          <ButtonGroup>
            <Button
              onClick={e => {
                e.preventDefault();
                setShowForm(false);
              }}
            >
              Update contact details
            </Button>
            <a
              href="#"
              className="govuk-link govuk-link--no-visited-state"
              onClick={e => {
                e.preventDefault();
                setShowForm(false);
              }}
            >
              Cancel
            </a>
          </ButtonGroup>
        </>
      )}
    </PrototypePage>
  );
};

export default PrototypeManagePublication;
