import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import SubjectForm, {
  SubjectFormSubmitHandler,
} from '@common/modules/table-tool/components/SubjectForm';
import FeaturedTablesList from '@common/modules/table-tool/components/FeaturedTablesList';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { Subject, FeaturedTable } from '@common/services/tableBuilderService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { ReactNode } from 'react';

const subjectTabsId = 'subjectTabs';
const subjectTabIds = {
  featuredTables: `${subjectTabsId}-featuredTables`,
  createTable: `${subjectTabsId}-createTable`,
};

interface Props extends InjectedWizardProps {
  featuredTables?: FeaturedTable[];
  loadingFastTrack?: boolean;
  subjects: Subject[];
  subjectId?: string;
  renderFeaturedTable?: (featuredTable: FeaturedTable) => ReactNode;
  onSubmit: SubjectFormSubmitHandler;
}

const SubjectStep = ({
  featuredTables = [],
  loadingFastTrack = false,
  subjects,
  subjectId = '',
  renderFeaturedTable,
  onSubmit,
  ...stepProps
}: Props) => {
  const { isActive } = stepProps;

  const hasFeaturedTables = renderFeaturedTable && featuredTables.length > 0;

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading={!hasFeaturedTables}>
      {hasFeaturedTables
        ? 'View a featured table or create your own'
        : 'Choose a subject'}
    </WizardStepHeading>
  );

  if (isActive) {
    const form = (
      <SubjectForm
        {...stepProps}
        initialValues={{
          subjectId: subjects.length === 1 ? subjects[0].id : subjectId,
        }}
        options={subjects}
        onSubmit={onSubmit}
        legendSize={hasFeaturedTables ? 'm' : 'l'}
        legend={
          hasFeaturedTables ? (
            <h3 className="govuk-fieldset__heading">Choose a subject</h3>
          ) : (
            stepHeading
          )
        }
        legendHint="Choose a subject to create your table from, more information on the data coverage can be found by viewing more details"
        hasFeaturedTables={hasFeaturedTables}
      />
    );

    return hasFeaturedTables ? (
      <>
        {stepHeading}

        <Tabs id={subjectTabsId} modifyHash={false}>
          <TabsSection
            title="Featured tables"
            id={subjectTabIds.featuredTables}
            headingTitle="Choose a table"
          >
            <LoadingSpinner loading={loadingFastTrack}>
              <span className="govuk-hint">
                Use the links below to quickly select existing featured tables
                for this publication. After viewing a table you can also adjust
                and change filters to quickly see different results.
              </span>

              {renderFeaturedTable && (
                <FeaturedTablesList
                  featuredTables={featuredTables}
                  renderLink={renderFeaturedTable}
                />
              )}

              <p>
                If you can't find the table you're looking for, then you can{' '}
              </p>
              <a
                className="govuk-button"
                href={`#${subjectTabIds.createTable}`}
              >
                Create your own table
              </a>
            </LoadingSpinner>
          </TabsSection>
          <TabsSection
            title="Create your own table"
            id={subjectTabIds.createTable}
          >
            {form}
          </TabsSection>
        </Tabs>
      </>
    ) : (
      form
    );
  }

  const subjectName =
    subjects.find(({ id }) => subjectId === id)?.name ?? 'None';

  return (
    <WizardStepSummary {...stepProps} goToButtonText="Change subject">
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term="Subject">{subjectName}</SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
};

export default SubjectStep;
