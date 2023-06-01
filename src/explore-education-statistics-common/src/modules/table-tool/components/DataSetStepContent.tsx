import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import DataSetDetails from '@common/modules/table-tool/components/DataSetDetails';
import AllFeaturedTables from '@common/modules/table-tool/components/AllFeaturedTables';
import { DataSetFormValues } from '@common/modules/table-tool/components/DataSetStep';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { SelectedRelease } from '@common/modules/table-tool/types/selectedPublication';
import ButtonText from '@common/components/ButtonText';
import { FeaturedTable, Subject } from '@common/services/tableBuilderService';
import React, { ReactNode } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';

interface Props extends InjectedWizardProps {
  featuredTables?: FeaturedTable[];
  isSubmitting: boolean;
  renderFeaturedTableLink?: (featuredTable: FeaturedTable) => ReactNode;
  release?: SelectedRelease;
  subjects: Subject[];
}

export default function DataSetStepContent({
  featuredTables = [],
  isSubmitting,
  renderFeaturedTableLink,
  release,
  subjects,
  ...stepProps
}: Props) {
  const { setValue } = useFormContext<DataSetFormValues>();
  const selectedSubjectId =
    useWatch<DataSetFormValues>({ name: 'subjectId' }) || '';
  const selectedSubject = subjects.find(
    subject => subject.id === selectedSubjectId,
  );

  const screenReaderText = `Showing ${
    selectedSubject
      ? `details for ${selectedSubject.name}`
      : 'all featured tables'
  }`;

  return (
    <div className="govuk-width-container govuk-!-margin-0">
      <VisuallyHidden>
        <p aria-live="polite" aria-atomic>
          {selectedSubjectId ? screenReaderText : ''}
        </p>
      </VisuallyHidden>

      {selectedSubjectId ? (
        <>
          {selectedSubject ? (
            <DataSetDetails
              {...stepProps}
              featuredTables={featuredTables.filter(
                table => table.subjectId === selectedSubject.id,
              )}
              isSubmitting={isSubmitting}
              renderFeaturedTableLink={renderFeaturedTableLink}
              release={release}
              subject={selectedSubject}
            />
          ) : (
            <AllFeaturedTables
              featuredTables={featuredTables}
              renderFeaturedTableLink={renderFeaturedTableLink}
            />
          )}
        </>
      ) : (
        <>
          <p>
            {`Please select a data set, you will then be able to see a summary of
            the data, create your own tables,${
              featuredTables.length > 0 ? ' view featured tables,' : ''
            } or
            download the entire data file.`}
          </p>
          {featuredTables.length > 0 && (
            <>
              <p>
                Alternatively you can browse{' '}
                <ButtonText
                  onClick={() => setValue('subjectId', 'all-featured')}
                >
                  featured tables
                </ButtonText>{' '}
                from across all the data sets for this publication.
              </p>
              <p>
                Featured tables have been created by our publication team,
                highlighting popular tables built from data sets available in
                this publication. These tables can be viewed, shared and
                customised to the data that you're interested in.
              </p>
            </>
          )}
        </>
      )}
    </div>
  );
}
