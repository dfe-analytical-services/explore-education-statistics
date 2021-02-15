import InsetText from '@common/components/InsetText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import SubjectForm, {
  SubjectFormSubmitHandler,
} from '@common/modules/table-tool/components/SubjectForm';
import TableHighlightsList from '@common/modules/table-tool/components/TableHighlightsList';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { Subject, TableHighlight } from '@common/services/tableBuilderService';
import React, { ReactNode } from 'react';

const subjectTabsId = 'subjectTabs';
const subjectTabIds = {
  popularTables: `${subjectTabsId}-popularTables`,
  createTable: `${subjectTabsId}-createTable`,
};

interface Props {
  highlights?: TableHighlight[];
  subjects: Subject[];
  subjectId?: string;
  renderHighlightLink?: (highlight: TableHighlight) => ReactNode;
  onSubmit: SubjectFormSubmitHandler;
}

const SubjectStep = ({
  highlights = [],
  subjects,
  subjectId = '',
  renderHighlightLink,
  onSubmit,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const { isActive } = stepProps;

  const hasHighlights = renderHighlightLink && highlights.length > 0;

  const heading = (
    <WizardStepHeading {...stepProps} fieldsetHeading={!hasHighlights}>
      {hasHighlights
        ? 'View a popular table or create your own'
        : 'Choose a subject'}
    </WizardStepHeading>
  );

  if (isActive) {
    const form = (
      <SubjectForm
        {...stepProps}
        initialValues={{
          subjectId,
        }}
        options={subjects}
        onSubmit={onSubmit}
        legendSize={hasHighlights ? 'm' : 'l'}
        legend={
          hasHighlights ? (
            <h3 className="govuk-fieldset__heading">Choose a subject</h3>
          ) : (
            heading
          )
        }
      />
    );

    return hasHighlights ? (
      <>
        {heading}

        <Tabs id={subjectTabsId}>
          <TabsSection
            title="Popular tables"
            id={subjectTabIds.popularTables}
            headingTitle="Choose a table"
          >
            <InsetText>
              Use the links below to quickly select existing popular tables for
              this publication. After viewing a table you can also adjust and
              change filters to quickly see different results.
            </InsetText>

            {renderHighlightLink && (
              <TableHighlightsList
                highlights={highlights}
                renderLink={renderHighlightLink}
              />
            )}

            <p>
              If you can't find the table you're looking for, then you can{' '}
              <a href={`#${subjectTabIds.createTable}`}>create your own</a>.
            </p>
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
    <>
      {heading}

      <SummaryList noBorder>
        <SummaryListItem term="Subject">{subjectName}</SummaryListItem>
      </SummaryList>
    </>
  );
};

export default SubjectStep;
