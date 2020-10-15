import { toolbarConfigs } from '@admin/components/form/FormEditor';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import { MetaGuidanceFormValues } from '@admin/pages/release/data/components/ReleaseMetaGuidanceSection';
import { SubjectMetaGuidance } from '@admin/services/releaseMetaGuidanceService';
import Details from '@common/components/Details';
import SanitizeHtml from '@common/components/SanitizeHtml';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { useField } from 'formik';
import React from 'react';

interface Props {
  formId: string;
  index: number;
  isEditing?: boolean;
  subject: SubjectMetaGuidance;
}

const ReleaseMetaGuidanceDataFile = ({
  formId,
  index,
  isEditing,
  subject,
}: Props) => {
  const { filename, timePeriods, geographicLevels, variables } = subject;

  const [field] = useField(`subjects[${index}].content`);

  return (
    <>
      <SummaryList className="govuk-!-margin-bottom-6">
        <SummaryListItem term="Filename">{filename}</SummaryListItem>
        <SummaryListItem term="Geographic levels">
          {geographicLevels.join('; ')}
        </SummaryListItem>
        <SummaryListItem term="Time period">
          {`${timePeriods.from} to ${timePeriods.to}`}
        </SummaryListItem>
        <SummaryListItem term="Content">
          {isEditing ? (
            <FormFieldEditor<MetaGuidanceFormValues>
              toolbarConfig={toolbarConfigs.simple}
              id={`${formId}-subjects${index}Content`}
              name={`subjects[${index}].content`}
              label="File guidance content"
            />
          ) : (
            <SanitizeHtml
              dirtyHtml={field.value}
              testId="fileGuidanceContent"
            />
          )}
        </SummaryListItem>
      </SummaryList>

      <Details summary="Variable names and descriptions">
        <p>Variable names and descriptions for this file are provided below:</p>

        <table>
          <thead>
            <tr>
              <th scope="col">Variable name</th>
              <th scope="col">Variable description</th>
            </tr>
          </thead>
          <tbody>
            {variables.map(({ value, label }) => (
              <tr key={value}>
                <td>{value}</td>
                <td>{label}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Details>
    </>
  );
};

export default ReleaseMetaGuidanceDataFile;
