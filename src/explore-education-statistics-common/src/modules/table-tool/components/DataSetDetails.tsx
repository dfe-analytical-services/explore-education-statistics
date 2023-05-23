import ChevronCard from '@common/components/ChevronCard';
import ChevronGrid from '@common/components/ChevronGrid';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import DataSetDetailsList from '@common/modules/table-tool/components/DataSetDetailsList';
import { SelectedRelease } from '@common/modules/table-tool/types/selectedPublication';
import ButtonText from '@common/components/ButtonText';
import downloadService from '@common/services/downloadService';
import { FeaturedTable, Subject } from '@common/services/tableBuilderService';
import React, { ReactNode } from 'react';

interface Props extends InjectedWizardProps {
  featuredTables?: FeaturedTable[];
  isSubmitting: boolean;
  renderFeaturedTableLink?: (featuredTable: FeaturedTable) => ReactNode;
  release?: SelectedRelease;
  subject: Subject;
}

export default function DataSetDetails({
  featuredTables = [],
  isSubmitting,
  renderFeaturedTableLink,
  release,
  subject,
  ...stepProps
}: Props) {
  return (
    <>
      <h3>Data set details</h3>
      <DataSetDetailsList subject={subject} />

      <hr />

      <h4 className="govuk-heading-m">What would you like to do?</h4>
      <WizardStepFormActions
        {...stepProps}
        additionalButton={
          release && (
            <>
              <span className="govuk-!-margin-left-3">or</span>
              <ButtonText
                className="govuk-!-margin-bottom-0 govuk-!-margin-left-2"
                onClick={async () => {
                  await downloadService.downloadFiles(release?.id, [
                    subject.file.id,
                  ]);
                }}
              >
                Download full data set (ZIP)
              </ButtonText>
            </>
          )
        }
        showPreviousStepButton={false}
        submitText="Create your own table"
        isSubmitting={isSubmitting}
      />

      {featuredTables.length > 0 && (
        <>
          <h4 className="govuk-heading-m govuk-!-margin-top-9">
            View our featured tables
          </h4>
          <p>
            These featured tables have been created by our publication team,
            highlighting popular tables built from this data set. These tables
            can be viewed, shared and customised to the data that you're
            interested in.
          </p>
        </>
      )}

      {featuredTables.length > 0 && (
        <ChevronGrid testId="featuredTables">
          {featuredTables.map(table => {
            return (
              <ChevronCard
                cardSize="s"
                description={table.description ?? ''}
                key={table.id}
                link={renderFeaturedTableLink?.(table)}
                noBorder
              />
            );
          })}
        </ChevronGrid>
      )}
    </>
  );
}
