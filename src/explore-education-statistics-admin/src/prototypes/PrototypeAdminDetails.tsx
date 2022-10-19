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
          <h3 className="govuk-heading-l">Publication details</h3>
          <SummaryList>
            <SummaryListItem term="Publiction title">
              Pupil absence in schools in England
            </SummaryListItem>
            <SummaryListItem term="Theme">Pupil and schools</SummaryListItem>
            <SummaryListItem term="Topic">Pupil absence</SummaryListItem>
          </SummaryList>
          <div>
            <Button
              onClick={e => {
                e.preventDefault();
                setShowForm(true);
              }}
              variant="secondary"
            >
              Edit publication details
            </Button>
          </div>
        </>
      )}

      {showForm && (
        <>
          <FormFieldset
            id="details"
            legend="Publication details"
            legendSize="l"
          >
            <FormGroup>
              <FormTextInput
                id="pubTitle"
                name="pubTitle"
                label="Publication title"
                className="govuk-!-width-one-half"
                value="Pupil absence in schools in England"
              />
            </FormGroup>
            <FormGroup>
              <label htmlFor="theme" className="govuk-label">
                Select theme
              </label>
              <select
                name="theme"
                id="theme"
                className="govuk-select govuk-!-width-one-half"
              >
                <option value="">Pupils and schools</option>
              </select>
            </FormGroup>
            <FormGroup>
              <label htmlFor="theme" className="govuk-label">
                Select topic
              </label>
              <select
                name="topic"
                id="topic"
                className="govuk-select govuk-!-width-one-half"
              >
                <option value="">Pupils and schools</option>
              </select>
            </FormGroup>
          </FormFieldset>
          <ButtonGroup>
            <Button
              onClick={e => {
                e.preventDefault();
                setShowForm(false);
              }}
            >
              Update publication details
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
