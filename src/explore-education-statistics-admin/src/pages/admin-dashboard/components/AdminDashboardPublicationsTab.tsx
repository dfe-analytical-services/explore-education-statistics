import publicationRoutes from '@admin/routes/edit-publication/routes';
import { AdminDashboardPublication } from '@admin/services/dashboard/types';
import FormSelect from '@common/components/form/FormSelect';
import React from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import { IdTitlePair } from '@admin/services/common/types';
import Link from '@admin/components/Link';
import AdminDashboardPublicationSummary from './AdminDashboardPublicationSummary';

interface AdminDashboardPublicationsTabProps {
  publications: AdminDashboardPublication[];
  noResultsMessage: string;
  themes: IdTitlePair[];
  topics: IdTitlePair[];
  selectedThemeId: string;
  selectedTopicId: string;
  onThemeChange: (selectedThemeId: string) => void;
  onTopicChange: (selectedTopicId: string) => void;
}

const AdminDashboardPublicationsTab = ({
  publications,
  noResultsMessage,
  themes,
  topics,
  selectedThemeId,
  selectedTopicId,
  onThemeChange,
  onTopicChange,
}: AdminDashboardPublicationsTabProps) => {
  const selectedTheme =
    themes.find(theme => theme.id === selectedThemeId) || themes[0];
  const selectedTopic =
    topics.find(topic => topic.id === selectedTopicId) || topics[0];

  return (
    <section>
      <p className="govuk-body">Select publications to:</p>
      <ul className="govuk-list--bullet">
        <li>create new releases and methodologies</li>
        <li>edit exiting releases and methodologies</li>
        <li>view and sign-off releases and methodologies</li>
      </ul>
      <p className="govuk-bo">
        To remove publications, releases and methodologies email{' '}
        <a href="mailto:explore.statistics@education.gov.uk">
          explore.statistics@education.gov.uk
        </a>
      </p>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <FormSelect
            id="selectTheme"
            label="Select theme"
            name="selectTheme"
            options={themes.map(theme => ({
              label: theme.title,
              value: theme.id,
            }))}
            value={selectedThemeId}
            onChange={event => {
              onThemeChange(event.target.value);
            }}
          />
        </div>
        <div className="govuk-grid-column-one-half">
          <FormSelect
            id="selectTopic"
            label="Select topic"
            name="selectTopic"
            options={topics.map(topic => ({
              label: topic.title,
              value: topic.id,
            }))}
            value={selectedTopicId}
            onChange={event => {
              onTopicChange(event.target.value);
            }}
          />
        </div>
      </div>
      <hr />
      <h2>{selectedTheme.title}</h2>
      <h3>{selectedTopic.title}</h3>
      {publications.length > 0 && (
        <Accordion id="publications">
          {publications.map(publication => (
            <AccordionSection
              key={publication.id}
              heading={publication.title}
              headingTag="h3"
            >
              <AdminDashboardPublicationSummary publication={publication} />
            </AccordionSection>
          ))}
        </Accordion>
      )}
      {publications.length === 0 && (
        <div className="govuk-inset-text">{noResultsMessage}</div>
      )}
      <Link
        to={publicationRoutes.createPublication.generateLink(selectedTopicId)}
        className="govuk-button"
      >
        Create new publication
      </Link>
    </section>
  );
};

export default AdminDashboardPublicationsTab;
