import boundaryDataService from '@admin/services/boundaryDataService';
import React, { useCallback } from 'react';
import Page from '@admin/components/Page';
import FormProvider from '@common/components/form/FormProvider';
import FormFieldset from '@common/components/form/FormFieldset';
import FormGroup from '@common/components/form/FormGroup';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import Button from '@common/components/Button';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import {
  Form,
  FormFieldSelect,
  FormFieldTextInput,
} from '@common/components/form';
import { useHistory } from 'react-router-dom';

export default function BoundaryDataUploadPage() {
  interface FormValues {
    boundaryLevel: string;
    boundaryLevelLabel: string;
    boundaryLevelPublishedDate: Date;
    boundaryDataFile: File;
  }

  const history = useHistory();
  const boundaryDataPath = '/administration/boundary-data';

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      const data = {
        level: values.boundaryLevel,
        label: values.boundaryLevelLabel,
        file: values.boundaryDataFile,
        published: values.boundaryLevelPublishedDate,
      };

      await boundaryDataService.uploadBoundaryFile(data);
      history.push(boundaryDataPath);
    },
    [boundaryDataPath, history],
  );

  return (
    <Page
      title="Boundary data"
      caption="Upload boundary data"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Boundary data', link: '/administration/boundary-data' },
        { name: 'Upload boundary data' },
      ]}
    >
      <FormProvider enableReinitialize>
        {({ formState }) => {
          return (
            <Form id="addBoundaryDataForm" onSubmit={handleSubmit}>
              <FormFieldset
                id="boundaryData"
                legend="Upload new boundary data"
                legendSize="l"
                hint="This file will be processed and added to the list of available boundary files on publications."
              >
                <FormGroup>
                  <FormFieldSelect<FormValues>
                    id="boundaryLevel"
                    name="boundaryLevel"
                    label="Select boundary level"
                    className="govuk-!-width-one-third"
                    options={[
                      { value: '', label: '' },
                      {
                        value: 'EnglishDevolvedArea',
                        label: 'EDA - English devolved area',
                      },
                      { value: 'Institution', label: 'INST - Institution' },
                      {
                        value: 'LocalAuthority',
                        label: 'LA - Local authority',
                      },
                      {
                        value: 'LocalAuthorityDistrict',
                        label: 'LAD - Local authority district',
                      },
                      {
                        value: 'LocalEnterprisePartnership',
                        label: 'LEP - Local enterprise partnership',
                      },
                      {
                        value: 'LocalSkillsImprovementPlanArea',
                        label: 'LSIP - Local skills improvement plan area',
                      },
                      {
                        value: 'MayoralCombinedAuthority',
                        label: 'MCA - Mayoral combined authority',
                      },
                      {
                        value: 'MultiAcademyTrust',
                        label: 'MAT - Multi academy trust',
                      },
                      { value: 'Country', label: 'NAT - National' },
                      {
                        value: 'OpportunityArea',
                        label: 'OA - Opportunity area',
                      },
                      { value: 'PlanningArea', label: 'PA - Planning area' },
                      {
                        value: 'ParliamentaryConstituency',
                        label: 'PCON - Parliamentary constituency',
                      },
                      { value: 'Provider', label: 'PROV - Provider' },
                      { value: 'Region', label: 'REG - Regional' },
                      {
                        value: 'RscRegion',
                        label: 'RSC - Regional School Commissioner region',
                      },
                      { value: 'School', label: 'SCH - School' },
                      { value: 'Sponsor', label: 'SPON - Sponsor' },
                      { value: 'Ward', label: 'WARD - Ward' },
                    ]}
                  />

                  <FormFieldTextInput<FormValues>
                    id="boundaryLevelLabel"
                    name="boundaryLevelLabel"
                    label="Label"
                    hint="Boundary level label must be unique"
                    className="govuk-!-width-one-third"
                  />

                  <FormFieldDateInput<FormValues>
                    id="boundaryLevelPublishedDate"
                    name="boundaryLevelPublishedDate"
                    hint="The date the file was released to the public"
                    legend="Published"
                    legendSize="s"
                  />
                </FormGroup>

                <FormFieldFileInput<FormValues>
                  id="boundaryDataFile"
                  name="boundaryDataFile"
                  label="Upload new boundary data file"
                  hint="Must be a GeoJSON file type"
                />
              </FormFieldset>

              <Button disabled={formState.isSubmitting} type="submit">
                Upload file
              </Button>
            </Form>
          );
        }}
      </FormProvider>
    </Page>
  );
}
