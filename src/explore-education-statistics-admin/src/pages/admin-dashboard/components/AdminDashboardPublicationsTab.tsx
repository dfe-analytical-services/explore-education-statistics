import { AdminDashboardPublication } from '@admin/services/dashboard/types';
import FormSelect from '@common/components/form/FormSelect';
import React from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import { IdLabelPair } from '@admin/services/common/types';
import Link from '@admin/components/Link';
import AdminDashboardPublicationSummary from './AdminDashboardPublicationSummary';

interface AdminDashboardPublicationsTabProps {
  publications: AdminDashboardPublication[];
  noResultsMessage: string;
  themes: IdLabelPair[];
  topics: IdLabelPair[];
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
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
        {`${selectedTheme.label}, ${selectedTopic.label}`}
      </h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <FormSelect
            id="selectTheme"
            label="Select theme"
            name="selectTheme"
            options={themes.map(theme => ({
              label: theme.label,
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
              label: topic.label,
              value: topic.id,
            }))}
            value={selectedTopicId}
            onChange={event => {
              onTopicChange(event.target.value);
            }}
          />
        </div>
      </div>
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
      <Link to={`/topic/${selectedTopicId}/publication/create`} className="govuk-button">
        Create new publication
      </Link>
    </section>
  );
};

export default AdminDashboardPublicationsTab;
